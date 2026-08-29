using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The cost a powerful ability charges its own user, against "### Dano em quem usou a
    /// habilidade" in abilities.md.
    ///
    /// The rule is that this damage never takes the user below 1 health. The promise has to be
    /// absolute, because the alternative — refusing to cast when the arithmetic would kill — looks
    /// at a number the preparation time can outdate, and disarms the character exactly when the
    /// battle got hard.
    ///
    /// The limit covers only the ability's own damage against its own user. Everything else still
    /// kills, or a character with a costly ability would be immortal by accident.
    /// </summary>
    public class AbilitySelfDamageTests
    {
        private TestBattle battle;
        private BattleRandom random;

        [SetUp]
        public void SetUp()
        {
            battle = new TestBattle();
            random = new BattleRandom(20260818);
        }

        [TearDown]
        public void TearDown()
        {
            battle.Dispose();
        }

        private Character Bare(string id, int column, int row, Team team)
        {
            return battle.Spawn(
                battle.Sheet(id, team == Team.Heroes ? CharacterKind.Hero : CharacterKind.Minion,
                    power: 0, agility: 0, constitution: 100, physicalArmor: 0),
                team, column, row);
        }

        private void Apply(Character user, AbilityDefinition ability, params Character[] targets)
        {
            AbilityResolver.Apply(user, ability, 1, new List<Character>(targets), battle.Grid, random, null);
        }

        private static AbilityDefinition Burn(float damage)
        {
            return TestAbility.Instant("warmUp")
                .Shaped(AbilityShape.Area)
                .WithDamage(damage, target: EffectTarget.Self);
        }

        [Test]
        public void SelfDamageStopsAtOneHealthInsteadOfKilling()
        {
            Character user = Bare("gadrat", 1, 1, Team.Heroes);
            user.TakeDamage(user.Stats.MaxHealth - 10);

            Apply(user, Burn(500f));

            Assert.IsTrue(user.IsAlive, "The ability killed the character that used it.");
            Assert.AreEqual(1, user.CurrentHealth);
        }

        /// <summary>
        /// The limit is a floor and not a discount. While there is health to pay with, the cost is
        /// charged in full, or the ability would be free for anybody near the top of their bar.
        /// </summary>
        [Test]
        public void SelfDamageWithinReachIsChargedInFull()
        {
            Character user = Bare("gadrat", 1, 1, Team.Heroes);
            int before = user.CurrentHealth;

            Apply(user, Burn(30f));

            Assert.AreEqual(before - 30, user.CurrentHealth);
        }

        [Test]
        public void AUserAlreadyAtOneHealthSurvivesItsOwnAbility()
        {
            Character user = Bare("gadrat", 1, 1, Team.Heroes);
            user.TakeDamage(user.Stats.MaxHealth - 1);

            Apply(user, Burn(500f));

            Assert.IsTrue(user.IsAlive);
            Assert.AreEqual(1, user.CurrentHealth);
        }

        /// <summary>
        /// The same ability, the same damage, aimed at somebody else. Without this the limit would
        /// have quietly turned into a defence for everyone an ability touches.
        /// </summary>
        [Test]
        public void TheSameAbilityStillKillsItsEnemies()
        {
            Character user = Bare("gadrat", 1, 1, Team.Heroes);
            Character victim = Bare("minion", 1, 2, Team.Enemies);

            Apply(user, TestAbility.Instant("hit").WithDamage(9999f), victim);

            Assert.IsFalse(victim.IsAlive);
            Assert.AreEqual(0, victim.CurrentHealth);
        }

        /// <summary>
        /// An ally's area damage is not the user's own cost, so it is not held back. This is the
        /// case the spec calls out by name, next to thorns and the basic attack.
        /// </summary>
        [Test]
        public void AnAllysAbilityStillKillsTheCharacterItCatches()
        {
            Character caster = Bare("ally", 1, 1, Team.Heroes);
            Character caught = Bare("gadrat", 1, 2, Team.Heroes);

            Apply(caster, TestAbility.Instant("blast").Shaped(AbilityShape.Area).WithDamage(9999f), caught);

            Assert.IsFalse(caught.IsAlive, "An ally's blast is not the user's own cost and has to kill normally.");
        }
    }
}
