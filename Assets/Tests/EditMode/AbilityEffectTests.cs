using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// What an ability does once it has found its targets, against "## Efeitos" in abilities.md.
    ///
    /// The order the effects are written in is load-bearing, and so is the fact that they are
    /// independent: one that cannot happen does not cancel the ones around it.
    /// </summary>
    public class AbilityEffectTests
    {
        private TestBattle battle;
        private BattleRandom random;

        [SetUp]
        public void SetUp()
        {
            battle = new TestBattle();
            random = new BattleRandom(20260817);
        }

        [TearDown]
        public void TearDown()
        {
            battle.Dispose();
        }

        private void Apply(Character user, AbilityDefinition ability, params Character[] targets)
        {
            AbilityResolver.Apply(user, ability, 1, new List<Character>(targets), battle.Grid, random, null);
        }

        private Character Body(string id, int column, int row, Team team, int power = 10, int constitution = 50)
        {
            return battle.Spawn(
                battle.Sheet(id, team == Team.Heroes ? CharacterKind.Hero : CharacterKind.Minion,
                    power: power, constitution: constitution),
                team, column, row);
        }

        // --- Damage ---

        [Test]
        public void DamageLandsOnEveryTarget()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);
            Character first = Body("a", 3, 2, Team.Enemies);
            Character second = Body("b", 4, 2, Team.Enemies);

            Apply(user, TestAbility.Instant("hit").WithDamage(20f), first, second);

            Assert.Less(first.CurrentHealth, first.Stats.MaxHealth);
            Assert.Less(second.CurrentHealth, second.Stats.MaxHealth);
        }

        /// <summary>
        /// The scaling is what ties an ability to the build behind it. Without it the same ability
        /// would be worth the same on a character that invested in it and one that did not.
        /// </summary>
        [Test]
        public void DamageCountsTheUsersAttributes()
        {
            Character weak = Body("weak", 2, 1, Team.Heroes, power: 10);
            Character strong = Body("strong", 4, 1, Team.Heroes, power: 100);

            Character hitByWeak = Body("a", 2, 2, Team.Enemies);
            Character hitByStrong = Body("b", 4, 2, Team.Enemies);

            AbilityDefinition ability = TestAbility.Instant("hit").WithDamage(10f, powerScaling: 1f);

            Apply(weak, ability, hitByWeak);
            Apply(strong, ability, hitByStrong);

            Assert.Less(hitByStrong.CurrentHealth, hitByWeak.CurrentHealth,
                "The user's power made no difference, so the scaling is being ignored.");
        }

        [Test]
        public void FirstTargetHitsOnlyTheOneAtTheHeadOfTheList()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);
            Character first = Body("a", 3, 2, Team.Enemies);
            Character second = Body("b", 4, 2, Team.Enemies);

            AbilityDefinition ability = TestAbility.Instant("hit").With(new AbilityEffect
            {
                Type = EffectType.DealDamage,
                Target = EffectTarget.FirstTarget,
                DamageType = DamageType.Physical,
                Base = RankedValue.Constant(20f),
                Scaling = new AbilityScaling()
            });

            Apply(user, ability, first, second);

            Assert.Less(first.CurrentHealth, first.Stats.MaxHealth);
            Assert.AreEqual(second.Stats.MaxHealth, second.CurrentHealth);
        }

        // --- The order effects are written in ---

        /// <summary>
        /// The example abilities.md gives: an ability that makes its user intangible has to apply
        /// the status **before** the damage, or the damage would land on it first.
        /// </summary>
        [Test]
        public void EffectsResolveInTheOrderTheyAreWritten()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);

            AbilityDefinition ability = TestAbility.Instant("blink")
                .WithStatus(StatusKind.Intangible, 1f, EffectTarget.Self)
                .With(new AbilityEffect
                {
                    Type = EffectType.DealDamage,
                    Target = EffectTarget.Self,
                    DamageType = DamageType.Physical,
                    Base = RankedValue.Constant(9999f),
                    Scaling = new AbilityScaling()
                });

            Apply(user, ability, user);

            Assert.AreEqual(user.Stats.MaxHealth, user.CurrentHealth,
                "The damage landed before the status, so the order is not being honoured.");
        }

        [Test]
        public void IntangibleStopsDamageFromEverySource()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);
            Character target = Body("a", 3, 2, Team.Enemies);

            target.Statuses.Apply(StatusKind.Intangible, 5f);

            Apply(user, TestAbility.Instant("hit").WithDamage(50f), target);
            target.TakeDamage(50);

            Assert.AreEqual(target.Stats.MaxHealth, target.CurrentHealth);
        }

        // --- Buffs ---

        /// <summary>
        /// A buff has to come out the other side of the stats, which is the seam architecture.md
        /// promised. Reading the modifier list is not enough: what matters is that the derived
        /// value moved.
        /// </summary>
        [Test]
        public void ABuffChangesTheStatItNames()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);

            float before = user.Stats.AttacksPerSecond;

            Apply(user, TestAbility.Instant("haste")
                .WithBuff(ModifiableStat.AttackSpeed, 20f, 5f, EffectTarget.Self), user);

            Assert.AreEqual(before * 1.2f, user.Stats.AttacksPerSecond, 0.0001f);
        }

        [Test]
        public void ADebuffIsTheSameEffectWithTheOppositeSign()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);
            Character target = Body("a", 3, 2, Team.Enemies);

            float before = target.Stats.CellsPerSecond;

            Apply(user, TestAbility.Instant("slow")
                .WithBuff(ModifiableStat.MovementSpeed, -10f, 5f), target);

            Assert.AreEqual(before * 0.9f, target.Stats.CellsPerSecond, 0.0001f);
        }

        [Test]
        public void ABuffOnAPrimaryAttributeReachesEverythingDerivedFromIt()
        {
            Character user = Body("hero", 3, 1, Team.Heroes, power: 10, constitution: 10);

            int before = user.Stats.MaxHealth;

            Apply(user, TestAbility.Instant("grow")
                .WithBuff(ModifiableStat.Power, 10f, 5f, EffectTarget.Self, StatModifierMode.Flat), user);

            Assert.AreEqual(before + 50, user.Stats.MaxHealth,
                "Maximum health is power x 5 plus constitution x 10, so ten more power is fifty more health.");
        }

        [Test]
        public void ABuffRunsOutOnItsOwnTime()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);

            float before = user.Stats.AttacksPerSecond;

            Apply(user, TestAbility.Instant("haste")
                .WithBuff(ModifiableStat.AttackSpeed, 20f, 2f, EffectTarget.Self), user);

            user.TickEffects(1f);
            Assert.Greater(user.Stats.AttacksPerSecond, before, "The buff wore off before its duration was up.");

            user.TickEffects(1.1f);
            Assert.AreEqual(before, user.Stats.AttacksPerSecond, 0.0001f);
        }

        // --- Statuses ---

        [Test]
        public void ATauntPointsTheTargetAtWhoeverAppliedIt()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);
            Character target = Body("a", 3, 2, Team.Enemies);

            Apply(user, TestAbility.Instant("provoke").WithStatus(StatusKind.Taunted, 3f), target);

            Assert.AreSame(user, target.TauntedBy);
        }

        [Test]
        public void ATauntIsForgottenWhenItRunsOut()
        {
            Character user = Body("hero", 3, 1, Team.Heroes);
            Character target = Body("a", 3, 2, Team.Enemies);

            Apply(user, TestAbility.Instant("provoke").WithStatus(StatusKind.Taunted, 1f), target);

            target.TickEffects(1.1f);

            Assert.IsNull(target.TauntedBy, "The character kept chasing somebody who stopped taunting it.");
        }

        // --- Repositioning ---

        [Test]
        public void MoveToPutsTheUserBesideTheLastTarget()
        {
            Character user = Body("hero", 1, 1, Team.Heroes);
            Character target = Body("a", 4, 4, Team.Enemies);

            Apply(user, TestAbility.Instant("blink").WithMove(), target);

            Assert.AreEqual(1, GridPosition.Distance(user.Position, target.Position),
                "The user did not end up next to the target.");
        }

        /// <summary>
        /// The order is fixed rather than "the nearest side", because the same battle from the same
        /// seed has to end the same way. Lowest row first, then lowest column.
        /// </summary>
        [Test]
        public void MoveToTakesTheSidesInAFixedOrder()
        {
            Character user = Body("hero", 1, 1, Team.Heroes);
            Character target = Body("a", 4, 4, Team.Enemies);

            Apply(user, TestAbility.Instant("blink").WithMove(), target);

            Assert.AreEqual(new GridPosition(4, 3), user.Position,
                "Expected the cell below the target, which is the lowest row of the four sides.");
        }

        /// <summary>
        /// With nowhere to go the character stays put and the rest of the ability still happens.
        /// An ability never fails whole because one effect did not fit.
        /// </summary>
        [Test]
        public void MoveToWithNoFreeSideLeavesTheCharacterWhereItIs()
        {
            Character user = Body("hero", 1, 1, Team.Heroes);
            Character target = Body("a", 4, 4, Team.Enemies);

            Body("n", 4, 3, Team.Enemies);
            Body("s", 4, 5, Team.Enemies);
            Body("w", 3, 4, Team.Enemies);
            Body("e", 5, 4, Team.Enemies);

            GridPosition before = user.Position;

            Apply(user, TestAbility.Instant("blink").WithDamage(20f).WithMove(), target);

            Assert.AreEqual(before, user.Position, "It found a cell that was not free.");
            Assert.Less(target.CurrentHealth, target.Stats.MaxHealth,
                "The damage was skipped because the reposition could not happen.");
        }
    }
}
