using HerOClock.Battle;
using HerOClock.Common;
using UnityEngine;

namespace HerOClock.Setup
{
    /// <summary>
    /// Desenha o tabuleiro com quadrados verdes, conforme o roadmap pede enquanto
    /// nao existe arte. Tons alternados deixam as casas visiveis, e a area dos
    /// lacaios e viloes fica um pouco mais escura para as duas metades se distinguirem.
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

            GameObject board = new GameObject("Tabuleiro");
            board.transform.SetParent(parent, false);

            foreach (GridPosition position in grid.AllPositions())
            {
                GameObject cell = new GameObject("Casa " + position.Column + "-" + position.Row);
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
