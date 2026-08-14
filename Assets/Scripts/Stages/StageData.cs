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

        /// <summary>Minion groups, fought in order.</summary>
        public StageWave[] waves;

        /// <summary>
        /// The final fight. Kept as its own field rather than as the last entry of waves, so
        /// the structure itself enforces the rule that a stage always ends against a villain.
        /// </summary>
        public StageWave villainWave;
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

        public float EffectiveMultiplier
        {
            get { return multiplier <= 0f ? 1f : multiplier; }
        }
    }
}
