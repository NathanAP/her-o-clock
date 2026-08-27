using HerOClock.Abilities;
using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The one rule about health that has no exceptions: "a vida atual nunca pode ultrapassar a vida
    /// máxima", in attributes.md.
    ///
    /// Damage and healing always respected it. What did not was the maximum **moving**, and this
    /// file is the history of that case shrinking:
    ///
    /// - it was found in 0.6.0.0, because a hero could take its points back and place them again;
    /// - 0.10.3.0 made points wait for the next stage, so a rebuild stopped moving it mid stage;
    /// - 0.10.4.0 made a combatant a photograph of a record, and the case stopped existing at all.
    ///
    /// What is left is the case that never went away and never will: **a debuff on CON lowers the
    /// maximum in the middle of a fight**, where no stage boundary is coming to tidy up. That is
    /// what the clamp in `RebuildStats` is for now, and the tests below are what say so.
    /// </summary>
    public class CharacterHealthTests
    {
        private TestBattle battle;

        [SetUp]
        public void SetUp()
        {
            battle = new TestBattle();
        }

        [TearDown]
        public void TearDown()
        {
            battle.Dispose();
        }

        private CharacterDefinition Sheet()
        {
            return battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20);
        }

        /// <summary>
        /// Rebuilding does not reach a fight in progress, because the fight is not reading the
        /// record. Nothing holds it back — there is simply nothing connecting the two.
        /// </summary>
        [Test]
        public void RebuildingTheRecordNeverTouchesTheCombatantFighting()
        {
            HeroRecord record = battle.Record(Sheet());
            record.AwardExperience(Progression.ExperienceTable.TotalXpTo(20));

            Character fighting = battle.SpawnHero(record, Team.Heroes, 2, 1);

            int maximum = fighting.Stats.MaxHealth;
            fighting.TakeDamage(maximum - 10);

            record.Attributes.SetAutomatic(false);
            record.Attributes.Reset();

            Assert.AreEqual(maximum, fighting.Stats.MaxHealth,
                "Taking every point back reached a combatant already fighting.");
            Assert.AreEqual(10, fighting.CurrentHealth, "The rebuild moved the health bar.");

            record.Attributes.Spend(Attribute.Constitution, record.Attributes.Unspent);

            Assert.AreEqual(maximum, fighting.Stats.MaxHealth,
                "Placing the points again reached it either.");
            Assert.AreEqual(10, fighting.CurrentHealth);
        }

        /// <summary>
        /// The next stage is what applies a rebuild, and the combatant it builds starts whole —
        /// not because anybody healed it, but because it has never been hit.
        /// </summary>
        [Test]
        public void TheNextStageBuildsTheRebuiltHero()
        {
            HeroRecord record = battle.Record(Sheet());
            record.AwardExperience(Progression.ExperienceTable.TotalXpTo(20));

            Character first = battle.SpawnHero(record, Team.Heroes, 2, 1);
            int before = first.Stats.MaxHealth;
            first.TakeDamage(before - 10);

            record.Attributes.SetAutomatic(false);
            record.Attributes.Reset();

            battle.Disband(first);
            Character second = battle.SpawnHero(record, Team.Heroes, 2, 1);

            Assert.Less(second.Stats.MaxHealth, before, "The next stage did not apply the rebuild.");
            Assert.AreEqual(second.Stats.MaxHealth, second.CurrentHealth,
                "A stage starts with the party whole.");
        }

        /// <summary>
        /// Levelling up mid stage changes the record and not the fight, so nothing about the
        /// combatant moves — not its level, not its points, not its maximum health.
        ///
        /// It replaces a test that asserted the maximum rising on the spot, which was the
        /// behaviour before any of this existed.
        /// </summary>
        [Test]
        public void LevellingUpMidStageLeavesTheCombatantExactlyAsItWas()
        {
            HeroRecord record = battle.Record(Sheet());
            Character fighting = battle.SpawnHero(record, Team.Heroes, 2, 1);

            fighting.TakeDamage(20);

            int hurt = fighting.CurrentHealth;
            int maximum = fighting.Stats.MaxHealth;

            fighting.AwardExperience(Progression.ExperienceTable.TotalXpTo(10));

            Assert.AreEqual(10, record.Level, "The experience did not reach the record.");
            Assert.AreEqual(1, fighting.Level, "The combatant followed the record's level.");
            Assert.AreEqual(maximum, fighting.Stats.MaxHealth, "The new levels reached the fight.");
            Assert.AreEqual(hurt, fighting.CurrentHealth, "Levelling up healed the hero.");

            battle.Disband(fighting);
            Character next = battle.SpawnHero(record, Team.Heroes, 2, 1);

            Assert.AreEqual(10, next.Level);
            Assert.Greater(next.Stats.MaxHealth, maximum, "The next stage did not apply the levels.");
            Assert.AreEqual(next.Stats.MaxHealth, next.CurrentHealth);
        }

        /// <summary>
        /// The clamp that is still load bearing, and the only case left that can move a maximum
        /// mid fight: a debuff on CON, which no stage boundary is coming to reconcile.
        /// </summary>
        [Test]
        public void ADebuffOnConstitutionNeverLeavesHealthAboveTheMaximum()
        {
            HeroRecord record = battle.Record(Sheet());
            Character hero = battle.SpawnHero(record, Team.Heroes, 2, 1);

            int before = hero.Stats.MaxHealth;

            hero.Modifiers.Apply("test-debuff", ModifiableStat.Constitution, StatModifierMode.Percent, -50f, 10f);

            Assert.Less(hero.Stats.MaxHealth, before, "The setup did not actually lower the maximum.");
            Assert.AreEqual(hero.Stats.MaxHealth, hero.CurrentHealth);
            Assert.LessOrEqual(hero.HealthFraction, 1f);
        }

        /// <summary>
        /// The clamp only bites when it has to. A hero already below the new maximum keeps exactly
        /// the health it had, so a debuff never doubles as extra damage.
        /// </summary>
        [Test]
        public void AHeroAlreadyBelowTheNewMaximumIsLeftAlone()
        {
            HeroRecord record = battle.Record(Sheet());
            Character hero = battle.SpawnHero(record, Team.Heroes, 2, 1);

            hero.TakeDamage(hero.Stats.MaxHealth - 10);
            Assert.AreEqual(10, hero.CurrentHealth, "The setup did not leave the hero on 10 health.");

            hero.Modifiers.Apply("test-debuff", ModifiableStat.Constitution, StatModifierMode.Percent, -50f, 10f);

            Assert.Greater(hero.Stats.MaxHealth, 10, "The setup needs a maximum that stays above the health.");
            Assert.AreEqual(10, hero.CurrentHealth);
        }
    }
}
