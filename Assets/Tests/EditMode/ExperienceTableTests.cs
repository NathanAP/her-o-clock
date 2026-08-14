using System;
using HerOClock.Progression;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks the two formulas of progress.md and the rules built on top of them.
    ///
    /// The expected values are recomputed here from the formulas as the spec writes them, rather
    /// than pasted from a run, so a changed constant shows up as a disagreement between the spec
    /// and the code instead of as a silently updated number.
    /// </summary>
    public class ExperienceTableTests
    {
        // xpParaSubir(nivel) = 50 x nivel^3.55
        private static long ExpectedXpToNextLevel(int level)
        {
            return (long)Math.Round(50.0 * Math.Pow(level, 3.55));
        }

        // xpPorLacaio(nivel) = 10 x nivel^2
        private static long ExpectedXpFromMinion(int level)
        {
            return (long)Math.Round(10.0 * Math.Pow(level, 2.0));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(99)]
        public void XpToNextLevel_FollowsTheSpecFormula(int level)
        {
            Assert.AreEqual(ExpectedXpToNextLevel(level), ExperienceTable.XpToNextLevel(level));
        }

        [TestCase(1)]
        [TestCase(12)]
        [TestCase(40)]
        [TestCase(100)]
        public void XpFromMinion_FollowsTheSpecFormula(int level)
        {
            Assert.AreEqual(ExpectedXpFromMinion(level), ExperienceTable.XpFromMinion(level));
        }

        /// <summary>A level 1 minion is worth 10, which is the anchor of the whole curve.</summary>
        [Test]
        public void ALevelOneMinionIsWorthTen()
        {
            Assert.AreEqual(10, ExperienceTable.XpFromMinion(1));
        }

        /// <summary>Going from level 1 to 2 costs 50, the other anchor.</summary>
        [Test]
        public void TheFirstLevelCostsFifty()
        {
            Assert.AreEqual(50, ExperienceTable.XpToNextLevel(1));
        }

        // --- A villain is worth ten minions of the same level, in both currencies ---

        [TestCase(1)]
        [TestCase(12)]
        [TestCase(60)]
        public void AVillainIsWorthTenMinions(int level)
        {
            Assert.AreEqual(ExperienceTable.XpFromMinion(level) * 10, ExperienceTable.XpFromVillain(level));
            Assert.AreEqual(ExperienceTable.MoneyFromMinion(level) * 10, ExperienceTable.MoneyFromVillain(level));
        }

        [Test]
        public void AMinionIsWorthOneMoneyRegardlessOfLevel()
        {
            Assert.AreEqual(1, ExperienceTable.MoneyFromMinion(1));
            Assert.AreEqual(1, ExperienceTable.MoneyFromMinion(80));
        }

        // --- Edges ---

        [Test]
        public void ThereIsNothingToEarnPastTheLastLevel()
        {
            Assert.AreEqual(0, ExperienceTable.XpToNextLevel(ExperienceTable.MaxLevel));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void InvalidLevelsAreWorthNothing(int level)
        {
            Assert.AreEqual(0, ExperienceTable.XpToNextLevel(level));
            Assert.AreEqual(0, ExperienceTable.XpFromMinion(level));
            Assert.AreEqual(0, ExperienceTable.MoneyFromMinion(level));
        }

        [Test]
        public void MaxLevelIsOneHundred()
        {
            Assert.AreEqual(100, ExperienceTable.MaxLevel);
        }

        // --- The shape of the curve ---

        /// <summary>
        /// What controls the pace of the game is the gap between the two exponents, because it
        /// decides how many enemies of your own level a level costs.
        ///
        /// Dividing one formula by the other gives 5 x level^1.55, and the expectation is written
        /// as that algebra rather than as numbers worked out by hand. Both formulas have to keep
        /// agreeing with the shape the spec chose, and the difference of the exponents is the
        /// thing that must not drift.
        /// </summary>
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(99)]
        public void EnemiesNeededPerLevel_IsFiveTimesLevelToTheOnePointFiveFive(int level)
        {
            double expected = 5.0 * Math.Pow(level, 3.55 - 2.0);
            double actual = (double)ExperienceTable.XpToNextLevel(level) / ExperienceTable.XpFromMinion(level);

            Assert.AreEqual(expected, actual, expected * 0.001);
        }

        /// <summary>
        /// The count has to keep growing, otherwise there would be a stretch of the game where
        /// levelling gets cheaper as you go.
        /// </summary>
        [Test]
        public void EnemiesNeededPerLevel_NeverStopsGrowing()
        {
            double previous = 0;

            for (int level = 1; level < ExperienceTable.MaxLevel; level++)
            {
                double enemies = (double)ExperienceTable.XpToNextLevel(level) / ExperienceTable.XpFromMinion(level);

                Assert.Greater(enemies, previous, "Level " + level + " costs fewer enemies than the one before.");
                previous = enemies;
            }
        }
    }
}
