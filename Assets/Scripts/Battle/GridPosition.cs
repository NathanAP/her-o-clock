using System;

namespace HerOClock.Battle
{
    /// <summary>
    /// Uma casa do campo de batalha. Coluna e fileira comecam em 1, igual a spec.
    /// </summary>
    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int Column;
        public int Row;

        public GridPosition(int column, int row)
        {
            Column = column;
            Row = row;
        }

        /// <summary>
        /// Distancia em casas contando a diagonal como 1, igual ao rei do xadrez.
        /// Ou seja, a maior diferenca entre as fileiras e as colunas.
        /// </summary>
        public static int Distance(GridPosition a, GridPosition b)
        {
            int columns = Math.Abs(a.Column - b.Column);
            int rows = Math.Abs(a.Row - b.Row);
            return columns > rows ? columns : rows;
        }

        public bool Equals(GridPosition other)
        {
            return Column == other.Column && Row == other.Row;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Column * 397) ^ Row;
        }

        public override string ToString()
        {
            return "(coluna " + Column + ", fileira " + Row + ")";
        }
    }
}
