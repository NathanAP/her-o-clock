using HerOClock.Characters;
using HerOClock.Progression;
using NUnit.Framework;
using Attribute = HerOClock.Characters.Attribute;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks where a character's level points end up.
    ///
    /// The invariant runs through every test here: what was placed by hand, plus what was placed
    /// automatically, plus what is still waiting, always adds up to exactly what the level
    /// granted. Points that appear or vanish would show up as a character quietly stronger or
    /// weaker than its level says.
    /// </summary>
    public class AttributeAllocationTests
    {
        private static AttributeGrowth Growth(int power, int agility, int specialty, int constitution)
        {
            return new AttributeGrowth
            {
                Power = power,
                Agility = agility,
                Specialty = specialty,
                Constitution = constitution
            };
        }

        private static readonly AttributeGrowth Balanced = Growth(25, 25, 25, 25);
        private static readonly AttributeGrowth Awkward = Growth(33, 33, 34, 0);

        private static AttributeAllocation At(int level, AttributeGrowth growth, bool automatic = true)
        {
            AttributeAllocation allocation = new AttributeAllocation(growth);
            allocation.SetAutomatic(automatic);
            allocation.GrantFor(level);
            return allocation;
        }

        private static int Placed(AttributeAllocation allocation)
        {
            int[] result = new int[4];
            allocation.WriteTo(result);
            return result[0] + result[1] + result[2] + result[3];
        }

        private static void AssertInvariant(AttributeAllocation allocation, int level)
        {
            Assert.AreEqual(LevelProgress.PointsAtLevel(level), Placed(allocation) + allocation.Unspent,
                "Placed plus unspent has to equal what the level granted.");
        }

        // --- Granting ---

        [Test]
        public void ALevelOneCharacterHasNothingToSpend()
        {
            AttributeAllocation allocation = At(1, Balanced);

            Assert.AreEqual(0, allocation.Granted);
            Assert.AreEqual(0, allocation.Unspent);
        }

        [TestCase(2, 5)]
        [TestCase(12, 55)]
        [TestCase(100, 495)]
        public void EveryLevelAfterTheFirstGrantsFivePoints(int level, int expected)
        {
            AttributeAllocation allocation = At(level, Balanced);

            Assert.AreEqual(expected, allocation.Granted);
            AssertInvariant(allocation, level);
        }

        [Test]
        public void OnAutomaticNothingIsEverLeftWaiting()
        {
            Assert.AreEqual(0, At(40, Awkward).Unspent);
        }

        [Test]
        public void OffAutomaticEverythingWaits()
        {
            AttributeAllocation allocation = At(11, Balanced, automatic: false);

            Assert.AreEqual(50, allocation.Unspent);
            Assert.AreEqual(0, Placed(allocation));
        }

        // --- The rule from architecture.md ---

        /// <summary>
        /// A character created straight at a level has to match one that climbed to it.
        ///
        /// This is the reason the automatic share is stored as a count and redistributed whole
        /// rather than being added to five points at a time. The percentages are resolved by
        /// largest remainder, so thirty-nine small splits do not add up to one big one.
        /// </summary>
        [Test]
        public void ClimbingToALevelMatchesBeingCreatedAtIt()
        {
            AttributeGrowth[] shapes = { Balanced, Awkward, Growth(40, 20, 0, 40), Growth(7, 11, 13, 17) };

            for (int s = 0; s < shapes.Length; s++)
            {
                AttributeAllocation climbed = new AttributeAllocation(shapes[s]);

                for (int level = 1; level <= 40; level++)
                {
                    climbed.GrantFor(level);
                }

                int[] fromClimbing = new int[4];
                climbed.WriteTo(fromClimbing);

                int[] fromCreation = new int[4];
                At(40, shapes[s]).WriteTo(fromCreation);

                CollectionAssert.AreEqual(fromCreation, fromClimbing, "Shape " + s + " diverged.");
            }
        }

        [Test]
        public void GrantingTheSameLevelTwiceChangesNothing()
        {
            AttributeAllocation allocation = At(20, Balanced);
            int before = allocation.Granted;

            allocation.GrantFor(20);
            allocation.GrantFor(20);

            Assert.AreEqual(before, allocation.Granted);
        }

        /// <summary>A hero carried by a strong group gains many levels from a single enemy.</summary>
        [Test]
        public void JumpingManyLevelsAtOnceGrantsThemAll()
        {
            AttributeAllocation allocation = new AttributeAllocation(Balanced);
            allocation.GrantFor(1);
            allocation.GrantFor(37);

            Assert.AreEqual(180, allocation.Granted);
            AssertInvariant(allocation, 37);
        }

        // --- Spending by hand ---

        [Test]
        public void PointsPlacedByHandLandWhereTheyWerePut()
        {
            AttributeAllocation allocation = At(3, Balanced, automatic: false);

            Assert.IsTrue(allocation.Spend(Attribute.Power, 7));

            Assert.AreEqual(7, allocation.SpentOn(Attribute.Power));
            Assert.AreEqual(3, allocation.Unspent);
            AssertInvariant(allocation, 3);
        }

        [Test]
        public void SpendingMoreThanIsAvailableIsRefused()
        {
            AttributeAllocation allocation = At(2, Balanced, automatic: false);

            Assert.IsFalse(allocation.Spend(Attribute.Power, 6));
            Assert.AreEqual(5, allocation.Unspent);
            Assert.AreEqual(0, allocation.SpentOn(Attribute.Power));
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void SpendingNothingOrLessIsRefused(int amount)
        {
            AttributeAllocation allocation = At(2, Balanced, automatic: false);

            Assert.IsFalse(allocation.Spend(Attribute.Power, amount));
            Assert.AreEqual(5, allocation.Unspent);
        }

        [Test]
        public void OnAutomaticThereIsNothingLeftToPlaceByHand()
        {
            Assert.IsFalse(At(10, Balanced).Spend(Attribute.Power, 1));
        }

        // --- The automatic switch ---

        /// <summary>
        /// Turning it on spends what was waiting and leaves alone what the player placed. Wiping
        /// a build has to be a deliberate press of reset, not a side effect of a toggle.
        /// </summary>
        [Test]
        public void TurningAutomaticOnKeepsWhatWasPlacedByHand()
        {
            AttributeAllocation allocation = At(3, Balanced, automatic: false);
            allocation.Spend(Attribute.Power, 6);

            allocation.SetAutomatic(true);

            Assert.GreaterOrEqual(allocation.SpentOn(Attribute.Power), 6, "The manual points were lost.");
            Assert.AreEqual(0, allocation.Unspent);
            AssertInvariant(allocation, 3);
        }

        [Test]
        public void TurningAutomaticOffKeepsEverythingAlreadyPlaced()
        {
            AttributeAllocation allocation = At(11, Balanced);
            int placed = Placed(allocation);

            allocation.SetAutomatic(false);

            Assert.AreEqual(placed, Placed(allocation));
            Assert.AreEqual(0, allocation.Unspent, "Nothing new was granted, so nothing should be waiting.");
        }

        [Test]
        public void PointsEarnedWhileAutomaticIsOffWaitForThePlayer()
        {
            AttributeAllocation allocation = At(3, Balanced);
            allocation.SetAutomatic(false);
            allocation.GrantFor(5);

            Assert.AreEqual(10, allocation.Unspent);
            AssertInvariant(allocation, 5);
        }

        // --- Reset ---

        /// <summary>With automatic on, reset means "go back to the sheet's build".</summary>
        [Test]
        public void ResetOnAutomaticReturnsToTheSheetDistribution()
        {
            AttributeAllocation recommended = At(11, Growth(40, 20, 0, 40));

            AttributeAllocation meddled = At(11, Growth(40, 20, 0, 40));
            meddled.SetAutomatic(false);
            meddled.Reset();
            meddled.Spend(Attribute.Specialty, 50);
            meddled.SetAutomatic(true);
            meddled.Reset();

            int[] expected = new int[4];
            recommended.WriteTo(expected);

            int[] actual = new int[4];
            meddled.WriteTo(actual);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ResetOffAutomaticFreesEverything()
        {
            AttributeAllocation allocation = At(11, Balanced);
            allocation.SetAutomatic(false);
            allocation.Reset();

            Assert.AreEqual(50, allocation.Unspent);
            Assert.AreEqual(0, Placed(allocation));
            AssertInvariant(allocation, 11);
        }

        [Test]
        public void ResetNeverCreatesOrLosesPoints()
        {
            AttributeAllocation allocation = At(21, Awkward);
            allocation.SetAutomatic(false);
            allocation.Reset();
            allocation.Spend(Attribute.Agility, 40);
            allocation.Reset();

            AssertInvariant(allocation, 21);
            Assert.AreEqual(100, allocation.Granted);
        }

        // --- The event ---

        [Test]
        public void EveryChangeIsAnnounced()
        {
            AttributeAllocation allocation = At(3, Balanced, automatic: false);

            int raised = 0;
            allocation.Changed += () => raised++;

            allocation.Spend(Attribute.Power, 1);
            allocation.SetAutomatic(true);
            allocation.Reset();

            Assert.AreEqual(3, raised);
        }

        [Test]
        public void SettingTheSwitchToWhatItAlreadyIsAnnouncesNothing()
        {
            AttributeAllocation allocation = At(3, Balanced);

            int raised = 0;
            allocation.Changed += () => raised++;

            allocation.SetAutomatic(true);

            Assert.AreEqual(0, raised);
        }
    }
}
