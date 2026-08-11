using System;

namespace HerOClock.Battle
{
    /// <summary>
    /// A cell on the battlefield. Column and row start at 1, matching the spec.
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
        /// Distance in cells, counting diagonals as 1, like a chess king.
        /// In other words, the largest difference between rows and columns.
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
            return "(column " + Column + ", row " + Row + ")";
        }
    }
}
