using System;

namespace HerOClock.Stages
{
    /// <summary>
    /// A stage, mirroring the shape of its JSON file.
    ///
    /// Field names are lowercase because JsonUtility matches them against the JSON keys
    /// character for character. This class is a wire format, not a normal C# type.
    /// </summary>
    [Serializable]
    public class StageData
    {
        /// <summary>
        /// Stable text id of the stage. Save games refer to it, and so does the strings file:
        /// the name and the short piece of fiction shown when the stage starts are read from
        /// <c>stage.{id}.name</c> and <c>stage.{id}.lore</c>.
        ///
        /// No text the player reads lives in this file, so a translation pass and a balance pass
        /// never touch the same lines.
        /// </summary>
        public string id;

        /// <summary>
        /// Level of every minion and villain in this stage, overriding whatever their sheets
        /// say. It is what lets the same minion be reused across acts at different strengths.
        /// </summary>
        public int enemyLevel = 1;

        /// <summary>
        /// Story characters that fight beside the party in this stage.
        ///
        /// They are placed once when the stage begins and **stay across every wave**, the same way
        /// the heroes do, rather than being spawned and cleared with each group of minions. They
        /// are a separate field from the waves because they belong to the hero side.
        ///
        /// An NPC never counts towards defeat: the stage is lost when the party falls.
        /// </summary>
        public StagePlacement[] allies;

        /// <summary>
        /// How many heroes this stage accepts, taken from the front of the player's team.
        ///
        /// A rule of the stage, not of the party: replaying an early stage in a later run still
        /// enters with its own limit, however big the team has grown. Zero means no limit.
        /// </summary>
        public int heroLimit;

        /// <summary>What clearing this stage for the first time hands out. Never happens twice.</summary>
        public StageFirstClear firstClear;

        /// <summary>Minion groups, fought in order.</summary>
        public StageWave[] waves;

        /// <summary>
        /// The final fight. Kept as its own field rather than as the last entry of waves, so
        /// the structure itself enforces the rule that a stage always ends against a villain.
        /// </summary>
        public StageWave villainWave;
    }

    /// <summary>
    /// Rewards granted the first time a stage is cleared, and never again.
    ///
    /// They live in the stage file rather than in code so that a rule of the story is content. A
    /// hero arriving and a team position opening are separate fields on purpose: they often happen
    /// together, and they are not the same thing.
    /// </summary>
    [Serializable]
    public class StageFirstClear
    {
        /// <summary>Id of a hero the player meets here. Empty for none.</summary>
        public string unlocksCharacter;

        /// <summary>How many team positions open up here.</summary>
        public int grantsTeamSlots;
    }

    [Serializable]
    public class StageWave
    {
        public StagePlacement[] placements;
    }

    [Serializable]
    public class StagePlacement
    {
        /// <summary>Id of the character sheet, resolved through the CharacterDatabase.</summary>
        public string character;

        public int column;
        public int row;

        /// <summary>
        /// Fine tuning for this single enemy, multiplying its attributes.
        ///
        /// The sheet says who the character is and the stage says how strong this instance is.
        /// It is what lets the same minion appear weakened as a summon in a late stage, or a
        /// single stage be softened without touching any other.
        ///
        /// Left out of the file it reads as 1, since a missing number arrives as zero.
        /// </summary>
        public float multiplier;

        /// <summary>
        /// Level of this one character, overriding the stage's <see cref="StageData.enemyLevel"/>.
        ///
        /// For somebody who has to be weaker or stronger than the rest of the wave for a reason of
        /// story rather than of fine tuning. It is not the same as the multiplier: the level moves
        /// mitigation, the experience granted and the whole growth of the character, while the
        /// multiplier only scales attributes that were already worked out.
        ///
        /// Left out of the file it reads as 0, which means "use the stage's level".
        /// </summary>
        public int level;

        /// <summary>
        /// How much health this character walks in with, as a percentage of its maximum.
        ///
        /// For somebody who arrives already wounded, without their maximum health changing. They
        /// are still who they are; they have just been hit already.
        ///
        /// Left out of the file it reads as 0, which means full health.
        /// </summary>
        public int startingHealthPercent;

        public float EffectiveMultiplier
        {
            get { return multiplier <= 0f ? 1f : multiplier; }
        }

        /// <summary>The stage's level unless this placement declared one of its own.</summary>
        public int EffectiveLevel(int stageLevel)
        {
            return level > 0 ? level : stageLevel;
        }

        /// <summary>Full health unless this placement declared otherwise.</summary>
        public int EffectiveStartingHealthPercent
        {
            get { return startingHealthPercent > 0 ? startingHealthPercent : 100; }
        }
    }
}
