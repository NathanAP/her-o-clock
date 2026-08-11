using HerOClock.Battle;
using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// A character on the battlefield: it occupies a cell, moves, fights and dies.
    /// </summary>
    public class Character : MonoBehaviour, IGridOccupant
    {
        private BattleGrid grid;

        public CharacterDefinition Definition { get; private set; }
        public Team Team { get; private set; }
        public GridPosition Position { get; private set; }
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// Who is currently taunting this character. Always null for now, since taunt only
        /// comes from skills, which do not exist yet. Target selection already honours it.
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

        /// <summary>Cell where the character started the battle, used when restarting.</summary>
        public GridPosition InitialPosition { get; private set; }

        /// <summary>Raised on damage, healing or death, so the view can react.</summary>
        public event System.Action Changed;

        public void Initialize(CharacterDefinition definition, Team team, GridPosition position, BattleGrid grid)
        {
            Definition = definition;
            Team = team;
            this.grid = grid;

            InitialPosition = position;
            CurrentHealth = definition.Stats.MaxHealth;
            Position = position;

            grid.Occupy(position, this);
            transform.position = grid.WorldPositionOf(position);
        }

        /// <summary>Current health over maximum health, from 0 to 1.</summary>
        public float HealthFraction
        {
            get
            {
                int max = Stats.MaxHealth;
                return max <= 0 ? 0f : (float)CurrentHealth / max;
            }
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            // Minimum health is always 0, even when taking more damage than current health.
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

            if (CurrentHealth == 0)
            {
                Die();
            }

            Changed?.Invoke();
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            // Current health can never exceed maximum health.
            CurrentHealth = Mathf.Min(Stats.MaxHealth, CurrentHealth + amount);
            Changed?.Invoke();
        }

        /// <summary>
        /// Minions and villains leave the field and free their cell. Heroes stay fallen on
        /// their cell, which remains occupied so they can be revived in place.
        /// </summary>
        private void Die()
        {
            if (Kind == CharacterKind.Hero)
            {
                return;
            }

            grid.Release(Position);
        }

        /// <summary>
        /// Takes the character off the board. First half of a battle restart.
        ///
        /// Only frees the cell if it really belongs to this character. A dead minion already
        /// freed its cell on death, and by now someone else may be standing there.
        /// </summary>
        public void ClearFromGrid()
        {
            if (ReferenceEquals(grid.OccupantAt(Position), this))
            {
                grid.Release(Position);
            }
        }

        /// <summary>
        /// Puts the character back on its starting cell at full health. Second half of a restart.
        ///
        /// Must run only after everyone has left the board. Otherwise a character standing on
        /// someone else's starting cell would make both claim the same cell.
        /// </summary>
        public void ResetForBattle()
        {
            Position = InitialPosition;
            CurrentHealth = Stats.MaxHealth;
            TauntedBy = null;

            grid.Occupy(Position, this);
            transform.position = grid.WorldPositionOf(Position);

            Changed?.Invoke();
        }

        /// <summary>
        /// Moves the character to another cell. The visual position is handled by the mover,
        /// which interpolates between the two cells.
        /// </summary>
        public void MoveTo(GridPosition destination)
        {
            grid.Release(Position);
            Position = destination;
            grid.Occupy(destination, this);
        }

        /// <summary>
        /// Whether a target would be within range from any given cell.
        /// A target is only in range when it satisfies both ends of the range at once.
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
