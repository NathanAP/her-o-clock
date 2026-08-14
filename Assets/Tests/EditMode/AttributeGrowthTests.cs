using HerOClock.Characters;
using NUnit.Framework;
using Attribute = HerOClock.Characters.Attribute;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks how the 5 points earned on every level are split between the four attributes.
    ///
    /// The rule that matters is that the split always lands on the exact total. Rounding each
    /// share on its own would hand out 4 or 6 points instead of 5, and the error would compound
    /// silently over a hundred levels.
    /// </summary>
    public class AttributeGrowthTests
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

        private static int[] Distribute(int points, AttributeGrowth growth)
        {
            int[] result = new int[4];
            AttributeGrowth.Distribute(points, growth, result);
            return result;
        }

        private static int Sum(int[] values)
        {
            return values[0] + values[1] + values[2] + values[3];
        }

        /// <summary>The example written in progress.md.</summary>
        [Test]
        public void TheSpecExampleSplitsAsWritten()
        {
            int[] result = Distribute(100, Growth(40, 20, 0, 40));

            Assert.AreEqual(40, result[(int)Attribute.Power]);
            Assert.AreEqual(20, result[(int)Attribute.Agility]);
            Assert.AreEqual(0, result[(int)Attribute.Specialty]);
            Assert.AreEqual(40, result[(int)Attribute.Constitution]);
        }

        [Test]
        public void AnEvenSplitOfFivePointsStillAddsUpToFive()
        {
            int[] result = Distribute(5, Growth(25, 25, 25, 25));

            Assert.AreEqual(5, Sum(result));
        }

        /// <summary>
        /// The awkward case the implementation was written for: 33/33/34 over 5 points gives
        /// 1.65, 1.65 and 1.70, and rounding those on their own hands out 4 or 6.
        /// </summary>
        [Test]
        public void TheAwkwardSplitStillAddsUpExactly()
        {
            int[] result = Distribute(5, Growth(33, 33, 34, 0));

            Assert.AreEqual(5, Sum(result));
        }

        [Test]
        public void TheTotalIsExactAtEveryLevelForEverySheetShape()
        {
            AttributeGrowth[] shapes =
            {
                Growth(25, 25, 25, 25),
                Growth(33, 33, 34, 0),
                Growth(40, 20, 0, 40),
                Growth(35, 45, 0, 20),
                Growth(30, 5, 0, 65),
                Growth(1, 1, 1, 1),
                Growth(100, 0, 0, 0),
                Growth(7, 11, 13, 17)
            };

            for (int s = 0; s < shapes.Length; s++)
            {
                for (int level = 1; level <= 100; level++)
                {
                    int points = HerOClock.Progression.LevelProgress.PointsAtLevel(level);

                    Assert.AreEqual(points, Sum(Distribute(points, shapes[s])),
                        "Shape " + s + " at level " + level + " did not add up.");
                }
            }
        }

        /// <summary>
        /// A share of zero must never receive a point, no matter how the leftover is handed out.
        /// A sheet that says "no specialty" has to mean it.
        /// </summary>
        [Test]
        public void AShareOfZeroNeverReceivesAPoint()
        {
            for (int level = 1; level <= 100; level++)
            {
                int points = HerOClock.Progression.LevelProgress.PointsAtLevel(level);
                int[] result = Distribute(points, Growth(40, 20, 0, 40));

                Assert.AreEqual(0, result[(int)Attribute.Specialty], "Level " + level + " leaked a point into SPE.");
            }
        }

        [Test]
        public void TheSameInputAlwaysGivesTheSameSplit()
        {
            int[] first = Distribute(37, Growth(33, 33, 34, 0));
            int[] second = Distribute(37, Growth(33, 33, 34, 0));

            CollectionAssert.AreEqual(first, second);
        }

        /// <summary>
        /// Percentages that do not add up to 100 still work, because the shares are normalised.
        /// The Inspector warns about it, since it is almost always a typo, but it must not break.
        /// </summary>
        [Test]
        public void SharesThatDoNotAddUpToOneHundredAreNormalised()
        {
            int[] result = Distribute(10, Growth(1, 1, 0, 0));

            Assert.AreEqual(10, Sum(result));
            Assert.AreEqual(5, result[(int)Attribute.Power]);
            Assert.AreEqual(5, result[(int)Attribute.Agility]);
        }

        [Test]
        public void ASheetWithNoSharesAtAllDistributesNothing()
        {
            Assert.AreEqual(0, Sum(Distribute(50, Growth(0, 0, 0, 0))));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void NonPositivePointsDistributeNothing(int points)
        {
            Assert.AreEqual(0, Sum(Distribute(points, Growth(25, 25, 25, 25))));
        }

        [Test]
        public void TotalIsTheSumOfTheFourShares()
        {
            Assert.AreEqual(100, Growth(40, 20, 0, 40).Total);
        }
    }
}
