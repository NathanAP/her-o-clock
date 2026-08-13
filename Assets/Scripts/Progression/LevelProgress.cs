using System;

namespace HerOClock.Progression
{
    /// <summary>
    /// The level and accumulated experience of one character instance.
    ///
    /// Pure C#, with no dependency on Unity, so the whole thing can be tested outside the editor.
    /// </summary>
    public class LevelProgress
    {
        private readonly int maxLevel;

        public LevelProgress(int startingLevel, int maxLevel)
        {
            this.maxLevel = Math.Max(1, maxLevel);
            Level = Math.Min(Math.Max(1, startingLevel), this.maxLevel);
        }

        public int Level { get; private set; }

        /// <summary>Experience accumulated towards the next level.</summary>
        public long CurrentXp { get; private set; }

        /// <summary>Attribute points earned and not yet spent.</summary>
        public int UnspentAttributePoints { get; private set; }

        /// <summary>Skill tree points earned. Nothing spends them yet.</summary>
        public int SkillPoints { get; private set; }

        public bool IsMaxLevel
        {
            get { return Level >= maxLevel; }
        }

        /// <summary>Raised once per level gained, so the character can react.</summary>
        public event Action<int> LevelGained;

        /// <summary>
        /// Adds experience and applies every level it earns.
        ///
        /// More than one level at a time is the normal case, not the exception: a level 1 hero
        /// joining a group that farms level 40 gains dozens of levels from a single enemy.
        /// </summary>
        public void Award(long xp)
        {
            if (xp <= 0 || IsMaxLevel)
            {
                return;
            }

            CurrentXp += xp;

            while (!IsMaxLevel)
            {
                long needed = ExperienceTable.XpToNextLevel(Level);

                if (needed <= 0 || CurrentXp < needed)
                {
                    break;
                }

                CurrentXp -= needed;
                Level++;

                UnspentAttributePoints += PointsPerLevel;
                SkillPoints += SkillPointsPerLevel;

                LevelGained?.Invoke(Level);
            }

            if (IsMaxLevel)
            {
                // Experience past the last level has nowhere to go.
                CurrentXp = 0;
            }
        }

        /// <summary>Attribute points granted on every level, as stated in progress.md.</summary>
        public const int PointsPerLevel = 5;

        /// <summary>Skill tree points granted on every level.</summary>
        public const int SkillPointsPerLevel = 1;

        /// <summary>
        /// Total attribute points a character of the given level has received.
        /// Level 1 has received none, which is why the starting level is subtracted.
        /// </summary>
        public static int PointsAtLevel(int level)
        {
            return level <= 1 ? 0 : (level - 1) * PointsPerLevel;
        }
    }
}
