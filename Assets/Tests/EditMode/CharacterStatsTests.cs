using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks the conversion from primary attributes into secondary ones against the numbers
    /// written in attributes.md.
    ///
    /// Every expected value here was copied from the spec, never from running the code. That is
    /// the whole point: a value read off the output would only confirm that the code does what
    /// the code does, and would happily protect a wrong formula forever.
    /// </summary>
    public class CharacterStatsTests
    {
        private const float Tolerance = 0.05f;

        private static CharacterStats Sheet(int power, int agility, int specialty, int constitution, EquipmentClass equipment)
        {
            CharacterStats stats = new CharacterStats
            {
                BasePower = power,
                BaseAgility = agility,
                BaseSpecialty = specialty,
                BaseConstitution = constitution,
                Equipment = equipment
            };

            // Level 1 with no growth, so the totals are exactly the sheet's base values.
            stats.ApplyInstance(1, new AttributeGrowth(), 1f);
            return stats;
        }

        // --- Maximum health: 5 per POW and 10 per CON ---

        [TestCase(0, 0, 0)]
        [TestCase(1, 0, 5)]
        [TestCase(0, 1, 10)]
        [TestCase(20, 30, 400)]
        public void MaxHealth_IsFivePerPowerAndTenPerConstitution(int power, int constitution, int expected)
        {
            Assert.AreEqual(expected, Sheet(power, 0, 0, constitution, EquipmentClass.Light).MaxHealth);
        }

        // --- Damage: 1 physical per POW, 1 elemental per SPE ---

        [Test]
        public void PhysicalDamage_IsOnePerPower()
        {
            Assert.AreEqual(17, Sheet(17, 0, 0, 0, EquipmentClass.Light).PhysicalDamage);
        }

        [Test]
        public void ElementalDamage_IsOnePerSpecialty()
        {
            Assert.AreEqual(23, Sheet(0, 0, 23, 0, EquipmentClass.Light).ElementalDamage);
        }

        // --- Attack speed: base 1, plus 1% / 0.5% / 0.2% per AGI by equipment class ---

        [TestCase(EquipmentClass.Light, 100, 2.00f)]
        [TestCase(EquipmentClass.Magic, 100, 1.50f)]
        [TestCase(EquipmentClass.Heavy, 100, 1.20f)]
        public void AttacksPerSecond_FollowsTheEquipmentTable(EquipmentClass equipment, int agility, float expected)
        {
            Assert.AreEqual(expected, Sheet(0, agility, 0, 0, equipment).AttacksPerSecond, Tolerance);
        }

        [Test]
        public void AttacksPerSecond_IsOneWithoutAgility()
        {
            Assert.AreEqual(1f, Sheet(0, 0, 0, 0, EquipmentClass.Light).AttacksPerSecond, Tolerance);
        }

        // --- Movement speed: base 2 cells per second, plus 1% / 0.75% / 0.5% per AGI ---

        [TestCase(EquipmentClass.Light, 100, 4.00f)]
        [TestCase(EquipmentClass.Magic, 100, 3.50f)]
        [TestCase(EquipmentClass.Heavy, 100, 3.00f)]
        public void CellsPerSecond_FollowsTheEquipmentTable(EquipmentClass equipment, int agility, float expected)
        {
            Assert.AreEqual(expected, Sheet(0, agility, 0, 0, equipment).CellsPerSecond, Tolerance);
        }

        [Test]
        public void CellsPerSecond_IsTwoWithoutAgility()
        {
            Assert.AreEqual(2f, Sheet(0, 0, 0, 0, EquipmentClass.Light).CellsPerSecond, Tolerance);
        }

        /// <summary>
        /// attributes.md states that repositioning one cell takes half a second at base speed,
        /// and design-decisions.md states that crossing the board takes 3.5 seconds. The board
        /// is 8 rows, so the longest crossing is 7 cells.
        /// </summary>
        [Test]
        public void BaseMovementSpeed_MatchesTheTimingsQuotedInTheSpecs()
        {
            float cellsPerSecond = Sheet(0, 0, 0, 0, EquipmentClass.Light).CellsPerSecond;

            Assert.AreEqual(0.5f, 1f / cellsPerSecond, 0.01f, "Repositioning one cell should take half a second.");
            Assert.AreEqual(3.5f, 7f / cellsPerSecond, 0.01f, "Crossing the 8 row board should take 3.5 seconds.");
        }

        // --- Evasion: 100 x AGI / (AGI + constant), constant 100 / 200 / 500 ---

        [TestCase(EquipmentClass.Light, 25, 20.0f)]
        [TestCase(EquipmentClass.Light, 100, 50.0f)]
        [TestCase(EquipmentClass.Light, 300, 75.0f)]
        [TestCase(EquipmentClass.Light, 900, 90.0f)]
        [TestCase(EquipmentClass.Magic, 100, 33.3f)]
        [TestCase(EquipmentClass.Magic, 200, 50.0f)]
        [TestCase(EquipmentClass.Magic, 600, 75.0f)]
        [TestCase(EquipmentClass.Heavy, 100, 16.7f)]
        [TestCase(EquipmentClass.Heavy, 500, 50.0f)]
        [TestCase(EquipmentClass.Heavy, 1500, 75.0f)]
        public void EvasionChance_MatchesTheExamplesInTheSpec(EquipmentClass equipment, int agility, float expected)
        {
            Assert.AreEqual(expected, Sheet(0, agility, 0, 0, equipment).EvasionChance, Tolerance);
        }

        [Test]
        public void EvasionChance_IsZeroWithoutAgility()
        {
            Assert.AreEqual(0f, Sheet(0, 0, 0, 0, EquipmentClass.Light).EvasionChance, Tolerance);
        }

        /// <summary>
        /// The curve only approaches its cap, which is why no manual clamp exists anywhere.
        /// If one is ever added, this is the test that should fail.
        /// </summary>
        [Test]
        public void EvasionChance_NeverReachesOneHundred()
        {
            Assert.Less(Sheet(0, 1000000, 0, 0, EquipmentClass.Light).EvasionChance, 100f);
        }

        // --- Cooldown reduction: 60 x SPE / (SPE + constant), constant 60 / 40 / 300 ---

        [TestCase(EquipmentClass.Magic, 20, 20.0f)]
        [TestCase(EquipmentClass.Magic, 40, 30.0f)]
        [TestCase(EquipmentClass.Magic, 120, 45.0f)]
        [TestCase(EquipmentClass.Magic, 360, 54.0f)]
        [TestCase(EquipmentClass.Light, 20, 15.0f)]
        [TestCase(EquipmentClass.Light, 60, 30.0f)]
        [TestCase(EquipmentClass.Light, 180, 45.0f)]
        [TestCase(EquipmentClass.Light, 540, 54.0f)]
        [TestCase(EquipmentClass.Heavy, 100, 15.0f)]
        [TestCase(EquipmentClass.Heavy, 300, 30.0f)]
        [TestCase(EquipmentClass.Heavy, 900, 45.0f)]
        public void CooldownReduction_MatchesTheExamplesInTheSpec(EquipmentClass equipment, int specialty, float expected)
        {
            Assert.AreEqual(expected, Sheet(0, 0, specialty, 0, equipment).CooldownReduction, Tolerance);
        }

        [Test]
        public void CooldownReduction_NeverReachesSixty()
        {
            Assert.Less(Sheet(0, 0, 1000000, 0, EquipmentClass.Magic).CooldownReduction, 60f);
        }

        // --- Physical mitigation: 75 x armour / (armour + 50 x attacker level) ---

        [TestCase(500, 10, 37.5f)]
        [TestCase(1500, 10, 56.2f)]
        [TestCase(500, 50, 12.5f)]
        [TestCase(2500, 50, 37.5f)]
        public void PhysicalMitigation_MatchesTheExamplesInTheSpec(int armor, int attackerLevel, float expected)
        {
            CharacterStats stats = Sheet(0, 0, 0, 0, EquipmentClass.Heavy);
            stats.PhysicalArmor = armor;

            Assert.AreEqual(expected, stats.PhysicalMitigationAgainst(attackerLevel), Tolerance);
        }

        [Test]
        public void PhysicalMitigation_NeverReachesSeventyFive()
        {
            CharacterStats stats = Sheet(0, 0, 0, 0, EquipmentClass.Heavy);
            stats.PhysicalArmor = 100000000;

            Assert.Less(stats.PhysicalMitigationAgainst(1), 75f);
        }

        // --- The curve itself ---

        [Test]
        public void DiminishingReturns_IsZeroWithoutPoints()
        {
            Assert.AreEqual(0f, CharacterStats.DiminishingReturns(100f, 0f, 100f), Tolerance);
        }

        [Test]
        public void DiminishingReturns_ReachesHalfTheCapWhenPointsEqualTheConstant()
        {
            Assert.AreEqual(50f, CharacterStats.DiminishingReturns(100f, 250f, 250f), Tolerance);
        }
    }
}
