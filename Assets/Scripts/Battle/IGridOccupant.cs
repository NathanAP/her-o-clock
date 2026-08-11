namespace HerOClock.Battle
{
    /// <summary>
    /// Qualquer coisa capaz de ocupar uma casa do campo de batalha.
    /// Existe para que o tabuleiro nao precise conhecer personagens, e para que
    /// um vilao possa ocupar varias casas se registrando em todas elas.
    /// </summary>
    public interface IGridOccupant
    {
        GridPosition Position { get; }
    }
}
