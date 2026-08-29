namespace HerOClock.Abilities
{
    /// <summary>
    /// Where a `moveTo` effect puts somebody, from abilities.md.
    /// </summary>
    public enum MoveAnchor
    {
        /// <summary>
        /// One of the four cells next to the last target: the first one free and on the board,
        /// ordered by lowest row and then lowest column.
        ///
        /// The order has to be fixed rather than "the nearest" or "the most convenient", or the
        /// same battle from the same seed could end differently.
        /// </summary>
        LastTargetAnySide = 0
    }
}
