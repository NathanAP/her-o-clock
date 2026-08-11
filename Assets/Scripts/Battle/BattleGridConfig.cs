using UnityEngine;

namespace HerOClock.Battle
{
    /// <summary>
    /// Battlefield dimensions.
    ///
    /// Kept in a separate asset on purpose, so we can try other board sizes without
    /// touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "BattleGridConfig", menuName = "Her-o-clock/Battle Grid Config")]
    public class BattleGridConfig : ScriptableObject
    {
        [Tooltip("Number of board columns. The spec uses 6.")]
        [Min(1)] public int Columns = 6;

        [Tooltip("Number of board rows. The spec uses 8.")]
        [Min(2)] public int Rows = 8;

        [Tooltip("How many rows, counting from the bottom, form the hero area. The spec uses 4.")]
        [Min(1)] public int HeroRows = 4;

        [Tooltip("Size of one cell in world units. With 30 pixels per unit, 1 means a 30x30 pixel cell.")]
        [Min(0.01f)] public float CellSize = 1f;

        public int TotalCells
        {
            get { return Columns * Rows; }
        }

        /// <summary>
        /// Rows 1 through HeroRows are the hero area, the rest belongs to minions and villains.
        /// The areas only define where each side starts, they do not restrict movement.
        /// </summary>
        public bool IsHeroRow(int row)
        {
            return row <= HeroRows;
        }
    }
}
