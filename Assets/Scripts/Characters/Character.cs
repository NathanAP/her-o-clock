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
        private int level;

        /// <summary>
        /// The attribute points this combatant fights with, frozen at construction.
        ///
        /// A copy and never a live reference. Everything that made a mid stage rebuild dangerous
        /// came from these being shared with something the player could edit.
        /// </summary>
        private readonly int[] points = new int[4];

        public CharacterDefinition Definition { get; private set; }
        public Team Team { get; private set; }

        public GridPosition Position { get; private set; }

        /// <summary>
        /// Which way the character is turned. Presentation only: nothing in combat reads it,
        /// so a character can be attacked from behind exactly as it is from the front.
        /// </summary>
        public View.Facing Facing { get; private set; }
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

        /// <summary>
        /// The hero this combatant was built from, or null for everybody else.
        ///
        /// It is here for one reason only: experience earned in a fight has to reach the hero who
        /// earned it, and the combatant is who the fight is talking to. **Nothing else reads it,
        /// and nothing here follows it.** A record levelling up mid stage changes nothing about
        /// this object, which is the whole point of them being two things.
        ///
        /// A minion, villain or NPC takes its level from the stage it appears in and never earns
        /// any experience, so it has no record. See "### Quem ganha experiência" in progress.md.
        /// </summary>
        public HeroRecord Record { get; private set; }

        /// <summary>
        /// This instance's level. Heroes take their starting level from the sheet, minions and
        /// villains take it from the stage they appear in, as stated in characters.md.
        /// </summary>
        public int Level
        {
            get { return level; }
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

        /// <summary>
        /// Builds a combatant that answers to nobody: a minion, a villain or an NPC, handed a
        /// level by the stage it appears in. Its points come straight from the sheet.
        /// </summary>
        public void Initialize(CharacterDefinition definition, Team team, GridPosition position, BattleGrid grid, int level, float multiplier)
        {
            Build(definition, team, position, grid, level, multiplier, null, null);
        }

        /// <summary>
        /// Builds the combatant a hero sends into a stage — a photograph of the record, taken now.
        ///
        /// The level and the points are read once, here. After this the record and the combatant
        /// have nothing to do with each other except experience flowing one way, so the player can
        /// level, rebuild and later re-equip the hero without any of it reaching the fight in
        /// progress. That is the rule "um ponto colocado só passa a valer na próxima fase" in
        /// attributes.md, and it costs no mechanism at all: the next stage takes a new photograph.
        /// </summary>
        /// <param name="rules">
        /// The item tables, needed to work out which worn pieces are active and what they grant.
        /// Null means equipment is not applied at all, which is what a test that is not about
        /// items wants, and what the game does before an item database exists.
        /// </param>
        public void InitializeFrom(
            HeroRecord record, Team team, GridPosition position, BattleGrid grid, float multiplier,
            Items.ItemRules rules = null)
        {
            Build(record.Definition, team, position, grid, record.Level, multiplier, record, rules);
        }

        private void Build(CharacterDefinition definition, Team team, GridPosition position, BattleGrid grid, int level, float multiplier, HeroRecord record, Items.ItemRules rules = null)
        {
            Definition = definition;
            Record = record;
            Team = team;
            this.grid = grid;
            this.multiplier = multiplier;

            this.level = level < 1 ? 1 : level > definition.MaxLevel ? definition.MaxLevel : level;

            // The points are copied, never referenced. From here on this combatant fights with
            // the build it was born with, whatever happens to the hero it came from.
            if (record != null)
            {
                record.WritePointsTo(points);
            }
            else
            {
                AttributeGrowth.Distribute(LevelProgress.PointsAtLevel(this.level), definition.Growth, points);
            }

            Modifiers = new StatModifiers();
            Statuses = new CharacterStatuses();

            Stats = definition.Stats.Clone();

            // Handed over before the first calculation, so every derived value already counts
            // buffs from the very first step.
            Stats.UseModifiers(Modifiers);
            Stats.ApplyInstance(this.level, points, multiplier);

            // After the attributes are settled and before anything is read from them, because the
            // requirement of a piece is measured against what the hero has **without** equipment.
            // Reading it afterwards would let an item pay for its own requirement.
            ApplyEquipment(record, rules);

            Modifiers.Changed += OnStatsSourceChanged;

            InitialPosition = position;
            CurrentHealth = Stats.MaxHealth;
            Position = position;
            Facing = View.FacingResolver.Default(team);

            grid.Occupy(position, this);
            transform.position = grid.WorldPositionOf(position);
        }

        /// <summary>
        /// Resolves what the hero is wearing and hands the result to the stats.
        ///
        /// The requirement is checked against the attributes the hero has with **no** equipment on,
        /// which is exactly what `Stats` reports at this moment: the sheet plus the level points,
        /// with nothing equipped yet. From there activation grows, and an item already switched on
        /// can pay for the next one.
        /// </summary>
        private void ApplyEquipment(HeroRecord record, Items.ItemRules rules)
        {
            if (record == null || rules == null || record.Equipment.Count == 0)
            {
                return;
            }

            int[] withoutEquipment =
            {
                Stats.TotalOf(Attribute.Power),
                Stats.TotalOf(Attribute.Agility),
                Stats.TotalOf(Attribute.Specialty),
                Stats.TotalOf(Attribute.Constitution)
            };

            Items.EquipmentResolution resolved = record.Equipment.Resolve(rules, withoutEquipment);

            Stats.UseEquipment(
                EquipmentComposition.Of(resolved.Classes, Stats.Equipment),
                resolved.Totals);
        }

        /// <summary>
        /// Brings the character onto the field already hurt, as a percentage of its maximum health.
        ///
        /// This is not the same as lowering maximum health: the character is still exactly who its
        /// sheet says it is, it has simply been hit already. A villain the story says was fighting
        /// somebody else before the party arrived belongs here.
        ///
        /// It never goes below 1. A character that walked in dead would end its wave before the
        /// first step, which is always a typo rather than a thing anybody wanted.
        /// </summary>
        public void StartWoundedAt(int percentOfMaximum)
        {
            if (percentOfMaximum >= 100)
            {
                return;
            }

            int wounded = Mathf.RoundToInt(Stats.MaxHealth * (percentOfMaximum / 100f));
            CurrentHealth = Mathf.Clamp(wounded, 1, Stats.MaxHealth);

            Changed?.Invoke();
        }

        /// <summary>
        /// Hands experience to the hero this combatant came from.
        ///
        /// It passes straight through: what it earns lands on the record, and **this object does
        /// not change** — not its level, not its points, not its maximum health. A level gained
        /// halfway through a stage is real, it is saved, and it arrives on the board when the next
        /// stage builds its combatants.
        ///
        /// Does nothing on a minion, villain or NPC, which have no record and never gain any.
        /// </summary>
        public void AwardExperience(long amount)
        {
            if (Record == null)
            {
                return;
            }

            Record.AwardExperience(amount);
        }

        /// <summary>
        /// A buff arrived or ran out, so everything derived from the stats has to be recomputed.
        ///
        /// It goes through the same path as points being placed, because it is the same problem:
        /// a source of stats changed and the cached maximum health has to follow, including the
        /// clamp that keeps current health from sitting above it.
        /// </summary>
        private void OnStatsSourceChanged()
        {
            RebuildStats();
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

        /// <summary>
        /// Recomputes everything derived from the stats, from the level and points this combatant
        /// was born with.
        ///
        /// Only buffs and debuffs reach here now. The level and the points cannot move on a
        /// combatant at all, so the one caller left is a modifier arriving or running out.
        /// </summary>
        private void RebuildStats()
        {
            Stats.ApplyInstance(level, points, multiplier);

            // "A vida atual nunca pode ultrapassar a vida máxima", in attributes.md. A debuff on
            // CON lowers the maximum in the middle of a fight, with the current health still
            // sitting above it, and the health bar would draw past its own end.
            //
            // Only ever downwards: a buff wearing on must not hand out health either.
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
            Facing = View.FacingResolver.Default(Team);

            grid.Occupy(Position, this);
            transform.position = grid.WorldPositionOf(Position);

            Changed?.Invoke();
        }

        /// <summary>
        /// Moves the character to another cell. The visual position is handled by the mover,
        /// which interpolates between the two cells.
        /// </summary>
        /// <summary>
        /// Turns the character back to face the enemy half of the board.
        ///
        /// Called when a step finishes, and it is what keeps a sideways step from leaving the
        /// character standing in profile while it fights something above it. Facing sideways is
        /// a state of travel, not a state of rest.
        /// </summary>
        public void SettleFacing()
        {
            Facing = View.FacingResolver.Default(Team);
        }

        public void MoveTo(GridPosition destination)
        {
            // The direction is read before the position changes, since it is the difference
            // between the two that says which way the character turned.
            Facing = View.FacingResolver.FromStep(Position, destination, Facing);

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
