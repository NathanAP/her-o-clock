using System;
using HerOClock.Progression;

namespace HerOClock.Characters
{
    /// <summary>
    /// Where a character's level points went.
    ///
    /// Every level grants 5 points. A hero can place them by hand or leave the sheet's
    /// distribution doing it, and can take them all back at any time. Minions and villains are
    /// simply characters left on automatic forever, so there is no second code path for them.
    ///
    /// The two origins are kept apart on purpose:
    ///
    /// - <c>manual</c> is what the player placed, point by point.
    /// - <c>automaticTotal</c> is how many were spent by the sheet's distribution.
    ///
    /// Keeping a count rather than a distributed result is what preserves the rule in
    /// architecture.md that the level contribution is **computed from the level, never
    /// accumulated**. Handing out 5 points thirty-nine times does not give the same split as
    /// handing out 195 once, because the leftover of the percentages is resolved by largest
    /// remainder. A minion created straight at level 40 has to match one that climbed there, and
    /// it only does because the automatic share is recomputed whole every time.
    ///
    /// ## Nothing here waits, and it does not have to
    ///
    /// Every method takes effect the instant it is called, which looks like it contradicts
    /// attributes.md — "um ponto colocado só passa a valer na próxima fase" — and does not.
    ///
    /// This lives on a <see cref="HeroRecord"/>, and **no fight ever reads a record**. A combatant
    /// copies the points once, when a stage builds it, and fights with that copy. So a player can
    /// rebuild in the middle of a battle and the battle simply does not notice: the wait is a
    /// consequence of where this object sits, not a mechanism it has to implement.
    ///
    /// 0.10.3.0 did implement it here, as a second split held back until a commit. 0.10.4.0
    /// deleted that, because separating the record from the combatant gives the same rule for
    /// free — and gives it to equipment and everything else later, without any of them asking.
    /// </summary>
    public class AttributeAllocation
    {
        private readonly int[] manual = new int[4];
        private readonly int[] automatic = new int[4];

        private AttributeGrowth growth;
        private int automaticTotal;
        private int unspent;
        private bool isAutomatic = true;

        /// <summary>Raised whenever the split changes, so the character can rebuild its stats.</summary>
        public event Action Changed;

        public AttributeAllocation(AttributeGrowth growth)
        {
            this.growth = growth;
        }

        /// <summary>While true, points are spent by the sheet's distribution the moment they arrive.</summary>
        public bool IsAutomatic
        {
            get { return isAutomatic; }
        }

        /// <summary>Points earned and not yet placed. Always zero while on automatic.</summary>
        public int Unspent
        {
            get { return unspent; }
        }

        /// <summary>Everything this character has ever been granted, placed or not.</summary>
        public int Granted
        {
            get { return TotalManual() + automaticTotal + unspent; }
        }

        /// <summary>How many points ended up on an attribute, counting both origins.</summary>
        public int SpentOn(Attribute attribute)
        {
            RebuildAutomatic();
            return manual[(int)attribute] + automatic[(int)attribute];
        }

        /// <summary>How many points the player placed on an attribute by hand.</summary>
        public int ManualOn(Attribute attribute)
        {
            return manual[(int)attribute];
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
            get { return automaticTotal; }
        }

        /// <summary>
        /// Brings the allocation up to the given level.
        ///
        /// It works from the total the level is worth rather than from a delta, so calling it for
        /// level 40 on a fresh allocation gives exactly what forty separate level ups would.
        /// </summary>
        public void GrantFor(int level)
        {
            int owed = LevelProgress.PointsAtLevel(level) - TotalManual() - automaticTotal;

            if (owed <= 0)
            {
                return;
            }

            if (isAutomatic)
            {
                automaticTotal += owed;
                unspent = 0;
            }
            else
            {
                unspent = owed;
            }

            Raise();
        }

        /// <summary>
        /// Places points on an attribute by hand. Refuses anything the character cannot pay for,
        /// so the total placed can never drift away from what the level granted.
        /// </summary>
        public bool Spend(Attribute attribute, int amount)
        {
            if (amount <= 0 || amount > unspent)
            {
                return false;
            }

            manual[(int)attribute] += amount;
            unspent -= amount;

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
            if (isAutomatic == value)
            {
                return;
            }

            isAutomatic = value;

            if (isAutomatic && unspent > 0)
            {
                automaticTotal += unspent;
                unspent = 0;
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
            int total = Granted;

            for (int i = 0; i < 4; i++)
            {
                manual[i] = 0;
            }

            if (isAutomatic)
            {
                automaticTotal = total;
                unspent = 0;
            }
            else
            {
                automaticTotal = 0;
                unspent = total;
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
        /// </summary>
        public void Restore(bool automatic, int automaticPoints, int unspentPoints, int[] manualPoints)
        {
            isAutomatic = automatic;
            automaticTotal = Math.Max(0, automaticPoints);
            unspent = Math.Max(0, unspentPoints);

            for (int i = 0; i < 4; i++)
            {
                manual[i] = manualPoints != null && i < manualPoints.Length
                    ? Math.Max(0, manualPoints[i])
                    : 0;
            }

            // On automatic there is nothing waiting to be placed, by definition. A file claiming
            // both would otherwise leave the character permanently owing itself points.
            if (isAutomatic && unspent > 0)
            {
                automaticTotal += unspent;
                unspent = 0;
            }

            Raise();
        }

        /// <summary>Copies the resulting split into a four slot array, in attribute order.</summary>
        public void WriteTo(int[] result)
        {
            RebuildAutomatic();

            for (int i = 0; i < 4; i++)
            {
                result[i] = manual[i] + automatic[i];
            }
        }

        /// <summary>
        /// The automatic share is always redistributed whole, never added to. That is the part
        /// that keeps a character's attributes a pure function of its level.
        /// </summary>
        private void RebuildAutomatic()
        {
            AttributeGrowth.Distribute(automaticTotal, growth, automatic);
        }

        private int TotalManual()
        {
            return manual[0] + manual[1] + manual[2] + manual[3];
        }

        private void Raise()
        {
            Changed?.Invoke();
        }
    }
}
