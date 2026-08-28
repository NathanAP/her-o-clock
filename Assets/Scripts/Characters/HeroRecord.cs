using HerOClock.Progression;

namespace HerOClock.Characters
{
    /// <summary>
    /// Everything the game knows about one hero **between stages**: the level it reached, the
    /// experience towards the next, the skill points it has not spent, and where its attribute
    /// points went.
    ///
    /// ## Why this is not the Character
    ///
    /// There are three layers, and each one changes on its own clock:
    ///
    /// - the **sheet** (<see cref="CharacterDefinition"/>) is a shared asset and never changes;
    /// - the **record**, here, is what the player builds up across a whole save;
    /// - the **combatant** (<see cref="Character"/>) is built from a record when a stage begins
    ///   and thrown away when it ends.
    ///
    /// Until 0.10.4.0 the last two were one object, and that fusion was the source of a whole
    /// family of problems that all sounded like "what happens if this changes mid stage?":
    ///
    /// - the level lived in two places and could disagree with itself, which is what 0.10.2.4 was;
    /// - attribute points had to be held back by a mechanism of their own so a player could not
    ///   rebuild during a fight, which is what 0.10.3.0 was;
    /// - maximum health could rise while a character was wounded, or fall under its current
    ///   health, and each needed its own rule;
    /// - a hand written list of state had to be cleared between stages, and it was already
    ///   incomplete — the regeneration carry crossed from one stage into the next unnoticed.
    ///
    /// None of those are handled here. **They stop existing**, because a combatant is a
    /// photograph: it is taken when the stage starts and nothing develops it afterwards. A record
    /// can be levelled, rebuilt and later re-equipped at any moment with no fear of corrupting a
    /// fight in progress, because the fight is not reading it.
    ///
    /// Minions, villains and NPCs have no record. They were always built per stage and thrown
    /// away, and heroes were the exception. Now nobody is.
    /// </summary>
    public class HeroRecord
    {
        /// <summary>The sheet this hero is an instance of.</summary>
        public CharacterDefinition Definition { get; private set; }

        /// <summary>Level, experience and skill points. The only place the level lives.</summary>
        public LevelProgress Progress { get; private set; }

        /// <summary>Where the level points went. Freely editable — no fight is reading it.</summary>
        public AttributeAllocation Attributes { get; private set; }

        /// <summary>
        /// What this hero is wearing.
        ///
        /// It lives on the record for the same reason the attribute points do: a player changing
        /// equipment mid stage is changing the hero, not the thing on the board. The combatant took
        /// its photograph when the stage began and never looks here again.
        /// </summary>
        public Items.Equipment Equipment { get; private set; }

        public HeroRecord(CharacterDefinition definition)
        {
            Definition = definition;

            int startingLevel = Clamp(definition.Level, definition.MaxLevel);

            Progress = new LevelProgress(startingLevel, definition.MaxLevel);
            Progress.LevelGained += OnLevelGained;

            Attributes = new AttributeAllocation(definition.Growth);
            Attributes.GrantFor(startingLevel);

            Equipment = new Items.Equipment();
        }

        public string Id
        {
            get { return Definition.Id; }
        }

        /// <summary>The level this hero really is, which is the one a new combatant is built at.</summary>
        public int Level
        {
            get { return Progress.Level; }
        }

        /// <summary>
        /// Adds experience, applying every level it earns.
        ///
        /// Safe to call in the middle of a stage, and that is the point. Whatever it changes here
        /// reaches the board when the next stage builds its combatants, and not before.
        /// </summary>
        public void AwardExperience(long amount)
        {
            Progress.Award(amount);
        }

        /// <summary>
        /// Puts a saved level, experience and skill points back.
        ///
        /// The level is read back from <see cref="Progress"/> rather than reused, because Progress
        /// clamps to 1 and to the maximum, and the points have to be topped up against the clamped
        /// one.
        /// </summary>
        public void Restore(int savedLevel, long currentXp, int skillPoints)
        {
            Progress.Restore(savedLevel, currentXp, skillPoints);
        }

        /// <summary>
        /// The attribute points this hero fights with, in attribute order.
        ///
        /// Read once, when a combatant is built. From then on the combatant holds its own copy and
        /// this can move underneath it without the fight noticing.
        /// </summary>
        public void WritePointsTo(int[] result)
        {
            Attributes.WriteTo(result);
        }

        private void OnLevelGained(int newLevel)
        {
            Attributes.GrantFor(newLevel);
        }

        private static int Clamp(int level, int maxLevel)
        {
            return level < 1 ? 1 : level > maxLevel ? maxLevel : level;
        }
    }
}
