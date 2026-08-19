using HerOClock.Battle;
using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks health regeneration against attributes.md.
    ///
    /// The attribute was defined in the spec long before it existed in the game, and the spec was
    /// incomplete: it promised 0.5% of regeneration speed per point of POW without saying a
    /// percentage of what. The answer settled here is that POW multiplies rather than grants, so a
    /// character with no source of regeneration still has none.
    /// </summary>
    public class HealthRegenerationTests
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

        private static CharacterStats Sheet(float baseRegen, int constitution)
        {
            CharacterStats stats = new CharacterStats
            {
                BaseConstitution = constitution,
                BaseHealthRegen = baseRegen
            };

            stats.ApplyInstance(1, new AttributeGrowth(), 1f);
            return stats;
        }

        // --- The formula ---

        [Test]
        public void NobodyRegeneratesJustByExisting()
        {
            Assert.AreEqual(0f, Sheet(0f, 0).HealthPerSecond, 0.0001f);
        }

        /// <summary>
        /// The point of the rule: POW alone grants nothing, no matter how much of it there is.
        /// </summary>
        [Test]
        public void PowerAloneGrantsNoRegeneration()
        {
            Assert.AreEqual(0f, Sheet(0f, 500).HealthPerSecond, 0.0001f);
        }

        /// <summary>The spec's example: 10 of base regeneration with 40 CON gives 12 per second.</summary>
        [Test]
        public void ConstitutionMultipliesWhateverRegenerationExists()
        {
            Assert.AreEqual(12f, Sheet(10f, 40).HealthPerSecond, 0.0001f);
        }

        [Test]
        public void WithoutConstitutionTheBaseRateIsUntouched()
        {
            Assert.AreEqual(10f, Sheet(10f, 0).HealthPerSecond, 0.0001f);
        }

        [Test]
        public void EachPointOfConstitutionAddsHalfAPercent()
        {
            Assert.AreEqual(0.005f, CharacterStats.HealthRegenPerConstitution, 0.00001f);
            Assert.AreEqual(200f, Sheet(100f, 200).HealthPerSecond, 0.0001f);
        }

        // --- The tick ---

        private Character Wounded(float baseRegen, int damage)
        {
            CharacterDefinition sheet = battle.Sheet("regenerator", CharacterKind.Hero, power: 0, constitution: 100);
            sheet.Stats.BaseHealthRegen = baseRegen;

            Character character = battle.Spawn(sheet, Team.Heroes, 3, 3);
            character.TakeDamage(damage);

            return character;
        }

        /// <summary>
        /// A realistic rate is a tiny fraction of a point per step, so the remainder has to be
        /// carried. Rounding each step on its own would floor every rate to nothing.
        /// </summary>
        [Test]
        public void ARateSmallerThanOnePointPerStepStillHeals()
        {
            Character character = Wounded(0.5f, 500);
            int wounded = character.CurrentHealth;

            // A base of 0.5 with 100 CON regenerates 0.75 per second, since CON multiplies whatever
            // regeneration exists. Ten seconds of that is seven and a half points, and the halves
            // only survive because the remainder is carried between steps.
            for (int step = 0; step < 600; step++)
            {
                character.Regenerate(BattleDirector.FixedStep);
            }

            Assert.AreEqual(wounded + 7, character.CurrentHealth, 1,
                "0.75 per second over ten seconds is seven and a half points.");
        }

        [Test]
        public void RegenerationNeverGoesPastMaximumHealth()
        {
            Character character = Wounded(100f, 10);

            for (int step = 0; step < 600; step++)
            {
                character.Regenerate(BattleDirector.FixedStep);
            }

            Assert.AreEqual(character.Stats.MaxHealth, character.CurrentHealth);
        }

        /// <summary>
        /// Nobody banks healing by standing intact. Without dropping the carry at full health, a
        /// character could sit unharmed for a minute and then absorb the first blow for free.
        /// </summary>
        [Test]
        public void TimeSpentAtFullHealthIsNotBanked()
        {
            CharacterDefinition sheet = battle.Sheet("regenerator", CharacterKind.Hero, power: 0, constitution: 100);
            sheet.Stats.BaseHealthRegen = 0.9f;

            Character character = battle.Spawn(sheet, Team.Heroes, 3, 3);

            // A minute at full health, which would otherwise pile up 54 points of carry.
            for (int step = 0; step < 3600; step++)
            {
                character.Regenerate(BattleDirector.FixedStep);
            }

            character.TakeDamage(500);
            int wounded = character.CurrentHealth;

            character.Regenerate(BattleDirector.FixedStep);

            Assert.AreEqual(wounded, character.CurrentHealth, "A banked burst of healing came out of nowhere.");
        }

        [Test]
        public void TheFallenDoNotRegenerate()
        {
            Character character = Wounded(100f, 100000);

            Assert.IsFalse(character.IsAlive);

            for (int step = 0; step < 600; step++)
            {
                character.Regenerate(BattleDirector.FixedStep);
            }

            Assert.AreEqual(0, character.CurrentHealth, "A fallen hero healed itself back up.");
            Assert.IsFalse(character.IsAlive);
        }

        [Test]
        public void ACharacterWithNoRegenerationNeverHeals()
        {
            Character character = Wounded(0f, 500);
            int wounded = character.CurrentHealth;

            for (int step = 0; step < 3600; step++)
            {
                character.Regenerate(BattleDirector.FixedStep);
            }

            Assert.AreEqual(wounded, character.CurrentHealth);
        }

        /// <summary>
        /// Regeneration is fed the fixed step like everything else, so how the time arrives cannot
        /// change how much is recovered.
        /// </summary>
        [Test]
        public void TheAmountRecoveredDoesNotDependOnHowTheTimeIsDelivered()
        {
            Character steady = Wounded(3f, 500);
            for (int step = 0; step < 300; step++)
            {
                steady.Regenerate(BattleDirector.FixedStep);
            }

            Character chunky = Wounded(3f, 500);
            for (int step = 0; step < 150; step++)
            {
                chunky.Regenerate(BattleDirector.FixedStep);
                chunky.Regenerate(BattleDirector.FixedStep);
            }

            Assert.AreEqual(steady.CurrentHealth, chunky.CurrentHealth);
        }
    }
}
