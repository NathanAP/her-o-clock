using HerOClock.Battle;
using HerOClock.Common;
using UnityEngine;

namespace HerOClock.Setup
{
    /// <summary>
    /// Draws the board with green squares, as the roadmap asks for while there is no art.
    ///
    /// The checkerboard is uniform rather than tinted per area. The areas only decide where
    /// each side starts, so colouring them differently would suggest a territorial rule that
    /// does not exist. A thin static line marks the boundary instead.
    ///
    /// Extra rows are drawn above and below the playable area so that sliding the board
    /// between waves never reveals a gap.
    /// </summary>
    public static class BoardRenderer
    {
        private static readonly Color GroundLight = new Color(0.27f, 0.5f, 0.3f);
        private static readonly Color GroundDark = new Color(0.22f, 0.42f, 0.25f);
        private static readonly Color DividerColor = new Color(0.6f, 0.65f, 0.45f, 0.35f);

        /// <summary>Rows drawn beyond each end of the board, hidden off screen.</summary>
        private const int DecorativeRows = 3;

        /// <summary>The board object, which is the one that slides between waves.</summary>
        public static Transform Build(BattleGrid grid, Transform parent)
        {
            BattleGridConfig config = grid.Config;

            GameObject board = new GameObject("Board");
            board.transform.SetParent(parent, false);

            for (int row = 1 - DecorativeRows; row <= config.Rows + DecorativeRows; row++)
            {
                for (int column = 1; column <= config.Columns; column++)
                {
                    GridPosition position = new GridPosition(column, row);

                    GameObject cell = new GameObject("Cell " + column + "-" + row);
                    cell.transform.SetParent(board.transform, false);
                    cell.transform.localPosition = grid.WorldPositionOf(position);
                    cell.transform.localScale = new Vector3(config.CellSize, config.CellSize, 1f);

                    SpriteRenderer renderer = cell.AddComponent<SpriteRenderer>();
                    renderer.sprite = SquareSprite.Get();
                    renderer.color = (column + row) % 2 == 0 ? GroundLight : GroundDark;
                    renderer.sortingLayerName = "Background";
                }
            }

            BuildAreaDivider(grid, parent);

            return board.transform;
        }

        /// <summary>
        /// Marks where the hero area ends. It is a sibling of the board rather than a child,
        /// so it stays put while the ground slides underneath it.
        /// </summary>
        private static void BuildAreaDivider(BattleGrid grid, Transform parent)
        {
            BattleGridConfig config = grid.Config;

            GameObject divider = new GameObject("AreaDivider");
            divider.transform.SetParent(parent, false);

            // Sits on the seam between the last hero row and the first enemy row.
            float y = (config.HeroRows - (config.Rows - 1) * 0.5f) * config.CellSize - config.CellSize * 0.5f;
            divider.transform.localPosition = new Vector3(0f, y, 0f);
            divider.transform.localScale = new Vector3(config.Columns * config.CellSize, config.CellSize * 0.05f, 1f);

            SpriteRenderer renderer = divider.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite.Get();
            renderer.color = DividerColor;
            renderer.sortingLayerName = "Ground";
        }
    }
}
