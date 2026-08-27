using System;
using HerOClock.Progression;

namespace HerOClock.Characters
{
    /// <summary>
    /// Where a character's level points went, and **when they start counting**.
    ///
    /// Every level grants 5 points. A hero can place them by hand or leave the sheet's
    /// distribution doing it, and can take them all back at any time. Minions and villains are
    /// simply characters left on automatic forever, so there is no second code path for them.
    ///
    /// The two origins are kept apart on purpose:
    ///
    /// - <c>Manual</c> is what the player placed, point by point.
    /// - <c>AutomaticTotal</c> is how many were spent by the sheet's distribution.
    ///
    /// Keeping a count rather than a distributed result is what preserves the rule in
    /// architecture.md that the level contribution is **computed from the level, never
    /// accumulated**. Handing out 5 points thirty-nine times does not give the same split as
    /// handing out 195 once, because the leftover of the percentages is resolved by largest
    /// remainder. A minion created straight at level 40 has to match one that climbed there, and
    /// it only does because the automatic share is recomputed whole every time.
    ///
    /// ## The two splits
    ///
    /// A point being placed and a point taking effect are two different moments, as
    /// attributes.md states under "Um ponto colocado só passa a valer na próxima fase". So there
    /// are two splits here and they are not interchangeable:
    ///
    /// - <c>edited</c> is what the player has been doing. The profile shows it and the save
    ///   writes it down, because it is the choice that was made.
    /// - <c>inEffect</c> is what the stats are allowed to read, through <see cref="WriteTo"/>,
    ///   which is the only member that touches it.
    ///
    /// Nothing moves from the first to the second except <see cref="Commit"/>, called when a
    /// stage begins. **That is the whole rule**, and it has no exception: the automatic
    /// distribution waits exactly like the player does. It decides *where* a point goes without
    /// deciding *when* it counts.
    ///
    /// Without it, taking points back being free and instant means a player could rebuild
    /// mid fight — pile on CON while being hit, pile on POW for the killing blow — and meet every
    /// enemy with the build made for that enemy. A choice that can be undone at any moment stops
    /// being a choice.
    ///
    /// It also removes a whole class of problem rather than managing it: maximum health comes
    /// only from CON, so with points frozen inside a stage the maximum cannot rise while a
    /// character is wounded, and cannot fall out from under a character's current health.
    /// </summary>
    public class AttributeAllocation
    {
        /// <summary>What the player has been editing. The profile and the save read this.</summary>
        private readonly AttributeSplit edited = new AttributeSplit();

        /// <summary>What the stats are allowed to read. Only <see cref="Commit"/> writes it.</summary>
        private readonly AttributeSplit inEffect = new AttributeSplit();

        private readonly int[] automatic = new int[4];

        private AttributeGrowth growth;

        /// <summary>Raised whenever the split changes, so the character can rebuild its stats.</summary>
        public event Action Changed;

        public AttributeAllocation(AttributeGrowth growth)
        {
            this.growth = growth;
        }

        /// <summary>While true, points are spent by the sheet's distribution the moment they arrive.</summary>
        public bool IsAutomatic
        {
            get { return edited.IsAutomatic; }
        }

        /// <summary>Points earned and not yet placed. Always zero while on automatic.</summary>
        public int Unspent
        {
            get { return edited.Unspent; }
        }

        /// <summary>Everything this character has ever been granted, placed or not.</summary>
        public int Granted
        {
            get { return edited.Granted; }
        }

        /// <summary>
        /// How many points ended up on an attribute, counting both origins.
        ///
        /// This is the profile's number, so it answers for what the player placed and not for
        /// what is currently driving the stats. Those two disagree between a point being placed
        /// and the next stage starting, and the profile is meant to show the choice.
        /// </summary>
        public int SpentOn(Attribute attribute)
        {
            AttributeGrowth.Distribute(edited.AutomaticTotal, growth, automatic);
            return edited.Manual[(int)attribute] + automatic[(int)attribute];
        }

        /// <summary>How many points the player placed on an attribute by hand.</summary>
        public int ManualOn(Attribute attribute)
        {
            return edited.Manual[(int)attribute];
        }

        /// <summary>
        /// How many points the sheet's distribution has spent.
        ///
        /// This is the number a save records, and recording the **count** rather than the split it
        /// produces is not a detail. The split is resolved by largest remainder, so handing out five
        /// points thirty-nine times does not give what handing out 195 once gives. Write the split
        /// down and a restored character stops matching one that climbed to the same level while
        /// playing, which quietly breaks replaying a battle from a seed.
        /// </summary>
        public int AutomaticPoints
        {
            get { return edited.AutomaticTotal; }
        }

