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

        /// <summary>
        /// Skill tree points earned. Nothing spends them yet, and that is fine: the skill tree
        /// does not exist. Attribute points used to sit here in the same shape, but they were
        /// not the same case — they were also being spent automatically at the same time, so the
        /// counter and the spending were two answers to the same question. They now live in the
        /// character's <see cref="Characters.AttributeAllocation"/>, which owns both sides.
        /// </summary>
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

                SkillPoints += SkillPointsPerLevel;

                LevelGained?.Invoke(Level);
            }

            if (IsMaxLevel)
            {
                // Experience past the last level has nowhere to go.
                CurrentXp = 0;
            }
        }

        /// <summary>
        /// Puts back the level and experience a save held.
        ///
        /// It deliberately does **not** raise <see cref="LevelGained"/>. Loading is not levelling
        /// up: the event exists so a character can react to earning a level, and firing it dozens of
        /// times on startup would hand out the same points the save already recorded.
        ///
        /// Whoever restores is responsible for bringing the attribute allocation up to this level
        /// afterwards, which is what reconciles a save whose numbers do not add up.
        /// </summary>
        public void Restore(int level, long currentXp, int skillPoints)
        {
            Level = Math.Min(Math.Max(1, level), maxLevel);
            SkillPoints = Math.Max(0, skillPoints);

            if (IsMaxLevel)
            {
                // Same rule as earning it: experience past the last level has nowhere to go.
                CurrentXp = 0;
                return;
            }

            long needed = ExperienceTable.XpToNextLevel(Level);
            CurrentXp = Math.Min(Math.Max(0L, currentXp), Math.Max(0L, needed - 1));
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
