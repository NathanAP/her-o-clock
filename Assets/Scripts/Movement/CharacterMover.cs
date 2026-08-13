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
        private float stepCarry;
        private bool isStepping;

        private GridPosition walkDestination;
        private bool isWalkingBack;

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
            stepCarry = 0f;
            isWalkingBack = false;
        }

        /// <summary>True while the character still has ground to cover on its way back.</summary>
        public bool IsWalkingBack
        {
            get { return isWalkingBack || isStepping; }
        }

        /// <summary>
        /// Starts walking back to a given cell, used between waves so the party visibly
        /// regroups instead of blinking back into formation.
        /// </summary>
        public void BeginWalkBack(GridPosition destination)
        {
            walkDestination = destination;
            isWalkingBack = !character.Position.Equals(destination);
            isStepping = false;
            stepProgress = 0f;
            stepCarry = 0f;

            // The battle may have ended mid step, leaving the body between two cells.
            character.transform.position = grid.WorldPositionOf(character.Position);
        }

        /// <summary>
        /// Advances the walk back. Called instead of Tick while the party is regrouping,
        /// so nobody looks for enemies that are no longer there.
        /// </summary>
        public void TickWalkBack(float step)
        {
            if (!character.IsAlive)
            {
                isWalkingBack = false;
                return;
            }

            if (isStepping)
            {
                ContinueStep(step);
                return;
            }

            if (!isWalkingBack)
            {
                stepCarry = 0f;
                return;
            }

            if (character.Position.Equals(walkDestination))
            {
                isWalkingBack = false;
                stepCarry = 0f;
                return;
            }

            StepTowards(walkDestination);

            if (!isStepping)
            {
                stepCarry = 0f;
            }
        }

        /// <summary>
        /// One step in the direction of a cell. Unlike combat movement, a step that does not
        /// get any closer is still accepted: two characters standing on each other's starting
        /// cell would otherwise block each other forever.
        /// </summary>
        private void StepTowards(GridPosition destination)
        {
            GridPosition best = character.Position;
            int bestDistance = int.MaxValue;
            bool found = false;

            foreach (GridPosition neighbour in grid.Neighbours(character.Position))
            {
                if (!grid.IsFree(neighbour))
                {
                    continue;
                }

                int distance = GridPosition.Distance(neighbour, destination);

                if (!found || distance < bestDistance
                    || (distance == bestDistance && IsLowerPosition(neighbour, best)))
                {
                    best = neighbour;
                    bestDistance = distance;
                    found = true;
                }
            }

            if (found)
            {
                StartStep(best);
            }
        }

        public void Tick(float step, IReadOnlyList<Character> enemies)
        {
            if (!character.IsAlive)
            {
                return;
            }

            if (isStepping)
            {
                ContinueStep(step);
                return;
            }

            Decide(enemies);

            // The leftover from the previous step only belongs to the step that follows it
            // immediately. A character that stopped to attack, found nobody left, or got
            // cornered is not owed that time, and keeping it would hand out a free head start
            // whenever it eventually walks again.
            if (!isStepping)
            {
                stepCarry = 0f;
            }
        }

        private void Decide(IReadOnlyList<Character> enemies)
        {
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

            StartStep(bestPosition);
        }

        private void StartStep(GridPosition destination)
        {
            float cellsPerSecond = character.Stats.CellsPerSecond;
            if (cellsPerSecond <= 0f)
            {
                // A movement speed of 0 means the character lost the ability to move.
                return;
            }

            stepFrom = character.Position;
            stepTo = destination;
            stepDuration = 1f / cellsPerSecond;

            // Time left over from the previous step is carried in, for the same reason the
            // attack timer carries its overshoot: a step can only end on a simulation step, so
            // discarding the remainder would round every crossing up to whole steps and make a
            // character walk slower than its sheet says.
            //
            // Clamped below a full cell because a character crossing more than one cell per
            // simulation step cannot be represented by a stepped simulation anyway.
            stepProgress = stepCarry > 0f ? Mathf.Min(stepCarry / stepDuration, 1f) : 0f;
            stepCarry = 0f;

            // The destination cell is claimed immediately, before the animation ends, so that
            // nobody else tries to enter it in the meantime.
            character.MoveTo(stepTo);
            isStepping = true;
        }

        private void ContinueStep(float step)
        {
            stepProgress += step / stepDuration;

            if (stepProgress >= 1f)
            {
                stepCarry = (stepProgress - 1f) * stepDuration;
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
