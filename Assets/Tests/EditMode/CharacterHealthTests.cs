using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The one rule about health that has no exceptions: "a vida atual nunca pode ultrapassar a vida
    /// máxima", in attributes.md.
    ///
    /// Damage and healing always respected it. What did not was the maximum **moving**, which is a
    /// case that only exists because a hero can take its attribute points back and place them again.
    /// Found while building the save in 0.6.0.0, of all places, by a test whose setup rebuilt a hero
    /// and then could not explain the health it was looking at.
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

        private Character Hero(int level)
        {
            return battle.Spawn(
                battle.Sheet("hero", CharacterKind.Hero, power: 10, constitution: 20),
                Team.Heroes, 2, 1, level);
        }

        /// <summary>
        /// Taking every point back does nothing until the next stage, and then the health that
        /// comes back is the health the new maximum allows.
        ///
        /// The clamp in <c>OnAttributesChanged</c> is still what guarantees the second half, and
        /// it is still needed — a debuff on CON can lower the maximum in the middle of a fight,
        /// where no stage boundary is coming to tidy up.
        /// </summary>
        [Test]
        public void TakingAttributePointsBackWaitsForTheNextStage()
        {
            Character hero = Hero(20);

            int before = hero.Stats.MaxHealth;

            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();

            Assert.AreEqual(before, hero.Stats.MaxHealth,
                "The rebuild took effect in the middle of a stage.");
            Assert.AreEqual(before, hero.CurrentHealth,
                "The rebuild moved the health it was not allowed to touch yet.");

            hero.ResetForBattle();

            Assert.Less(hero.Stats.MaxHealth, before, "The next stage did not apply the rebuild.");
            Assert.AreEqual(hero.Stats.MaxHealth, hero.CurrentHealth);
            Assert.LessOrEqual(hero.HealthFraction, 1f);
        }

        /// <summary>
        /// Placing the points again raises the maximum, and it raises it on the same schedule:
        /// not now, next stage.
        /// </summary>
        [Test]
        public void PlacingPointsAgainAlsoWaitsForTheNextStage()
        {
            Character hero = Hero(20);

            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();
            hero.ResetForBattle();

            int afterReset = hero.Stats.MaxHealth;

            hero.Attributes.Spend(Attribute.Constitution, hero.Attributes.Unspent);

            Assert.AreEqual(afterReset, hero.Stats.MaxHealth,
                "Spending the points raised the maximum in the middle of a stage.");

            hero.ResetForBattle();

            Assert.Greater(hero.Stats.MaxHealth, afterReset, "The next stage did not apply the build.");
        }

        /// <summary>
        /// Damage taken during a stage survives everything the player does to the build during
        /// that stage. A rebuild is neither a potion nor a punishment while the fight is on.
        /// </summary>
        [Test]
        public void RebuildingMidStageNeverMovesTheHealthBar()
        {
            Character hero = Hero(20);
            hero.TakeDamage(hero.Stats.MaxHealth - 10);

            Assert.AreEqual(10, hero.CurrentHealth, "The setup did not leave the hero on 10 health.");

            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();
            hero.Attributes.Spend(Attribute.Constitution, hero.Attributes.Unspent);

            Assert.AreEqual(10, hero.CurrentHealth);
        }

        /// <summary>
        /// Levelling up mid stage does not move a single attribute, and therefore does not move
        /// the maximum health either.
        ///
        /// This is the automatic distribution obeying the same rule as the player, which is the
        /// part of "Um ponto colocado só passa a valer na próxima fase" that is easiest to forget:
        /// the automatic decides *where* a point goes, never *when* it counts.
        ///
        /// It replaces an older test that asserted the maximum rising on the spot. That was the
        /// behaviour before the rule existed, and the change is deliberate.
        /// </summary>
        [Test]
        public void LevellingUpMidStageLeavesTheAttributesWhereTheyWere()
        {
            Character hero = Hero(1);
            hero.TakeDamage(20);

            int hurt = hero.CurrentHealth;
            int before = hero.Stats.MaxHealth;

            hero.AwardExperience(Progression.ExperienceTable.TotalXpTo(10));

            Assert.AreEqual(10, hero.Level, "The setup did not actually reach level 10.");
            Assert.AreEqual(before, hero.Stats.MaxHealth,
                "The points of the new levels took effect in the middle of a stage.");
            Assert.AreEqual(hurt, hero.CurrentHealth, "Levelling up healed the hero.");

            hero.ResetForBattle();

            Assert.Greater(hero.Stats.MaxHealth, before, "The next stage did not apply the new levels.");
            Assert.AreEqual(hero.Stats.MaxHealth, hero.CurrentHealth, "A stage starts with the party whole.");
        }

        /// <summary>
        /// The armour a level buys arrives immediately, unlike the points.
        ///
        /// The two halves of a level up are on different schedules on purpose, per attributes.md:
        /// growth per level exists so a character does not rot against stronger enemies, and
        /// holding it back to the next stage would work against the reason it exists.
        /// </summary>
        [Test]
        public void TheArmourALevelBuysArrivesImmediately()
        {
            CharacterDefinition sheet = battle.Sheet("armoured", CharacterKind.Hero,
                power: 10, constitution: 20, physicalArmor: 20);
            sheet.Stats.PhysicalArmorPerLevel = 20;

            Character hero = battle.Spawn(sheet, Team.Heroes, 2, 1, level: 1);
            int before = hero.Stats.PhysicalArmor;

            hero.AwardExperience(Progression.ExperienceTable.TotalXpTo(10));

            Assert.Greater(hero.Stats.PhysicalArmor, before,
                "Armour that grows per level waited for the next stage, which it must not.");
        }
    }
}
