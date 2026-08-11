using HerOClock.Battle;
using HerOClock.Common;
using UnityEngine;

namespace HerOClock.Setup
{
    /// <summary>
    /// Draws the board with green squares, as the roadmap asks for while there is no art.
    /// Alternating shades keep the cells visible, and the minion and villain area is a little
    /// darker so the two halves can be told apart.
    /// </summary>
    public static class BoardRenderer
    {
        private static readonly Color HeroAreaLight = new Color(0.29f, 0.53f, 0.31f);
        private static readonly Color HeroAreaDark = new Color(0.24f, 0.45f, 0.26f);
        private static readonly Color EnemyAreaLight = new Color(0.18f, 0.35f, 0.22f);
        private static readonly Color EnemyAreaDark = new Color(0.15f, 0.29f, 0.18f);

        public static GameObject Build(BattleGrid grid, Transform parent)
        {
            BattleGridConfig config = grid.Config;

            GameObject board = new GameObject("Board");
            board.transform.SetParent(parent, false);

            foreach (GridPosition position in grid.AllPositions())
            {
                GameObject cell = new GameObject("Cell " + position.Column + "-" + position.Row);
                cell.transform.SetParent(board.transform, false);
                cell.transform.position = grid.WorldPositionOf(position);
                cell.transform.localScale = new Vector3(config.CellSize, config.CellSize, 1f);

                SpriteRenderer renderer = cell.AddComponent<SpriteRenderer>();
                renderer.sprite = SquareSprite.Get();
                renderer.color = ColorFor(config, position);
                renderer.sortingLayerName = "Background";
            }

            return board;
        }

        private static Color ColorFor(BattleGridConfig config, GridPosition position)
        {
            bool isLightCell = (position.Column + position.Row) % 2 == 0;

            if (config.IsHeroRow(position.Row))
            {
                return isLightCell ? HeroAreaLight : HeroAreaDark;
            }

            return isLightCell ? EnemyAreaLight : EnemyAreaDark;
        }
    }
}
