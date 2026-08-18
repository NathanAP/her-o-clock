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
        /// Who is currently taunting this character, set by an ability applying the taunt status
        /// and cleared when it runs out. Target selection honours it above every other rule.
        /// </summary>
        public Character TauntedBy { get; set; }

        /// <summary>The buffs and debuffs on this instance. The stats read them through their own seam.</summary>
        public StatModifiers Modifiers { get; private set; }

        /// <summary>The named states on this instance, such as untargetable or silenced.</summary>
        public CharacterStatuses Statuses { get; private set; }

        /// <summary>True while nothing can be chosen as a target, though an area still catches it.</summary>
        public bool IsUntargetable
        {
            get { return Statuses.Has(Abilities.StatusKind.Untargetable); }
        }

        /// <summary>True while no damage from any source lands on it.</summary>
        public bool IsIntangible
        {
            get { return Statuses.Has(Abilities.StatusKind.Intangible); }
        }

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
        /// Where this instance's level points went.
        ///
        /// Heroes can place them by hand, leave the sheet's distribution doing it, or take them
        /// all back. Minions and villains are left on automatic and never touch it.
        /// </summary>
        public AttributeAllocation Attributes { get; private set; }

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

        /// <summary>Whether the basic attack draws a projectile. Visual only, and no rule reads it.</summary>
        public AutoAttackType AutoAttack
        {
            get { return Definition.AutoAttack; }
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

            // Granted before subscribing, because the stats do not exist yet to be rebuilt.
            Attributes = new AttributeAllocation(definition.Growth);
            Attributes.GrantFor(Progress.Level);

            Modifiers = new StatModifiers();
            Statuses = new CharacterStatuses();

            Stats = definition.Stats.Clone();

            // Handed over before the first calculation, so every derived value already counts
            // buffs from the very first step.
            Stats.UseModifiers(Modifiers);
            Stats.ApplyInstance(Progress.Level, Attributes, multiplier);

            Modifiers.Changed += OnStatsSourceChanged;

            Attributes.Changed += OnAttributesChanged;

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
            // Granting raises Changed, which rebuilds the stats through OnAttributesChanged.
            Attributes.GrantFor(newLevel);
            Changed?.Invoke();
        }

        /// <summary>
        /// Rebuilds the stats after the level points moved, whether that was a level arriving, the
        /// player placing a point, the automatic distribution being toggled, or a reset.
        /// </summary>
        /// <summary>
        /// A buff arrived or ran out, so everything derived from the stats has to be recomputed.
        ///
        /// It goes through the same path as points being placed, because it is the same problem:
        /// a source of stats changed and the cached maximum health has to follow, including the
        /// clamp that keeps current health from sitting above it.
        /// </summary>
        private void OnStatsSourceChanged()
        {
            OnAttributesChanged();
        }

        /// <summary>
        /// Counts down every buff, debuff and named state on this character.
        ///
        /// Driven from outside like everything else in combat, so it advances by the fixed step and
        /// keeps running between waves, where an ability's buff is quietly ticking away.
        /// </summary>
        public void TickEffects(float step)
        {
            Modifiers.Tick(step);
            Statuses.Tick(step);

            // The taunt lives as a status, but who is doing the taunting is a reference. When the
            // status runs out the reference has to go with it, or the character would keep chasing
            // somebody who stopped taunting minutes ago.
            if (TauntedBy != null && !Statuses.Has(Abilities.StatusKind.Taunted))
            {
                TauntedBy = null;
                Changed?.Invoke();
            }
        }

        /// <summary>Forces this character to attack whoever applied it, for a while.</summary>
        public void ApplyTaunt(Character source, float duration)
        {
            if (source == null || duration <= 0f)
            {
                return;
            }

            // Taunt does not stack, and the most recent one wins, as gameplay.md states.
            TauntedBy = source;
            Statuses.Apply(Abilities.StatusKind.Taunted, duration);

            Changed?.Invoke();
        }

        private void OnAttributesChanged()
        {
            Stats.ApplyInstance(Level, Attributes, multiplier);

            // "A vida atual nunca pode ultrapassar a vida máxima", in attributes.md. Points only
            // ever arrive while levelling, so the maximum only ever goes up and this does nothing —
            // until the player takes their points back to rebuild, which drops the maximum with the
            // current health still sitting above it. The health bar then draws past its own end.
            //
            // Only ever downwards: a rebuild must not hand out health either.
            CurrentHealth = Mathf.Min(CurrentHealth, Stats.MaxHealth);

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

            // The single choke point every source of damage passes through, which is why the
            // intangible check lives here rather than in each of them. Missing it in one place
            // would be a state that works everywhere except against one attack.
            if (IsIntangible)
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
            // Whatever was on the character stops with it. A fallen hero stays on its cell and can
            // be revived, and it should come back as itself rather than under a buff that has been
            // counting down on a corpse.
            Modifiers.Clear();
            Statuses.Clear();
            TauntedBy = null;

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
            // A stage starting over is a clean slate. Carrying a buff across it would make the
            // restart depend on what was happening at the moment of the wipe.
            Modifiers.Clear();
            Statuses.Clear();
            TauntedBy = null;

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
