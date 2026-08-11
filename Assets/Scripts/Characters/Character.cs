using HerOClock.Battle;
using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// Um personagem no campo de batalha. Na 0.1.0.0 ele ainda nao ataca nem morre,
    /// apenas ocupa uma casa e se movimenta.
    /// </summary>
    public class Character : MonoBehaviour, IGridOccupant
    {
        private BattleGrid grid;

        public CharacterDefinition Definition { get; private set; }
        public Team Team { get; private set; }
        public GridPosition Position { get; private set; }
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// Quem esta provocando este personagem. Sempre nulo na 0.1.0.0, pois provocacao
        /// so vem de habilidade. A escolha de alvo ja respeita este campo.
        /// </summary>
        public Character TauntedBy { get; set; }

        public CharacterStats Stats
        {
            get { return Definition.Stats; }
        }

        public CharacterKind Kind
        {
            get { return Definition.Kind; }
        }

        public int Level
        {
            get { return Definition.Level; }
        }

        public int MinRange
        {
            get { return Definition.MinRange; }
        }

        public int MaxRange
        {
            get { return Definition.MaxRange; }
        }

        public bool IsAlive
        {
            get { return CurrentHealth > 0; }
        }

        public void Initialize(CharacterDefinition definition, Team team, GridPosition position, BattleGrid grid)
        {
            Definition = definition;
            Team = team;
            this.grid = grid;

            CurrentHealth = definition.Stats.MaxHealth;
            Position = position;

            grid.Occupy(position, this);
            transform.position = grid.WorldPositionOf(position);
        }

        /// <summary>
        /// Move o personagem de casa no tabuleiro. A posicao visual e cuidada pelo mover,
        /// que interpola entre as duas casas.
        /// </summary>
        public void MoveTo(GridPosition destination)
        {
            grid.Release(Position);
            Position = destination;
            grid.Occupy(destination, this);
        }

        /// <summary>
        /// Diz se um alvo estaria dentro do alcance a partir de uma casa qualquer.
        /// Um inimigo so esta no alcance quando respeita as duas pontas ao mesmo tempo.
        /// </summary>
        public bool CanAttackFrom(GridPosition from, Character target)
        {
            if (target == null || !target.IsAlive)
            {
                return false;
            }

            int distance = GridPosition.Distance(from, target.Position);
            return distance >= MinRange && distance <= MaxRange;
        }

        public bool CanAttack(Character target)
        {
            return CanAttackFrom(Position, target);
        }
    }
}
