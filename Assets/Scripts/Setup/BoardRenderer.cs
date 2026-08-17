using HerOClock.Battle;
using HerOClock.Common;
using HerOClock.View;
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

        /// <summary>
        /// Rows drawn beyond each end of the board, hidden off screen.
        ///
        /// It has to cover the whole slide of a transition plus the strip of board the camera
        /// shows past the playable area. Anything less and the slide exposes an empty gap at
        /// the top, so this value is tied to the scroller and must never fall behind it.
        /// </summary>
        private const int DecorativeRows = BoardScroller.RowsPerTransition + 3;

        /// <summary>
        /// Columns drawn beyond each side of the board.
        ///
        /// The board is exactly as wide as the camera's reference resolution, so it has no margin
        /// at all: a window a couple of pixels wider than intended shows the ground simply stop,
        /// with the camera's background behind it. That is not supposed to happen, and the window
        /// holds itself to an exact size to make sure it does not, but a hair of margin costs
        /// nothing and turns a visible black band into nothing at all.
        ///
        /// They are only decoration. Nobody can walk there, exactly like the rows above and below.
        /// </summary>
        private const int DecorativeColumns = 2;

        /// <summary>The board object, which is the one that slides between waves.</summary>
        public static Transform Build(BattleGrid grid, Transform parent)
        {
            BattleGridConfig config = grid.Config;

            GameObject board = new GameObject("Board");
            board.transform.SetParent(parent, false);

            for (int row = 1 - DecorativeRows; row <= config.Rows + DecorativeRows; row++)
            {
                for (int column = 1 - DecorativeColumns; column <= config.Columns + DecorativeColumns; column++)
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
            // Spans the decorative columns too, so it does not stop short of the visible ground.
            divider.transform.localScale = new Vector3(
                (config.Columns + DecorativeColumns * 2) * config.CellSize, config.CellSize * 0.05f, 1f);

            SpriteRenderer renderer = divider.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite.Get();
            renderer.color = DividerColor;
            renderer.sortingLayerName = "Ground";
        }
    }
}
