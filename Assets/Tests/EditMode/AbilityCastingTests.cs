using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// An ability actually being used: the phases running, the charge being held, and the cooldown.
    ///
    /// Driven a step at a time, the same way the battle director drives it, so the timings are the
    /// real ones rather than an approximation of them.
    /// </summary>
    public class AbilityCastingTests
    {
        private const float Step = BattleDirector.FixedStep;

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

        private AbilityCaster CasterFor(Character character)
        {
            return new AbilityCaster(character, battle.Grid, random);
        }

        /// <summary>
        /// Advances a caster for a number of seconds, starting an ability whenever it is idle.
        ///
        /// It ticks the character's own effects too, because that is what the battle director does
        /// and a status that never expires is not a state the game can be in.
        /// </summary>
        private static void Run(
            AbilityCaster caster,
            Character user,
            float seconds,
            IReadOnlyList<Character> allies,
            IReadOnlyList<Character> enemies)
        {
            int steps = UnityEngine.Mathf.RoundToInt(seconds / Step);

            for (int i = 0; i < steps; i++)
            {
                user.TickEffects(Step);
                caster.Tick(Step, allies, enemies);

                if (!caster.IsBusy)
                {
                    caster.TryStart(allies, enemies);
                }
            }
        }

        // --- The phases occupy the character ---

        /// <summary>
        /// The three phases are one occupied block. Ability B is 2 + 2 + 0, so the character is
        /// held for 4 seconds and free after them.
        /// </summary>
        [Test]
        public void TheCharacterIsHeldForTheWholeOfPreparationCastingAndRecoil()
        {
            CharacterDefinition sheet = battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20);
            sheet.Give(TestAbility.Timed("b", 2f, 2f, 0f, 30f).WithDamage(10f));

            Character user = battle.Spawn(sheet, Team.Heroes, 3, 1);
            Character enemy = battle.Spawn(
                battle.Sheet("enemy", CharacterKind.Minion, power: 1, constitution: 50), Team.Enemies, 3, 2);

            AbilityCaster caster = CasterFor(user);
            Character[] allies = { user };
            Character[] enemies = { enemy };

            caster.TryStart(allies, enemies);
            Assert.IsTrue(caster.IsBusy, "The ability never started.");

            Run(caster, user, 3.9f, allies, enemies);
            Assert.IsTrue(caster.IsBusy, "The character was freed before the four seconds were up.");

            Run(caster, user, 0.2f, allies, enemies);
            Assert.IsFalse(caster.IsBusy, "The character is still held after the whole block finished.");
        }

        /// <summary>
        /// The recovery holds the character even though the ability has already begun recharging.
        /// Ability C is 0 + 1 + 1: free at 2 seconds, recharging since 1.
        /// </summary>
        [Test]
        public void TheRecoveryStillHoldsTheCharacterAfterTheAbilityHappened()
        {
            CharacterDefinition sheet = battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20);
            sheet.Give(TestAbility.Timed("c", 0f, 1f, 1f, 30f).WithDamage(10f));

            Character user = battle.Spawn(sheet, Team.Heroes, 3, 1);
            Character enemy = battle.Spawn(
                battle.Sheet("enemy", CharacterKind.Minion, power: 1, constitution: 50), Team.Enemies, 3, 2);

            AbilityCaster caster = CasterFor(user);
            Character[] allies = { user };
            Character[] enemies = { enemy };

            caster.TryStart(allies, enemies);

            int before = enemy.CurrentHealth;

            Run(caster, user, 1.1f, allies, enemies);

            Assert.Less(enemy.CurrentHealth, before, "The damage had not landed by the end of the casting.");
            Assert.IsTrue(caster.IsBusy, "The recovery freed the character early.");

            Run(caster, user, 1f, allies, enemies);
            Assert.IsFalse(caster.IsBusy);
        }

        // --- Holding the charge ---

        /// <summary>
        /// A ready ability with nowhere to go waits. It is never fired into empty space, and it
        /// does not go on cooldown for having tried.
        /// </summary>
        [Test]
        public void AReadyAbilityWithNoTargetHoldsItsCharge()
        {
            CharacterDefinition sheet = battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20);
            sheet.Give(TestAbility.Timed("a", 0f, 0f, 0f, 30f).Shaped(AbilityShape.Single, AbilityWho.Enemies, 1).WithDamage(10f));

            Character user = battle.Spawn(sheet, Team.Heroes, 3, 1);
            Character far = battle.Spawn(
                battle.Sheet("enemy", CharacterKind.Minion, power: 1, constitution: 50), Team.Enemies, 3, 8);

            AbilityCaster caster = CasterFor(user);
            Character[] allies = { user };
            Character[] enemies = { far };

            Run(caster, user, 2f, allies, enemies);

            Assert.IsFalse(caster.IsBusy, "It started an ability with nobody in range.");
            Assert.AreEqual(far.Stats.MaxHealth, far.CurrentHealth, "It hit somebody out of range.");
        }

        // --- The cooldown ---

        [Test]
        public void AnAbilityCannotBeUsedAgainUntilItsCooldownIsUp()
        {
            CharacterDefinition sheet = battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20);
            sheet.Give(TestAbility.Timed("a", 0f, 0f, 0f, 5f).WithDamage(10f));

            Character user = battle.Spawn(sheet, Team.Heroes, 3, 1);
            Character enemy = battle.Spawn(
                battle.Sheet("enemy", CharacterKind.Minion, power: 1, constitution: 500), Team.Enemies, 3, 2);

            AbilityCaster caster = CasterFor(user);
            Character[] allies = { user };
            Character[] enemies = { enemy };

            Run(caster, user, 4f, allies, enemies);
            int afterOne = enemy.CurrentHealth;

            Run(caster, user, 0.5f, allies, enemies);
            Assert.AreEqual(afterOne, enemy.CurrentHealth, "It fired again before the cooldown was up.");

            Run(caster, user, 1.5f, allies, enemies);
            Assert.Less(enemy.CurrentHealth, afterOne, "It never fired a second time.");
        }

        /// <summary>
        /// Silence cancels a preparation, and abilities.md sends the cancelled ability to **half**
        /// its cooldown rather than to none of it.
        /// </summary>
        [Test]
        public void SilenceCancelsAPreparationAndCostsHalfTheCooldown()
        {
            CharacterDefinition sheet = battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20);
            sheet.Give(TestAbility.Timed("b", 2f, 0f, 0f, 10f).WithDamage(10f));

            Character user = battle.Spawn(sheet, Team.Heroes, 3, 1);
            Character enemy = battle.Spawn(
                battle.Sheet("enemy", CharacterKind.Minion, power: 1, constitution: 500), Team.Enemies, 3, 2);

            AbilityCaster caster = CasterFor(user);
            Character[] allies = { user };
            Character[] enemies = { enemy };

            caster.TryStart(allies, enemies);
            Assert.IsTrue(caster.IsBusy);

            user.Statuses.Apply(StatusKind.Silenced, 0.5f);
            caster.Tick(Step, allies, enemies);

            Assert.IsFalse(caster.IsBusy, "Silence did not cancel the preparation.");

            // Half of ten seconds is five, and only then does the two second wind up start again.
            // So nothing can have landed at four, and it has to have landed by eight.
            Run(caster, user, 4f, allies, enemies);
            Assert.AreEqual(enemy.Stats.MaxHealth, enemy.CurrentHealth, "It came back before half the cooldown.");

            Run(caster, user, 4f, allies, enemies);
            Assert.Less(enemy.CurrentHealth, enemy.Stats.MaxHealth, "It never came back after half the cooldown.");
        }

        [Test]
        public void ASilencedCharacterCannotStartAnAbilityAtAll()
        {
            CharacterDefinition sheet = battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20);
            sheet.Give(TestAbility.Timed("a", 0f, 0f, 0f, 0f).WithDamage(10f));

            Character user = battle.Spawn(sheet, Team.Heroes, 3, 1);
            Character enemy = battle.Spawn(
                battle.Sheet("enemy", CharacterKind.Minion, power: 1, constitution: 500), Team.Enemies, 3, 2);

            user.Statuses.Apply(StatusKind.Silenced, 5f);

            AbilityCaster caster = CasterFor(user);
            Run(caster, user, 2f, new[] { user }, new[] { enemy });

            Assert.AreEqual(enemy.Stats.MaxHealth, enemy.CurrentHealth);
        }

        // --- The basic attack keeps priority ---

        /// <summary>
        /// The attack timer keeps running while the character is busy, and is held at zero rather
        /// than banking blows. One blow lands in the first gap, not four.
        /// </summary>
        [Test]
        public void TheBasicAttackDoesNotBankBlowsWhileAnAbilityIsRunning()
        {
            CharacterDefinition sheet = battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20);
            sheet.Give(TestAbility.Timed("b", 2f, 2f, 0f, 30f).WithBuff(ModifiableStat.Power, 0f, 1f));

            Character user = battle.Spawn(sheet, Team.Heroes, 3, 1);
            Character enemy = battle.Spawn(
                battle.Sheet("enemy", CharacterKind.Minion, power: 1, constitution: 500), Team.Enemies, 3, 2);

            AbilityCaster caster = CasterFor(user);
            CharacterAttacker attacker = new CharacterAttacker(user, random);

            int blows = 0;
            attacker.Attacked += (a, t, r) => blows++;

            Character[] allies = { user };
            Character[] enemies = { enemy };

            // Four seconds held by the ability, then a single step of freedom.
            for (int i = 0; i < UnityEngine.Mathf.RoundToInt(4f / Step); i++)
            {
                caster.Tick(Step, allies, enemies);

                if (!caster.IsBusy)
                {
                    caster.TryStart(allies, enemies);
                }

                attacker.Tick(Step, enemies, caster.IsBusy);
            }

            Assert.LessOrEqual(blows, 1,
                "The attack timer banked blows during the ability, so several landed at once when it ended.");
        }
    }
}
