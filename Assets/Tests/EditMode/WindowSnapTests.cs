using HerOClock.Window;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks the magnetism that pulls the window against the edges of the screen.
    ///
    /// Whether it feels good to drag is not something a test can answer, and most of this feature
    /// is exactly that. What can be pinned down is the arithmetic: a window that lands one pixel
    /// off the corner, or that snaps to the wrong edge, would look like nothing in particular and
    /// be very annoying to use.
    ///
    /// The window here is 360 by 640, which is the game at 2x, on a 1920 by 1080 screen starting
    /// at the origin.
    /// </summary>
    public class WindowSnapTests
    {
        private const int Width = 360;
        private const int Height = 640;

        private const int Left = 0;
        private const int Top = 0;
        private const int Right = 1920;
        private const int Bottom = 1080;

        private static WindowSnap.Position At(int x, int y)
        {
            return WindowSnap.Apply(x, y, Width, Height, Left, Top, Right, Bottom);
        }

        // --- Nothing near an edge ---

        [Test]
        public void AWindowInTheMiddleIsLeftAlone()
        {
            WindowSnap.Position result = At(800, 300);

            Assert.AreEqual(800, result.X);
            Assert.AreEqual(300, result.Y);
        }

        [Test]
        public void AWindowJustOutOfReachIsLeftAlone()
        {
            int justTooFar = WindowSnap.Threshold + 1;

            Assert.AreEqual(justTooFar, At(justTooFar, 300).X);
        }

        // --- Single edges ---

        [Test]
        public void NearTheLeftItGoesFlushLeft()
        {
            Assert.AreEqual(Left, At(WindowSnap.Threshold - 1, 300).X);
        }

        [Test]
        public void NearTheRightItGoesFlushRight()
        {
            int nearlyRight = Right - Width - (WindowSnap.Threshold - 1);

            Assert.AreEqual(Right - Width, At(nearlyRight, 300).X);
        }

        [Test]
        public void NearTheTopItGoesFlushTop()
        {
            Assert.AreEqual(Top, At(800, WindowSnap.Threshold - 1).Y);
        }

        /// <summary>
        /// The bottom edge is the one the taskbar usually sits on, and this game is meant to be
        /// above it. Snapping to the work area instead would leave a gap exactly where somebody
        /// most wants the window to sit.
        /// </summary>
        [Test]
        public void NearTheBottomItGoesFlushBottomOverTheTaskbar()
        {
            int nearlyBottom = Bottom - Height - (WindowSnap.Threshold - 1);

            Assert.AreEqual(Bottom - Height, At(800, nearlyBottom).Y);
        }

        [Test]
        public void ExactlyAtTheThresholdStillSnaps()
        {
            Assert.AreEqual(Left, At(WindowSnap.Threshold, 300).X);
        }

        // --- Corners ---

        /// <summary>
        /// Each axis decides on its own, which is what makes corners work without being a case of
        /// their own.
        /// </summary>
        [Test]
        public void NearACornerBothAxesSnap()
        {
            WindowSnap.Position result = At(
                Right - Width - 5,
                Bottom - Height - 5);

            Assert.AreEqual(Right - Width, result.X);
            Assert.AreEqual(Bottom - Height, result.Y);
        }

        [Test]
        public void NearOneEdgeOnlyThatAxisMoves()
        {
            WindowSnap.Position result = At(5, 300);

            Assert.AreEqual(Left, result.X);
            Assert.AreEqual(300, result.Y, "The vertical was nowhere near an edge.");
        }

        // --- Awkward screens ---

        /// <summary>
        /// A monitor that is not the primary one does not start at zero. Snapping has to work off
        /// the bounds it is given rather than assuming the origin.
        /// </summary>
        [Test]
        public void ASecondMonitorToTheLeftSnapsToItsOwnEdges()
        {
            WindowSnap.Position result = WindowSnap.Apply(
                -1910, 5, Width, Height,
                -1920, 0, 0, 1080);

            Assert.AreEqual(-1920, result.X);
            Assert.AreEqual(0, result.Y);
        }

        [Test]
        public void AWindowAlreadyFlushStaysPut()
        {
            WindowSnap.Position result = At(Left, Top);

            Assert.AreEqual(Left, result.X);
            Assert.AreEqual(Top, result.Y);
        }

        /// <summary>
        /// On a screen barely bigger than the window both edges are in reach at once. The near
        /// edge wins, so the result is at least predictable rather than jittering between the two.
        /// </summary>
        [Test]
        public void WhenBothEdgesAreInReachTheNearOneWins()
        {
            WindowSnap.Position result = WindowSnap.Apply(
                5, 5, Width, Height,
                0, 0, Width + 10, Height + 10);

            Assert.AreEqual(0, result.X);
            Assert.AreEqual(0, result.Y);
        }

        [Test]
        public void AThresholdOfZeroSnapsOnlyWhenAlreadyExact()
        {
            Assert.AreEqual(3, WindowSnap.Apply(3, 300, Width, Height, Left, Top, Right, Bottom, 0).X);
            Assert.AreEqual(0, WindowSnap.Apply(0, 300, Width, Height, Left, Top, Right, Bottom, 0).X);
        }
    }
}
