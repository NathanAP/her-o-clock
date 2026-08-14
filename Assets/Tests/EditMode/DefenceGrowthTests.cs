using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks that defence grows with the level, against the examples in attributes.md.
    ///
    /// The rule exists because the mitigation curve's constant is 50 times the attacker's level.
    /// A flat armour value is worth less every level, so a character with no new source of armour
    /// rots on its own: before this, the villain went from 56% mitigation at level 1 to 4% at
    /// level 50 without anybody touching it.
    /// </summary>
    public class DefenceGrowthTests
    {
        private const float Tolerance = 0.05f;

        private static CharacterStats Sheet(int baseArmor, int armorPerLevel, int level, float multiplier = 1f)
        {
            CharacterStats stats = new CharacterStats
            {
                BasePower = 10,
                BaseConstitution = 10,
                Equipment = EquipmentClass.Heavy,
                BasePhysicalArmor = baseArmor,
                PhysicalArmorPerLevel = armorPerLevel
            };

            stats.ApplyInstance(level, new AttributeGrowth(), multiplier);
            return stats;
        }

        // --- The formula: base + growth x (level - 1) ---

        [TestCase(1, 100)]
        [TestCase(2, 200)]
        [TestCase(12, 1200)]
        [TestCase(50, 5000)]
        public void ArmourFollowsTheGrowthFormula(int level, int expected)
        {
            Assert.AreEqual(expected, Sheet(100, 100, level).PhysicalArmor);
        }

        [Test]
        public void WithoutGrowthTheArmourStaysWhereItStarted()
        {
            Assert.AreEqual(100, Sheet(100, 0, 50).PhysicalArmor);
        }

        [Test]
        public void ASheetWithNoArmourStaysWithNone()
        {
            Assert.AreEqual(0, Sheet(0, 0, 40).PhysicalArmor);
        }

        // --- The property the rule exists for ---

        /// <summary>
        /// attributes.md: growth equal to the base holds mitigation against a same level attacker
        /// still for the whole game. 100 and 100 gives 50% at every level, 150 and 150 gives
        /// 56.25%, which are the two values the current sheets are built on.
        /// </summary>
        [TestCase(100, 50.0f)]
        [TestCase(150, 56.25f)]
        [TestCase(10, 12.5f)]
        public void GrowthEqualToTheBaseHoldsMitigationStill(int value, float expected)
        {
            int[] levels = { 1, 2, 12, 30, 50, 100 };

            for (int i = 0; i < levels.Length; i++)
            {
                int level = levels[i];
                float mitigation = Sheet(value, value, level).PhysicalMitigationAgainst(level);

                Assert.AreEqual(expected, mitigation, Tolerance,
                    "Mitigation moved at level " + level + ".");
            }
        }

        /// <summary>
        /// The failure this whole version is about, kept as a test so it cannot come back: with no
        /// growth, mitigation collapses as the game goes on.
        /// </summary>
        [Test]
        public void WithoutGrowthMitigationCollapsesAsLevelsRise()
        {
            CharacterStats stuck = Sheet(150, 0, 50);

            Assert.AreEqual(56.25f, Sheet(150, 0, 1).PhysicalMitigationAgainst(1), Tolerance);
            Assert.Less(stuck.PhysicalMitigationAgainst(50), 10f,
                "This is the behaviour the growth field exists to replace.");
        }

        /// <summary>Growth below the base loses ground slowly, above it gains. Both are valid.</summary>
        [Test]
        public void GrowthBelowTheBaseLosesGroundAndAboveItGains()
        {
            Assert.Less(Sheet(100, 50, 50).PhysicalMitigationAgainst(50), 50f);
            Assert.Greater(Sheet(100, 200, 50).PhysicalMitigationAgainst(50), 50f);
        }

        // --- The stage multiplier ---

        /// <summary>
        /// fases.md says the multiplier scales that enemy's attributes. It has to reach the
        /// defences too, otherwise a weakened summon keeps the armour of a full strength one.
        /// </summary>
        [Test]
        public void TheStageMultiplierReachesTheDefences()
        {
            Assert.AreEqual(1200, Sheet(100, 100, 12).PhysicalArmor);
            Assert.AreEqual(600, Sheet(100, 100, 12, 0.5f).PhysicalArmor);
            Assert.AreEqual(2400, Sheet(100, 100, 12, 2f).PhysicalArmor);
        }

        [Test]
        public void AMultiplierNeverPushesDefenceBelowZero()
        {
            Assert.AreEqual(0, Sheet(100, 100, 12, 0f).PhysicalArmor);
        }

        // --- The three elemental resistances are independent ---

        [Test]
        public void EachResistanceGrowsOnItsOwn()
        {
            CharacterStats stats = new CharacterStats
            {
                BaseFireResistance = 40,
                FireResistancePerLevel = 40,
                BaseWaterResistance = 10,
                WaterResistancePerLevel = 0
            };

            stats.ApplyInstance(5, new AttributeGrowth(), 1f);

            Assert.AreEqual(200, stats.FireResistance, "Fire should have grown.");
            Assert.AreEqual(10, stats.WaterResistance, "Water declares no growth.");
            Assert.AreEqual(0, stats.ElectricResistance, "Electric was never given anything.");
        }

        // --- Instances stay independent ---

        [Test]
        public void TwoCharactersFromTheSameSheetDoNotShareTheirDefence()
        {
            CharacterStats sheet = new CharacterStats
            {
                BasePhysicalArmor = 100,
                PhysicalArmorPerLevel = 100
            };

            CharacterStats weak = sheet.Clone();
            weak.ApplyInstance(1, new AttributeGrowth(), 1f);

            CharacterStats strong = sheet.Clone();
            strong.ApplyInstance(20, new AttributeGrowth(), 1f);

            Assert.AreEqual(100, weak.PhysicalArmor);
            Assert.AreEqual(2000, strong.PhysicalArmor);
        }
    }
}
