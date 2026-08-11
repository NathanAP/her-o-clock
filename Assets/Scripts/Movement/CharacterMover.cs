using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Targeting;
using UnityEngine;

namespace HerOClock.Movement
{
    /// <summary>
    /// Handles one character's movement, following the "Movimentação" section of gameplay.md.
    ///
    /// A character only moves when it cannot attack its target from where it stands. If the
    /// target is too far it advances, if the target is too close it backs away, and it always
    /// goes to the free cell that best improves its chance of attacking.
    ///
    /// Not a MonoBehaviour on purpose. The BattleDirector calls Tick, so there is a single
    /// update loop running in a predictable order.
    /// </summary>
    public class CharacterMover
    {
        private readonly Character character;
        private readonly BattleGrid grid;

        private GridPosition stepFrom;
        private GridPosition stepTo;
        private float stepProgress;
        private float stepDuration;
        private bool isStepping;

        public CharacterMover(Character character, BattleGrid grid)
        {
            this.character = character;
            this.grid = grid;
        }

        public bool IsMoving
        {
            get { return isStepping; }
        }

        /// <summary>Interrupts any step in progress. Used when restarting the battle.</summary>
        public void Reset()
        {
            isStepping = false;
            stepProgress = 0f;
        }

        public void Tick(float deltaTime, IReadOnlyList<Character> enemies)
        {
            if (!character.IsAlive)
            {
                return;
            }

            if (isStepping)
            {
                ContinueStep(deltaTime);
                return;
            }

            // Can it attack anyone from where it stands? Then it stays put.
            if (TargetSelector.Select(character, enemies, true) != null)
            {
                return;
            }

            // Nobody is in range. The chain is consulted again, this time without the range
            // filter, to decide who to walk towards. That is what keeps a taunt working even
            // when the taunter is too far away to be attacked.
            Character walkTarget = TargetSelector.Select(character, enemies, false);
            if (walkTarget == null)
            {
                return;
            }

            TryStartStep(walkTarget);
        }

        private void TryStartStep(Character target)
        {
            int currentCost = RangeCost(GridPosition.Distance(character.Position, target.Position));

            GridPosition bestPosition = character.Position;
            int bestCost = currentCost;
            bool found = false;

            foreach (GridPosition neighbour in grid.Neighbours(character.Position))
            {
                if (!grid.IsFree(neighbour))
                {
                    continue;
                }

                int cost = RangeCost(GridPosition.Distance(neighbour, target.Position));

                // Moving is only worth it when the new cell brings the character closer to
                // being able to attack. Ties are broken positionally so movement is reproducible.
                if (cost < bestCost || (found && cost == bestCost && IsLowerPosition(neighbour, bestPosition)))
                {
                    bestPosition = neighbour;
                    bestCost = cost;
                    found = true;
                }
            }

            if (!found)
            {
                // Cornered: no cell improves the situation. Picking another target is up to
                // the priority chain itself on the next tick.
                return;
            }

            float cellsPerSecond = character.Stats.CellsPerSecond;
            if (cellsPerSecond <= 0f)
            {
                // A movement speed of 0 means the character lost the ability to move.
                return;
            }

            stepFrom = character.Position;
            stepTo = bestPosition;
            stepProgress = 0f;
            stepDuration = 1f / cellsPerSecond;

            // The destination cell is claimed immediately, before the animation ends, so that
            // nobody else tries to enter it in the meantime.
            character.MoveTo(stepTo);
            isStepping = true;
        }

        private void ContinueStep(float deltaTime)
        {
            stepProgress += deltaTime / stepDuration;

            if (stepProgress >= 1f)
            {
                stepProgress = 1f;
                isStepping = false;
            }

            Vector3 from = grid.WorldPositionOf(stepFrom);
            Vector3 to = grid.WorldPositionOf(stepTo);
            character.transform.position = Vector3.Lerp(from, to, stepProgress);
        }

        /// <summary>
        /// How far a distance falls outside the character's range band.
        /// Zero means it can attack from there.
        /// </summary>
        private int RangeCost(int distance)
        {
            if (distance > character.MaxRange)
            {
                return distance - character.MaxRange;
            }

            if (distance < character.MinRange)
            {
                return character.MinRange - distance;
            }

            return 0;
        }

        private static bool IsLowerPosition(GridPosition candidate, GridPosition current)
        {
            if (candidate.Row != current.Row)
            {
                return candidate.Row < current.Row;
            }

            return candidate.Column < current.Column;
        }
    }
}
