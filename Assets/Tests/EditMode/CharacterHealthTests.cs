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
        /// Taking every point back lowers the maximum, since the points were what was holding it up.
        /// Before this was fixed the hero kept the health the old maximum allowed, and the health bar
        /// drew past its own end.
        /// </summary>
        [Test]
        public void TakingAttributePointsBackNeverLeavesHealthAboveTheMaximum()
        {
            Character hero = Hero(20);

            int before = hero.Stats.MaxHealth;

            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();

            Assert.Less(hero.Stats.MaxHealth, before, "The setup did not actually lower the maximum.");
            Assert.AreEqual(hero.Stats.MaxHealth, hero.CurrentHealth);
            Assert.LessOrEqual(hero.HealthFraction, 1f);
        }

        /// <summary>
        /// Placing the points again raises the maximum, and the health stays where it was. A rebuild
        /// is not a potion either.
        /// </summary>
        [Test]
        public void PlacingPointsAgainRaisesTheMaximumWithoutHealing()
        {
            Character hero = Hero(20);

            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();

            int afterReset = hero.CurrentHealth;

            hero.Attributes.Spend(Attribute.Constitution, hero.Attributes.Unspent);

            Assert.Greater(hero.Stats.MaxHealth, afterReset, "Spending the points did not raise the maximum.");
            Assert.AreEqual(afterReset, hero.CurrentHealth, "Rebuilding a character healed it.");
        }

        /// <summary>
        /// The clamp only bites when it has to. A hero already below the new maximum keeps exactly
        /// the health it had, so rebuilding never doubles as a punishment either.
        /// </summary>
        [Test]
        public void AHeroAlreadyBelowTheNewMaximumIsLeftAlone()
        {
            Character hero = Hero(20);
            hero.TakeDamage(hero.Stats.MaxHealth - 10);

            Assert.AreEqual(10, hero.CurrentHealth, "The setup did not leave the hero on 10 health.");

            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();

            Assert.Greater(hero.Stats.MaxHealth, 10, "The setup needs a maximum that stays above the hero's health.");
            Assert.AreEqual(10, hero.CurrentHealth);
        }

        /// <summary>
        /// Levelling up is the normal case, and it must keep working exactly as it did: the maximum
        /// rises and the current health is left alone, so a level is never a free potion in the
        /// middle of a stage.
        /// </summary>
        [Test]
        public void LevellingUpRaisesTheMaximumAndLeavesTheDamageAlone()
        {
            Character hero = Hero(1);
            hero.TakeDamage(20);

            int hurt = hero.CurrentHealth;
            int before = hero.Stats.MaxHealth;

            hero.AwardExperience(Progression.ExperienceTable.TotalXpTo(10));

            Assert.Greater(hero.Stats.MaxHealth, before);
            Assert.AreEqual(hurt, hero.CurrentHealth, "Levelling up healed the hero.");
        }
    }
}
