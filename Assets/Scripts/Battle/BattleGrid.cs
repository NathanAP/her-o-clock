using System.Collections.Generic;
using UnityEngine;

namespace HerOClock.Battle
{
    /// <summary>
    /// Knows which cells exist, who occupies each one and where each cell sits in the world.
    /// The board is centred on the origin.
    /// </summary>
    public class BattleGrid
    {
        private readonly BattleGridConfig config;
        private readonly IGridOccupant[,] occupants;

        public BattleGrid(BattleGridConfig config)
        {
            this.config = config;
            // Indices run from 1 to Columns and from 1 to Rows, so index 0 is left unused.
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
        /// Converts a cell into its world position, with the board centred on the origin.
        /// </summary>
        public Vector3 WorldPositionOf(GridPosition position)
        {
            float x = (position.Column - 1 - (config.Columns - 1) * 0.5f) * config.CellSize;
            float y = (position.Row - 1 - (config.Rows - 1) * 0.5f) * config.CellSize;
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// The eight surrounding cells, all at distance 1. Cells outside the board are skipped.
        /// The order is always the same so that combat stays reproducible.
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
