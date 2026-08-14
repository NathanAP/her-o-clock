using HerOClock.Progression;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks levelling: the experience going in, the levels coming out, and the points earned,
    /// against the rules in progress.md.
    /// </summary>
    public class LevelProgressTests
    {
        private static LevelProgress Fresh(int startingLevel = 1)
        {
            return new LevelProgress(startingLevel, ExperienceTable.MaxLevel);
        }

        [Test]
        public void ACharacterStartsAtItsGivenLevelWithNoExperience()
        {
            LevelProgress progress = Fresh(7);

            Assert.AreEqual(7, progress.Level);
            Assert.AreEqual(0, progress.CurrentXp);
        }

        [TestCase(0, 1)]
        [TestCase(-3, 1)]
        [TestCase(500, 100)]
        public void TheStartingLevelIsKeptInsideTheAllowedRange(int given, int expected)
        {
            Assert.AreEqual(expected, Fresh(given).Level);
        }

        [Test]
        public void ExactlyEnoughExperienceGainsOneLevel()
        {
            LevelProgress progress = Fresh();
            progress.Award(ExperienceTable.XpToNextLevel(1));

            Assert.AreEqual(2, progress.Level);
            Assert.AreEqual(0, progress.CurrentXp);
        }

        [Test]
        public void OneShortOfALevelGainsNothing()
        {
            LevelProgress progress = Fresh();
            progress.Award(ExperienceTable.XpToNextLevel(1) - 1);

            Assert.AreEqual(1, progress.Level);
        }

        [Test]
        public void TheLeftoverCarriesIntoTheNextLevel()
        {
            LevelProgress progress = Fresh();
            progress.Award(ExperienceTable.XpToNextLevel(1) + 17);

            Assert.AreEqual(2, progress.Level);
            Assert.AreEqual(17, progress.CurrentXp);
        }

        /// <summary>
        /// Several levels from a single award is the normal case, not the exception: a level 1
        /// hero joining a group that farms high level enemies gains many at once.
        /// </summary>
        [Test]
        public void OneAwardCanGainSeveralLevels()
        {
            LevelProgress progress = Fresh();
            progress.Award(ExperienceTable.TotalXpTo(6));

            Assert.AreEqual(6, progress.Level);
        }

        [Test]
        public void EveryLevelGainedRaisesTheEvent()
        {
            LevelProgress progress = Fresh();
            int raised = 0;
            progress.LevelGained += _ => raised++;

            progress.Award(ExperienceTable.TotalXpTo(6));

            Assert.AreEqual(5, raised, "Going from 1 to 6 is five levels.");
        }

        // --- Points earned ---

        [Test]
        public void EveryLevelGrantsFiveAttributePointsAndOneSkillPoint()
        {
            LevelProgress progress = Fresh();
            progress.Award(ExperienceTable.TotalXpTo(6));

            Assert.AreEqual(25, progress.UnspentAttributePoints);
            Assert.AreEqual(5, progress.SkillPoints);
        }

        [TestCase(1, 0)]
        [TestCase(2, 5)]
        [TestCase(12, 55)]
        [TestCase(100, 495)]
        public void PointsAtLevel_CountsEveryLevelAfterTheFirst(int level, int expected)
        {
            Assert.AreEqual(expected, LevelProgress.PointsAtLevel(level));
        }

        // --- The ceiling ---

        [Test]
        public void ExperiencePastTheLastLevelIsDiscarded()
        {
            LevelProgress progress = Fresh(ExperienceTable.MaxLevel);
            progress.Award(999999999L);

            Assert.AreEqual(ExperienceTable.MaxLevel, progress.Level);
            Assert.AreEqual(0, progress.CurrentXp);
            Assert.IsTrue(progress.IsMaxLevel);
        }

        [Test]
        public void ClimbingAllTheWayStopsAtTheLastLevel()
        {
            LevelProgress progress = Fresh();
            progress.Award(long.MaxValue / 2);

            Assert.AreEqual(ExperienceTable.MaxLevel, progress.Level);
            Assert.AreEqual(0, progress.CurrentXp);
        }

        /// <summary>A sheet can cap a character below 100, which minions and villains may use.</summary>
        [Test]
        public void ASheetCanCapACharacterBelowTheGlobalMaximum()
        {
            LevelProgress progress = new LevelProgress(1, 10);
            progress.Award(long.MaxValue / 2);

            Assert.AreEqual(10, progress.Level);
        }

        [TestCase(0)]
        [TestCase(-100)]
        public void NonPositiveExperienceChangesNothing(long amount)
        {
            LevelProgress progress = Fresh();
            progress.Award(amount);

            Assert.AreEqual(1, progress.Level);
            Assert.AreEqual(0, progress.CurrentXp);
        }
    }
}
