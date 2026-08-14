using HerOClock.Progression;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Keeps the pacing numbers published in progress.md honest.
    ///
    /// These are the claims that justified choosing 3.55 as the experience exponent, and they are
    /// the easiest thing in the project to invalidate by accident: any change to either formula
    /// moves all of them at once, and nothing on screen would look wrong.
    ///
    /// The model is the one the spec states: the player farms enemies of their own level at
    /// roughly 11 enemies per minute. The tolerances are wide because the claim is "this is a
    /// matter of hundreds of hours", not "this is exactly 366 hours". They are not wide enough to
    /// survive a changed exponent.
    /// </summary>
    public class PacingTests
    {
        /// <summary>Enemies defeated per minute of open game, as stated in progress.md.</summary>
        private const double EnemiesPerMinute = 11.0;

        /// <summary>
        /// Hours of open game needed to go from level 1 to the given level, farming enemies of
        /// the player's own level the whole way.
        /// </summary>
        private static double HoursToReach(int level)
        {
            double minutes = 0;

            for (int current = 1; current < level; current++)
            {
                double enemies = (double)ExperienceTable.XpToNextLevel(current) / ExperienceTable.XpFromMinion(current);
                minutes += enemies / EnemiesPerMinute;
            }

            return minutes / 60.0;
        }

        /// <summary>
        /// progress.md: "com 3.55, as mesmas 250 horas levam ao nível 86 de 100". This is the
        /// calibration against Task Bar Hero and the reason the exponent is what it is.
        /// </summary>
        [Test]
        public void TwoHundredAndFiftyHoursReachLevelEightySix()
        {
            Assert.AreEqual(250.0, HoursToReach(86), 25.0);
        }

        /// <summary>
        /// progress.md: reaching level 100 at the normal pace takes about 366 hours.
        /// </summary>
        [Test]
        public void ReachingTheLastLevelTakesAboutThreeHundredAndSixtySixHours()
        {
            Assert.AreEqual(366.0, HoursToReach(100), 40.0);
        }

        /// <summary>
        /// progress.md: "os últimos 14 níveis custam mais de 100 horas sozinhos. O nível 100
        /// existe, mas ninguém precisa alcançá-lo, que é o comportamento desejado."
        /// </summary>
        [Test]
        public void TheLastFourteenLevelsCostMoreThanOneHundredHours()
        {
            Assert.Greater(HoursToReach(100) - HoursToReach(86), 100.0);
        }

        /// <summary>
        /// The early game has to be quick, otherwise nobody reaches the part that is slow on
        /// purpose. A handful of levels in the first minutes is the intent.
        /// </summary>
        [Test]
        public void TheFirstLevelsArriveInMinutes()
        {
            Assert.Less(HoursToReach(5) * 60.0, 20.0, "Level 5 should be minutes away, not an evening.");
        }

        /// <summary>
        /// Every level must cost strictly more time than the one before it. A dip anywhere would
        /// mean a stretch of the game that gets easier as it goes.
        /// </summary>
        [Test]
        public void EveryLevelCostsMoreThanThePreviousOne()
        {
            for (int level = 2; level < ExperienceTable.MaxLevel; level++)
            {
                Assert.Greater(
                    ExperienceTable.XpToNextLevel(level),
                    ExperienceTable.XpToNextLevel(level - 1),
                    "Level " + level + " costs no more than level " + (level - 1) + ".");
            }
        }

        /// <summary>
        /// progress.md says a level 1 hero carried by a group farming at the maximum level reaches
        /// 100 in roughly 200 hours, against 366 at the normal pace. The point of the number is the
        /// gap: being carried is worth something, but it is not a shortcut, which is what justifies
        /// the experience item existing at all.
        /// </summary>
        [Test]
        public void BeingCarriedAtMaxLevelIsFasterButNotAShortcut()
        {
            double carried = HoursCarriedAt(ExperienceTable.MaxLevel);

            Assert.Less(carried, HoursToReach(100), "Being carried has to be faster than the normal pace.");
            Assert.Greater(carried, HoursToReach(100) * 0.35, "Being carried must not trivialise the climb.");
        }

        /// <summary>
        /// Hours for a level 1 hero to reach the last level while every enemy it receives
        /// experience from is of the given level.
        /// </summary>
        private static double HoursCarriedAt(int enemyLevel)
        {
            long perEnemy = ExperienceTable.XpFromMinion(enemyLevel);
            double minutes = 0;

            for (int current = 1; current < ExperienceTable.MaxLevel; current++)
            {
                minutes += (double)ExperienceTable.XpToNextLevel(current) / perEnemy / EnemiesPerMinute;
            }

            return minutes / 60.0;
        }

        /// <summary>
        /// progress.md: experience comes from the enemy's level, never from the killer's. It is
        /// what stops the player farming act 1 forever, so it is worth an explicit assertion.
        /// </summary>
        [Test]
        public void FarmingLowLevelEnemiesStopsPayingOff()
        {
            double hoursAtLevelOne = ExperienceTable.XpToNextLevel(60) / (double)ExperienceTable.XpFromMinion(1)
                / EnemiesPerMinute / 60.0;

            Assert.Greater(hoursAtLevelOne, 10000.0,
                "Grinding level 1 minions to gain a level in the sixties has to be hopeless.");
        }
    }
}
