using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Targeting;
using UnityEngine;

namespace HerOClock.Movement
{
    /// <summary>
    /// Cuida da movimentacao de um personagem, seguindo a secao "Movimentacao" de gameplay.md.
    ///
    /// Um personagem so se movimenta quando nao consegue atacar seu alvo de onde esta.
    /// Se o alvo esta longe demais ele avanca, se esta perto demais ele recua, e sempre
    /// vai para a casa vazia que mais o aproxima de conseguir atacar.
    ///
    /// Nao e MonoBehaviour de proposito. Quem chama o Tick e o BattleDirector, para que
    /// exista um unico laco de atualizacao em ordem previsivel.
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

            // Consegue atacar alguem de onde esta? Entao fica parado.
            if (TargetSelector.Select(character, enemies, true) != null)
            {
                return;
            }

            // Nao consegue atacar ninguem. A cadeia e consultada de novo, agora sem o
            // filtro de alcance, para decidir em direcao a quem andar. E isso que faz
            // uma provocacao continuar valendo mesmo com o provocador longe demais.
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

                // So vale a pena sair do lugar se a casa nova aproxima de conseguir atacar.
                // O desempate e posicional para a movimentacao ser reproduzivel.
                if (cost < bestCost || (found && cost == bestCost && IsLowerPosition(neighbour, bestPosition)))
                {
                    bestPosition = neighbour;
                    bestCost = cost;
                    found = true;
                }
            }

            if (!found)
            {
                // Encurralado: nao existe casa que melhore a situacao.
                // Quem escolhe outro alvo e a propria cadeia de prioridade no proximo tick.
                return;
            }

            float cellsPerSecond = character.Stats.CellsPerSecond;
            if (cellsPerSecond <= 0f)
            {
                // Velocidade de movimento 0 significa que o personagem perdeu a
                // habilidade de se movimentar.
                return;
            }

            stepFrom = character.Position;
            stepTo = bestPosition;
            stepProgress = 0f;
            stepDuration = 1f / cellsPerSecond;

            // A casa de destino e ocupada imediatamente, antes da animacao terminar,
            // para que ninguem tente entrar nela no mesmo instante.
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
        /// O quanto uma distancia esta fora da faixa de alcance do personagem.
        /// Zero significa que ele consegue atacar dali.
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