        /// <summary>
        /// Brings the allocation up to the given level.
        ///
        /// It works from the total the level is worth rather than from a delta, so calling it for
        /// level 40 on a fresh allocation gives exactly what forty separate level ups would.
        ///
        /// Like everything else here it lands on the edited split only. A level gained mid stage
        /// credits its points and places them, and the player sees that in the profile, but the
        /// attributes do not move until the next stage begins.
        /// </summary>
        public void GrantFor(int level)
        {
            int owed = LevelProgress.PointsAtLevel(level) - edited.TotalManual - edited.AutomaticTotal;

            if (owed <= 0)
            {
                return;
            }

            if (edited.IsAutomatic)
            {
                edited.AutomaticTotal += owed;
                edited.Unspent = 0;
            }
            else
            {
                edited.Unspent = owed;
            }

            Raise();
        }

        /// <summary>
        /// Places points on an attribute by hand. Refuses anything the character cannot pay for,
        /// so the total placed can never drift away from what the level granted.
        /// </summary>
        public bool Spend(Attribute attribute, int amount)
        {
            if (amount <= 0 || amount > edited.Unspent)
            {
                return false;
            }

            edited.Manual[(int)attribute] += amount;
            edited.Unspent -= amount;

            Raise();
            return true;
        }

        /// <summary>
        /// Turns the automatic distribution on or off.
        ///
        /// Turning it on only spends what was sitting unspent. It deliberately leaves alone what
        /// the player placed by hand: wiping a build with one toggle is what the reset button is
        /// for, and it should take a deliberate press.
        /// </summary>
        public void SetAutomatic(bool value)
        {
            if (edited.IsAutomatic == value)
            {
                return;
            }

            edited.IsAutomatic = value;

            if (edited.IsAutomatic && edited.Unspent > 0)
            {
                edited.AutomaticTotal += edited.Unspent;
                edited.Unspent = 0;
            }

            Raise();
        }

        /// <summary>
        /// Takes every point back.
        ///
        /// With automatic on they go straight back out by the sheet's distribution, which is how
        /// "return to the recommended build" falls out of the same rule. With it off, everything
        /// is freed for the player to place again.
        /// </summary>
        public void Reset()
        {
            int total = edited.Granted;

            for (int i = 0; i < 4; i++)
            {
                edited.Manual[i] = 0;
            }

            if (edited.IsAutomatic)
            {
                edited.AutomaticTotal = total;
                edited.Unspent = 0;
            }
            else
            {
                edited.AutomaticTotal = 0;
                edited.Unspent = total;
            }

            Raise();
        }

        /// <summary>
        /// Puts back an allocation a save held.
        ///
        /// It restores the two origins separately, exactly as they were stored, and never a
        /// finished split. Nothing is validated against a level here, because this class has never
        /// known what level the character is: whoever loads calls <see cref="GrantFor"/> straight
        /// after, and that is what tops up any points a save is short of.
        ///
        /// Only the edited split is written, and that is right rather than an oversight. Loading
        /// is followed by a stage starting, which commits — so a rebuild left pending when the
        /// game was closed lands exactly where it would have landed had the player kept playing.
        /// </summary>
        public void Restore(bool automatic, int automaticPoints, int unspentPoints, int[] manualPoints)
        {
            edited.IsAutomatic = automatic;
            edited.AutomaticTotal = Math.Max(0, automaticPoints);
            edited.Unspent = Math.Max(0, unspentPoints);

            for (int i = 0; i < 4; i++)
            {
                edited.Manual[i] = manualPoints != null && i < manualPoints.Length
                    ? Math.Max(0, manualPoints[i])
                    : 0;
            }

            // On automatic there is nothing waiting to be placed, by definition. A file claiming
            // both would otherwise leave the character permanently owing itself points.
            if (edited.IsAutomatic && edited.Unspent > 0)
            {
                edited.AutomaticTotal += edited.Unspent;
                edited.Unspent = 0;
            }

            Raise();
        }

        /// <summary>
        /// Puts everything placed since the last stage into effect. **The only way points ever
        /// start counting.**
        ///
        /// Called when a character enters a stage, which covers the first attempt, the restart
        /// after a defeat, advancing after a win, and reopening the game — the same moment the
        /// party itself is composed, as gameplay.md describes.
        ///
        /// It must run before anything reads the stats for that stage. In particular it runs
        /// before health is put back, since the maximum it produces is the one that gets filled.
        /// </summary>
        public void Commit()
        {
            inEffect.CopyFrom(edited);
            Raise();
        }

        /// <summary>
        /// Copies the resulting split into a four slot array, in attribute order.
        ///
        /// The only member that reads what is in effect, which is what makes this the single
        /// door between a point being placed and a point counting.
        /// </summary>
        public void WriteTo(int[] result)
        {
            // The automatic share is always redistributed whole, never added to. That is the part
            // that keeps a character's attributes a pure function of its level.
            AttributeGrowth.Distribute(inEffect.AutomaticTotal, growth, automatic);

            for (int i = 0; i < 4; i++)
            {
                result[i] = inEffect.Manual[i] + automatic[i];
            }
        }

        private void Raise()
        {
            Changed?.Invoke();
        }
    }
}
