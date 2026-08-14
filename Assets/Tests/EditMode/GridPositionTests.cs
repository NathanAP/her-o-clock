using HerOClock.Battle;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks the distance rule of gameplay.md: distance is counted in cells like a chess king,
    /// so the eight surrounding cells are all at distance 1.
    ///
    /// This is not a detail. Counting the diagonal as 1 is what lets a front row character reach
    /// the three enemy cells ahead of it. Without it every column would be an isolated corridor
    /// and the formation would lose almost all of its meaning.
    /// </summary>
    public class GridPositionTests
    {
        private static int Distance(int columnA, int rowA, int columnB, int rowB)
        {
            return GridPosition.Distance(new GridPosition(columnA, rowA), new GridPosition(columnB, rowB));
        }

        [Test]
        public void ACellIsAtDistanceZeroFromItself()
        {
            Assert.AreEqual(0, Distance(3, 4, 3, 4));
        }

        /// <summary>The spec's own example: row 3 column 2 to row 4 column 1 is distance 1.</summary>
        [Test]
        public void TheSpecExampleIsDistanceOne()
        {
            Assert.AreEqual(1, Distance(2, 3, 1, 4));
        }

        [Test]
        public void AllEightNeighboursAreAtDistanceOne()
        {
            for (int column = 2; column <= 4; column++)
            {
                for (int row = 2; row <= 4; row++)
                {
                    int expected = column == 3 && row == 3 ? 0 : 1;

                    Assert.AreEqual(expected, Distance(3, 3, column, row),
                        "Cell (" + column + ", " + row + ") should be at distance " + expected + ".");
                }
            }
        }

        [Test]
        public void DistanceIsTheLargerOfTheTwoDifferences()
        {
            Assert.AreEqual(5, Distance(1, 1, 6, 3), "Five columns apart, two rows apart.");
            Assert.AreEqual(7, Distance(1, 1, 2, 8), "One column apart, seven rows apart.");
        }

        [Test]
        public void DistanceIsTheSameBothWays()
        {
            Assert.AreEqual(Distance(1, 1, 6, 8), Distance(6, 8, 1, 1));
        }

        /// <summary>
        /// The far corners of a 6 by 8 board are 7 apart, which is the longest crossing that
        /// design-decisions.md prices at 3.5 seconds of base movement speed.
        /// </summary>
        [Test]
        public void TheLongestCrossingOfTheBoardIsSevenCells()
        {
            Assert.AreEqual(7, Distance(1, 1, 6, 8));
        }

        [Test]
        public void TwoCellsWithTheSameCoordinatesAreEqual()
        {
            GridPosition a = new GridPosition(2, 5);
            GridPosition b = new GridPosition(2, 5);

            Assert.IsTrue(a.Equals(b));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ColumnAndRowAreNotInterchangeable()
        {
            Assert.IsFalse(new GridPosition(2, 5).Equals(new GridPosition(5, 2)));
        }
    }
}
