using HerOClock.Window;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks the arithmetic of the window sizes.
    ///
    /// How the window looks is not something a test can judge, and the rest of this feature is
    /// exactly that. What can be pinned down is the part that would be wrong in a way nobody
    /// notices: a size that is not a whole multiple, or a window larger than the screen it opens on.
    ///
    /// The reference resolution used here is the game's own, 180 by 320.
    /// </summary>
    public class WindowScaleLadderTests
    {
        private const int Width = 180;
        private const int Height = 320;

        // --- The ladder itself ---

        [Test]
        public void EveryRungIsAWholeNumberAndTheyAscend()
        {
            int previous = 0;

            for (int i = 0; i < WindowScaleLadder.Steps.Length; i++)
            {
                Assert.Greater(WindowScaleLadder.Steps[i], previous, "The rungs have to ascend.");
                previous = WindowScaleLadder.Steps[i];
            }
        }

        /// <summary>
        /// Nothing below 1x. Shrinking below the reference would mean drawing a fraction of a
        /// pixel, and the pixel art would shimmer instead of getting smaller.
        /// </summary>
        [Test]
        public void TheSmallestRungIsOne()
        {
            Assert.AreEqual(1, WindowScaleLadder.Steps[0]);
        }

        // --- What fits ---

        /// <summary>A 1080p screen has room for 2x, since 4x would need 1280 pixels of height.</summary>
        [Test]
        public void ATenEightyScreenReachesTwo()
        {
            Assert.AreEqual(2, WindowScaleLadder.Largest(Width, Height, 1920, 1080, WindowScaleLadder.MaximumShare));
        }

        [Test]
        public void AFourKScreenReachesFour()
        {
            Assert.AreEqual(4, WindowScaleLadder.Largest(Width, Height, 3840, 2160, WindowScaleLadder.MaximumShare));
        }

        /// <summary>
        /// A window slightly too large can be fixed by the player. No window at all cannot, so
        /// the answer is never zero however small the screen is.
        /// </summary>
        [Test]
        public void AScreenTooSmallForEvenOneStillGetsOne()
        {
            Assert.AreEqual(1, WindowScaleLadder.Largest(Width, Height, 100, 100, WindowScaleLadder.MaximumShare));
        }

        /// <summary>
        /// The window never opens larger than the screen, on any screen a person might have.
        /// </summary>
        [TestCase(1280, 720)]
        [TestCase(1366, 768)]
        [TestCase(1920, 1080)]
        [TestCase(2560, 1440)]
        [TestCase(3840, 2160)]
        public void TheChosenSizeAlwaysFitsTheScreen(int screenWidth, int screenHeight)
        {
            int scale = WindowScaleLadder.Largest(Width, Height, screenWidth, screenHeight, WindowScaleLadder.MaximumShare);

            Assert.LessOrEqual(Width * scale, screenWidth);
            Assert.LessOrEqual(Height * scale, screenHeight);
        }

        /// <summary>
        /// The game opens small, because it is meant to sit beside whatever the player is really
        /// doing. Half the screen is not small.
        /// </summary>
        [TestCase(1920, 1080)]
        [TestCase(2560, 1440)]
        [TestCase(3840, 2160)]
        public void TheStartingSizeLeavesMostOfTheScreenAlone(int screenWidth, int screenHeight)
        {
            int scale = WindowScaleLadder.Largest(Width, Height, screenWidth, screenHeight, WindowScaleLadder.DefaultShare);

            Assert.LessOrEqual(Height * scale, screenHeight / 2,
                "The window should not take half the height of the screen on opening.");
        }

        [Test]
        public void TheStartingSizeIsNeverLargerThanWhatFits()
        {
            int start = WindowScaleLadder.Largest(Width, Height, 1920, 1080, WindowScaleLadder.DefaultShare);
            int most = WindowScaleLadder.Largest(Width, Height, 1920, 1080, WindowScaleLadder.MaximumShare);

            Assert.LessOrEqual(start, most);
        }

        // --- Stepping ---

        [Test]
        public void SteppingUpWalksTheLadder()
        {
            Assert.AreEqual(2, WindowScaleLadder.Next(1, 4));
            Assert.AreEqual(4, WindowScaleLadder.Next(2, 4));
        }

        [Test]
        public void SteppingUpStopsAtWhatTheScreenAllows()
        {
            Assert.AreEqual(2, WindowScaleLadder.Next(2, 2), "The screen only has room for 2x.");
        }

        [Test]
        public void SteppingDownWalksBack()
        {
            Assert.AreEqual(2, WindowScaleLadder.Previous(4));
            Assert.AreEqual(1, WindowScaleLadder.Previous(2));
        }

        [Test]
        public void SteppingDownStopsAtTheSmallest()
        {
            Assert.AreEqual(1, WindowScaleLadder.Previous(1));
        }

        // --- Remembered choices ---

        /// <summary>
        /// A remembered size can outlive the screen it was chosen on: the player moves the game to
        /// a laptop, or unplugs a monitor. What fits has to be decided again on every start.
        /// </summary>
        [Test]
        public void ARememberedSizeTooLargeForTheScreenIsBroughtDown()
        {
            Assert.AreEqual(2, WindowScaleLadder.Snap(4, 2));
        }

        [Test]
        public void ARememberedSizeThatIsNotOnTheLadderIsBroughtDown()
        {
            Assert.AreEqual(2, WindowScaleLadder.Snap(3, 4), "Three is not a rung.");
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void ANonsenseRememberedSizeFallsBackToTheSmallest(int remembered)
        {
            Assert.AreEqual(1, WindowScaleLadder.Snap(remembered, 4));
        }
    }
}
