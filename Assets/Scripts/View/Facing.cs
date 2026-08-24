namespace HerOClock.View
{
    /// <summary>
    /// Which way a character is turned on the board.
    ///
    /// Four directions and not eight. The board is only six columns wide and is always read
    /// from the bottom up, so a character spends nearly all of its time walking straight up or
    /// straight down a column. Diagonal art would cost four more sprites per character to be
    /// seen for a fraction of a second during a sidestep.
    /// </summary>
    public enum Facing
    {
        /// <summary>Turned towards the bottom of the board, so the player sees its front.</summary>
        Down,

        /// <summary>Turned towards the top of the board, so the player sees its back.</summary>
        Up,

        Left,

        Right
    }
}
