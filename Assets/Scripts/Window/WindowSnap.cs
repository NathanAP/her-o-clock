namespace HerOClock.Window
{
    /// <summary>
    /// Pulls the window flush against the edges of the screen when it is dragged near them.
    ///
    /// The game is meant to live in a corner of somebody's desktop, and lining a window up by
    /// hand is fiddly work that nobody enjoys. Magnetism makes the corner the easy place to put
    /// it rather than the precise one.
    ///
    /// Each axis is decided on its own, which is what gives corners for free: drag into the
    /// bottom right and both the horizontal and the vertical pull happen at once.
    ///
    /// Pure arithmetic, with no Unity and no Windows in it, so the part that could be quietly
    /// wrong is the part that gets tested.
    /// </summary>
    public static class WindowSnap
    {
        /// <summary>
        /// How close an edge has to come before it is pulled in, in pixels.
        ///
        /// Small enough that the window can still be placed freely near an edge, large enough
        /// that reaching a corner does not need precision.
        /// </summary>
        public const int Threshold = 20;

        public struct Position
        {
            public int X;
            public int Y;

            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// Where the window should actually go, given where the drag would put it.
        ///
        /// The bounds are the whole monitor rather than the desktop work area, because this game
        /// deliberately sits above the taskbar. Snapping to the work area would leave a gap along
        /// whichever edge the taskbar is on, which is exactly the edge somebody is most likely to
        /// want to sit against.
        /// </summary>
        public static Position Apply(
            int x, int y, int width, int height,
            int monitorLeft, int monitorTop, int monitorRight, int monitorBottom,
            int threshold)
        {
            return new Position(
                SnapAxis(x, width, monitorLeft, monitorRight, threshold),
                SnapAxis(y, height, monitorTop, monitorBottom, threshold));
        }

        public static Position Apply(
            int x, int y, int width, int height,
            int monitorLeft, int monitorTop, int monitorRight, int monitorBottom)
        {
            return Apply(x, y, width, height, monitorLeft, monitorTop, monitorRight, monitorBottom, Threshold);
        }

        /// <summary>
        /// One axis. The near edge wins ties, which only matters on a screen barely wider than
        /// the window, where both edges are within reach at once.
        /// </summary>
        private static int SnapAxis(int position, int size, int min, int max, int threshold)
        {
            if (Distance(position, min) <= threshold)
            {
                return min;
            }

            if (Distance(position + size, max) <= threshold)
            {
                return max - size;
            }

            return position;
        }

        private static int Distance(int a, int b)
        {
            int difference = a - b;
            return difference < 0 ? -difference : difference;
        }
    }
}
