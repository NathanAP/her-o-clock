namespace HerOClock.Battle
{
    /// <summary>
    /// Anything able to occupy a cell on the battlefield.
    ///
    /// It exists so the grid does not need to know about characters, and so a villain can
    /// occupy several cells by registering itself on all of them.
    /// </summary>
    public interface IGridOccupant
    {
        GridPosition Position { get; }
    }
}
