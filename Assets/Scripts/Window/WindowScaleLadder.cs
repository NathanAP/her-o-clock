namespace HerOClock.Window
{
    /// <summary>
    /// The sizes the game window is allowed to be.
    ///
    /// The window is never an arbitrary size. It is always the reference resolution multiplied by
    /// a whole number, which is what keeps the pixel art honest: at 2x every game pixel is exactly
    /// four screen pixels, and at 2.3x some pixels would be bigger than others and the art would
    /// visibly shimmer.
    ///
    /// Holding to whole numbers also means the aspect ratio can never drift, so nothing ever has
    /// to be cropped or letterboxed. The window is either a legal size or it is not offered.
    ///
    /// There is no step below 1x. Going smaller would mean shrinking the picture rather than
    /// enlarging it, and a fraction of a pixel cannot be drawn. To make the game genuinely smaller
    /// the reference resolution itself would have to shrink, which means showing less of the
    /// board, and that is a decision about framing rather than about window size.
    ///
    /// No Unity here, so the arithmetic is checked outside the editor.
    /// </summary>
    public static class WindowScaleLadder
    {
        /// <summary>The rungs, ascending. Matches the small, medium and large of the genre.</summary>
        public static readonly int[] Steps = { 1, 2, 4 };

        /// <summary>
        /// Share of the screen the window is allowed to take when the game is opened for the
        /// first time. Deliberately small: the game is meant to sit in a corner while the player
        /// does something else, and a window that opens over half the screen is not that.
        /// </summary>
        public const float DefaultShare = 0.4f;

        /// <summary>
        /// Share of the screen the player is allowed to reach by hand. Short of the whole screen,
        /// so the window never ends up larger than the desktop it lives on.
        /// </summary>
        public const float MaximumShare = 0.9f;

        /// <summary>
        /// The largest rung that fits inside the given share of the screen.
        ///
        /// Always at least 1, even on a screen too small for it. A window slightly too large is
        /// recoverable by the player; no window at all is not.
        /// </summary>
        public static int Largest(int referenceWidth, int referenceHeight, int screenWidth, int screenHeight, float share)
        {
            int best = Steps[0];

            for (int i = 0; i < Steps.Length; i++)
            {
                int step = Steps[i];

                if (referenceWidth * step <= screenWidth * share
                    && referenceHeight * step <= screenHeight * share)
                {
                    best = step;
                }
            }

            return best;
        }

        /// <summary>The next rung up, or the current one when there is nowhere left to go.</summary>
        public static int Next(int current, int maximum)
        {
            for (int i = 0; i < Steps.Length; i++)
            {
                if (Steps[i] > current && Steps[i] <= maximum)
                {
                    return Steps[i];
                }
            }

            return current;
        }

        /// <summary>The next rung down, or the current one when already at the smallest.</summary>
        public static int Previous(int current)
        {
            int best = current;

            for (int i = 0; i < Steps.Length; i++)
            {
                if (Steps[i] < current)
                {
                    best = Steps[i];
                }
            }

            return best;
        }

        /// <summary>
        /// Forces a value onto the ladder. A remembered preference can outlive the screen it was
        /// chosen on, so what fits has to be decided again every time the game starts.
        /// </summary>
        public static int Snap(int value, int maximum)
        {
            int best = Steps[0];

            for (int i = 0; i < Steps.Length; i++)
            {
                if (Steps[i] <= value && Steps[i] <= maximum)
                {
                    best = Steps[i];
                }
            }

            return best;
        }
    }
}
