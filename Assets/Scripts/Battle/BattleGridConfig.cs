using UnityEngine;

namespace HerOClock.Battle
{
    /// <summary>
    /// Dimensoes do campo de batalha.
    /// Fica em um asset separado de proposito, para conseguirmos testar outros
    /// tamanhos de tabuleiro sem mexer em codigo.
    /// </summary>
    [CreateAssetMenu(fileName = "BattleGridConfig", menuName = "Her-o-clock/Battle Grid Config")]
    public class BattleGridConfig : ScriptableObject
    {
        [Tooltip("Quantidade de colunas do tabuleiro. A spec usa 6.")]
        [Min(1)] public int Columns = 6;

        [Tooltip("Quantidade de fileiras do tabuleiro. A spec usa 8.")]
        [Min(2)] public int Rows = 8;

        [Tooltip("Quantas fileiras, contadas de baixo para cima, formam a area dos herois. A spec usa 4.")]
        [Min(1)] public int HeroRows = 4;

        [Tooltip("Tamanho de uma casa em unidades de mundo. Com 30 pixels por unidade, 1 equivale a uma casa de 30x30 pixels.")]
        [Min(0.01f)] public float CellSize = 1f;

        public int TotalCells
        {
            get { return Columns * Rows; }
        }

        /// <summary>
        /// Fileiras de 1 ate HeroRows sao a area dos herois. O resto e a area dos lacaios e viloes.
        /// As areas definem apenas onde cada lado comeca, nao limitam a movimentacao.
        /// </summary>
        public bool IsHeroRow(int row)
        {
            return row <= HeroRows;
        }
    }
}
