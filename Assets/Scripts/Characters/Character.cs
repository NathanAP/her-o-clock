using HerOClock.Battle;
using HerOClock.Progression;
using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// A character on the battlefield: it occupies a cell, moves, fights and dies.
    /// </summary>
    public class Character : MonoBehaviour, IGridOccupant
    {
        private BattleGrid grid;
        private float multiplier = 1f;
        private float regenerationCarry;

        public CharacterDefinition Definition { get; private set; }
        public Team Team { get; private set; }
        public GridPosition Position { get; private set; }
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// Who is currently taunting this character. Always null for now, since taunt only
        /// comes from skills, which do not exist yet. Target selection already honours it.
        /// </summary>
        public Character TauntedBy { get; set; }

        /// <summary>
        /// This instance's own stats, copied from the definition on initialisation.
        ///
        /// The definition is a shared asset: several characters can come from the same sheet,
        /// and in the editor it is the file on disk. Reading stats straight from it would make
        /// every hero of the same type share one set of numbers, and levelling one up would
        /// edit the asset itself and persist after Play.
        /// </summary>
        public CharacterStats Stats { get; private set; }

        /// <summary>Level and experience of this instance.</summary>
        public LevelProgress Progress { get; private set; }

        /// <summary>
        /// This instance's level. Heroes take their starting level from the sheet, minions and
        /// villains take it from the stage they appear in, as stated in characters.md.
        /// </summary>
        public int Level
        {
            get { return Progress.Level; }
        }

        public CharacterKind Kind
        {
            get { return Definition.Kind; }
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

        /// <summary>Raised on damage, healing, death or a level gained, so the view can react.</summary>
        public event System.Action Changed;

        /// <summary>Raised once when the character falls, so rewards can be handed out.</summary>
        public event System.Action<Character> Died;

        public void Initialize(CharacterDefinition definition, Team team, GridPosition position, BattleGrid grid, int level, float multiplier)
        {
            Definition = definition;
            Team = team;
            this.grid = grid;
            this.multiplier = multiplier;

            Progress = new LevelProgress(level, definition.MaxLevel);
            Progress.LevelGained += OnLevelGained;

            Stats = definition.Stats.Clone();
            Stats.ApplyInstance(Progress.Level, definition.Growth, multiplier);

            InitialPosition = position;
            CurrentHealth = Stats.MaxHealth;
            Position = position;

            grid.Occupy(position, this);
            transform.position = grid.WorldPositionOf(position);
        }

        /// <summary>Adds experience to this character, applying every level it earns.</summary>
        public void AwardExperience(long amount)
        {
            Progress.Award(amount);
        }

        /// <summary>
        /// Applies a level that was just gained.
        ///
        /// Maximum health goes up because it comes from POW and CON, but **current health is
        /// left alone**. Levelling up in the middle of a stage must not work as a free potion,
        /// otherwise it would undo the damage carried between waves, which is what makes the
        /// player need to farm in the first place.
        /// </summary>
        private void OnLevelGained(int newLevel)
        {
            Stats.ApplyInstance(newLevel, Definition.Growth, multiplier);
            Changed?.Invoke();
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
                Changed?.Invoke();
                Died?.Invoke(this);
                return;
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
        /// Applies one simulation step of health regeneration.
        ///
        /// Regeneration is measured in health per second and health is a whole number, so the
        /// fraction has to be carried between steps. Rounding each step on its own would floor
        /// every realistic rate to nothing: half a point per second at a sixtieth of a second is
        /// 0.008, and a hundred of those would still be zero.
        ///
        /// The carry is dropped at full health, so nobody banks a burst of healing by standing
        /// intact for a while and then taking a hit.
        /// </summary>
        public void Regenerate(float step)
        {
            if (!IsAlive)
            {
                regenerationCarry = 0f;
                return;
            }

            if (CurrentHealth >= Stats.MaxHealth)
            {
                regenerationCarry = 0f;
                return;
            }

            float perSecond = Stats.HealthPerSecond;

            if (perSecond <= 0f)
            {
                return;
            }

            regenerationCarry += perSecond * step;

            int whole = (int)regenerationCarry;

            if (whole <= 0)
            {
                return;
            }

            regenerationCarry -= whole;
            Heal(whole);
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
            CurrentHealth = Stats.MaxHealth;
            ReturnToStart();
        }

        /// <summary>
        /// Puts the character back on its starting cell without touching its health.
        ///
        /// Used between waves of the same stage, where damage carries over. Fallen heroes come
        /// back too, so the enemy area is clear for the next wave and so they sit in the right
        /// place if they are ever revived.
        ///
        /// Like ResetForBattle, it must run only after everyone has left the board.
        /// </summary>
        public void ReturnToStart()
        {
            Position = InitialPosition;
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
