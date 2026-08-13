using System;

namespace HerOClock.Progression
{
    /// <summary>
    /// The experience and money an enemy is worth, and what a level costs.
    ///
    /// The formulas come from progress.md. What controls the pace of the whole game is not the
    /// constants but the gap between the two exponents, since it decides how many enemies of a
    /// character's own level are needed to gain a level, and how that number grows.
    /// </summary>
    public static class ExperienceTable
    {
        /// <summary>Highest level any character can reach.</summary>
        public const int MaxLevel = 100;

        private const double LevelConstant = 50.0;
        private const double LevelExponent = 3.55;

        private const double MinionConstant = 10.0;
        private const double MinionExponent = 2.0;

        /// <summary>A villain is worth this many minions of the same level.</summary>
        public const int VillainMultiplier = 10;

        /// <summary>
        /// Money a minion is worth. Deliberately a flat placeholder: the right number only exists
        /// once the progress tree exists, because it is the cost curve of that tree which decides
        /// how much money has to come in. See progress.md.
        /// </summary>
        public const int MoneyPerMinion = 1;

        /// <summary>Experience needed to go from the given level to the next one.</summary>
        public static long XpToNextLevel(int level)
        {
            if (level < 1 || level >= MaxLevel)
            {
                return 0;
            }

            return (long)Math.Round(LevelConstant * Math.Pow(level, LevelExponent));
        }

        /// <summary>Experience granted by a minion of the given level.</summary>
        public static long XpFromMinion(int enemyLevel)
        {
            if (enemyLevel < 1)
            {
                return 0;
            }

            return (long)Math.Round(MinionConstant * Math.Pow(enemyLevel, MinionExponent));
        }

        /// <summary>Experience granted by a villain of the given level.</summary>
        public static long XpFromVillain(int enemyLevel)
        {
            return XpFromMinion(enemyLevel) * VillainMultiplier;
        }

        public static int MoneyFromMinion(int enemyLevel)
        {
            return enemyLevel < 1 ? 0 : MoneyPerMinion;
        }

        public static int MoneyFromVillain(int enemyLevel)
        {
            return MoneyFromMinion(enemyLevel) * VillainMultiplier;
        }

        /// <summary>
        /// Total experience needed to go from level 1 to the given level.
        ///
        /// Nothing calls this yet. It exists for the test that recalculates the pacing table
        /// published in progress.md, so a constant cannot be changed without the spec and the
        /// code disagreeing out loud. That test arrives in 0.5.2.0.
        /// </summary>
        public static long TotalXpTo(int level)
        {
            long total = 0;

            for (int l = 1; l < level; l++)
            {
                total += XpToNextLevel(l);
            }

            return total;
        }
    }
}
