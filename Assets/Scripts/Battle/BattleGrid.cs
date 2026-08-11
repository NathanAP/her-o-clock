using System.Collections.Generic;
using UnityEngine;

namespace HerOClock.Battle
{
    /// <summary>
    /// Sabe quais casas existem, quem ocupa cada uma e onde cada casa fica no mundo.
    /// O tabuleiro fica centralizado na origem.
    /// </summary>
    public class BattleGrid
    {
        private readonly BattleGridConfig config;
        private readonly IGridOccupant[,] occupants;

        public BattleGrid(BattleGridConfig config)
        {
            this.config = config;
            // Indices de 1 ate Columns e de 1 ate Rows, entao a posicao 0 fica sem uso de proposito.
            occupants = new IGridOccupant[config.Columns + 1, config.Rows + 1];
        }

        public BattleGridConfig Config
        {
            get { return config; }
        }

        public bool IsInside(GridPosition position)
        {
            return position.Column >= 1 && position.Column <= config.Columns
                && position.Row >= 1 && position.Row <= config.Rows;
        }

        public IGridOccupant OccupantAt(GridPosition position)
        {
            if (!IsInside(position))
            {
                return null;
            }

            return occupants[position.Column, position.Row];
        }

        public bool IsFree(GridPosition position)
        {
            return IsInside(position) && occupants[position.Column, position.Row] == null;
        }

        public void Occupy(GridPosition position, IGridOccupant occupant)
        {
            if (!IsInside(position))
            {
                return;
            }

            occupants[position.Column, position.Row] = occupant;
        }

        public void Release(GridPosition position)
        {
            if (!IsInside(position))
            {
                return;
            }

            occupants[position.Column, position.Row] = null;
        }

        /// <summary>
        /// Converte uma casa na posicao correspondente do mundo, com o tabuleiro centralizado na origem.
        /// </summary>
        public Vector3 WorldPositionOf(GridPosition position)
        {
            float x = (position.Column - 1 - (config.Columns - 1) * 0.5f) * config.CellSize;
            float y = (position.Row - 1 - (config.Rows - 1) * 0.5f) * config.CellSize;
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// As oito casas ao redor, todas a distancia 1. Casas fora do tabuleiro nao entram.
        /// A ordem e sempre a mesma para que o combate seja reproduzivel.
        /// </summary>
        public IEnumerable<GridPosition> Neighbours(GridPosition position)
        {
            for (int row = position.Row - 1; row <= position.Row + 1; row++)
            {
                for (int column = position.Column - 1; column <= position.Column + 1; column++)
                {
                    if (row == position.Row && column == position.Column)
                    {
                        continue;
                    }

                    GridPosition neighbour = new GridPosition(column, row);
                    if (IsInside(neighbour))
                    {
                        yield return neighbour;
                    }
                }
            }
        }

        public IEnumerable<GridPosition> AllPositions()
        {
            for (int row = 1; row <= config.Rows; row++)
            {
                for (int column = 1; column <= config.Columns; column++)
                {
                    yield return new GridPosition(column, row);
                }
            }
        }
    }
}
