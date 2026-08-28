using System.Collections.Generic;
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

        /// <summary>A sheet carrying only physical armour, for the mitigation curve.</summary>
        private static CharacterStats Armoured(int armor)
        {
            CharacterStats stats = new CharacterStats
            {
                Equipment = EquipmentClass.Light,
                BasePhysicalArmor = armor
            };

            stats.ApplyInstance(1, new AttributeGrowth(), 1f);
            return stats;
        }

        /// <summary>
        /// attributes.md: ten points of POW raise the damage base by one percent, and the attribute
        /// never adds to it. With a base of 100 and 200 points, the swing is 120.
        ///
        /// The multiplication is the whole point. A point that added flat damage would hand a
        /// character at the 500 cap five hundred damage for free, and no weapon is worth that, so
        /// the item would stop mattering.
        /// </summary>
        [Test]
        public void PowerMultipliesTheDamageBaseAndNeverAddsToIt()
        {
            CharacterStats stats = Sheet(200, 0, 0, 0, EquipmentClass.Light);
            stats.BaseDamage = 100;

            Assert.AreEqual(120, stats.PhysicalDamage);
        }

        /// <summary>A character with no damage base does no damage, however much POW it carries.</summary>
        [Test]
        public void PowerAloneIsWorthNoDamage()
        {
            CharacterStats stats = Sheet(500, 0, 0, 0, EquipmentClass.Light);
            stats.BaseDamage = 0;

            Assert.AreEqual(0, stats.PhysicalDamage);
        }

        // --- Attack speed: base 1, plus 1% / 0.5% / 0.2% per AGI by equipment class ---

        [TestCase(EquipmentClass.Light, 100, 2.00f)]
        [TestCase(EquipmentClass.Special, 100, 1.50f)]
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
        [TestCase(EquipmentClass.Special, 100, 3.50f)]
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

        // --- Evasion: points through the same curve as armour, against 50 x attacker level ---

        /// <summary>
        /// attributes.md: AGI turns into evasion points at 1, 0.5 or 0.2 per point depending on
        /// the class. A light character with 100 AGI has 100 points, special has 50, heavy has 20.
        /// </summary>
        [TestCase(EquipmentClass.Light, 100, 100)]
        [TestCase(EquipmentClass.Special, 100, 50)]
        [TestCase(EquipmentClass.Heavy, 100, 20)]
        public void EvasionPoints_ConvertAgilityAtTheClassRate(EquipmentClass equipment, int agility, int expected)
        {
            Assert.AreEqual(expected, Sheet(0, agility, 0, 0, equipment).EvasionPoints);
        }

        /// <summary>
        /// attributes.md: the sheet's own evasion grows per level exactly like armour, and adds to
        /// whatever AGI contributes. 25 with a growth of 25 is 25 points at level 1 and 2500 at
        /// level 100.
        /// </summary>
        [TestCase(1, 25)]
        [TestCase(100, 2500)]
        public void EvasionPoints_AddTheSheetValueGrownByLevel(int level, int expected)
        {
            CharacterStats stats = new CharacterStats
            {
                Equipment = EquipmentClass.Light,
                BaseEvasion = 25,
                EvasionPerLevel = 25
            };

            stats.ApplyInstance(level, new AttributeGrowth(), 1f);

            Assert.AreEqual(expected, stats.EvasionPoints);
        }

        /// <summary>
        /// The two worked examples of attributes.md: 150 points are 75% against a level 1 attacker
        /// and 23.1% against a level 10 one.
        ///
        /// The same defender, two different numbers. That is the whole change: evasion is not a
        /// property of who is being hit, it is a comparison between two characters.
        /// </summary>
        [TestCase(1, 75.0f)]
        [TestCase(10, 23.1f)]
        public void EvasionChance_DependsOnWhoIsAttacking(int attackerLevel, float expected)
        {
            CharacterStats stats = Evading(150);

            Assert.AreEqual(expected, stats.EvasionChanceAgainst(attackerLevel), Tolerance);
        }

        /// <summary>
        /// attributes.md: whenever the points equal 50 x the attacker's level, the chance is
        /// exactly 50%. It is the same identity the mitigation curve has, and it is what makes the
        /// constant readable at a glance.
        /// </summary>
        [TestCase(1)]
        [TestCase(12)]
        [TestCase(100)]
        public void EvasionChance_IsHalfWhenThePointsMatchTheConstant(int attackerLevel)
        {
            Assert.AreEqual(50f, Evading(50 * attackerLevel).EvasionChanceAgainst(attackerLevel), Tolerance);
        }

        /// <summary>
        /// attributes.md: a growth equal to the base holds the chance still for the whole game,
        /// exactly as it does for armour. A sheet with 25 and 25 sits at 33.3% at every level.
        ///
        /// This is the reason the whole version exists. Before it, evasion was the one defence
        /// whose constant did not grow, so it climbed on its own from level 1 to 100 while every
        /// other defence rotted.
        /// </summary>
        [TestCase(1)]
        [TestCase(12)]
        [TestCase(100)]
        public void Evasion_HoldsStillWhenTheGrowthMatchesTheBase(int level)
        {
            CharacterStats stats = new CharacterStats
            {
                Equipment = EquipmentClass.Light,
                BaseEvasion = 25,
                EvasionPerLevel = 25
            };

            stats.ApplyInstance(level, new AttributeGrowth(), 1f);

            Assert.AreEqual(33.3f, stats.EvasionChanceAgainst(level), Tolerance);
        }

        [Test]
        public void EvasionPoints_AreZeroWithoutAgilityOrSheetValue()
        {
            Assert.AreEqual(0, Sheet(0, 0, 0, 0, EquipmentClass.Light).EvasionPoints);
            Assert.AreEqual(0f, Sheet(0, 0, 0, 0, EquipmentClass.Light).EvasionChanceAgainst(1), Tolerance);
        }

        /// <summary>
        /// The curve only approaches its cap, which is why no manual clamp exists anywhere.
        /// If one is ever added, this is the test that should fail.
        /// </summary>
        [Test]
        public void EvasionChance_NeverReachesOneHundred()
        {
            Assert.Less(Evading(int.MaxValue / 2).EvasionChanceAgainst(1), 100f);
        }

        /// <summary>A sheet carrying only evasion points, for the curve.</summary>
        private static CharacterStats Evading(int points)
        {
            CharacterStats stats = new CharacterStats
            {
                Equipment = EquipmentClass.Light,
                BaseEvasion = points,
                EvasionPerLevel = 0
            };

            stats.ApplyInstance(1, new AttributeGrowth(), 1f);
            return stats;
        }

        /// <summary>
        /// attributes.md: with equipment worn, AGI converts at the mixed rate rather than the
        /// sheet's own class. The example there — two heavy casings, two light, one medium weapon
        /// and one special controller — converts at 0.5833, so 100 AGI is 58 points.
        ///
        /// The sheet says Heavy here on purpose. Wearing something has to beat what the sheet
        /// declares, otherwise equipment would never change a hero at all.
        /// </summary>
        [Test]
        public void EvasionPoints_FollowTheEquipmentWornRatherThanTheSheet()
        {
            CharacterStats stats = Sheet(0, 100, 0, 0, EquipmentClass.Heavy);

            stats.UseEquipment(EquipmentComposition.Of(
                new List<ItemClass>
                {
                    ItemClass.Heavy,
                    ItemClass.Heavy,
                    ItemClass.Light,
                    ItemClass.Light,
                    ItemClass.Medium,
                    ItemClass.Special
                },
                EquipmentClass.Heavy));

            Assert.AreEqual(58, stats.EvasionPoints);
        }

        /// <summary>
        /// A clone belongs to another character, so it must not arrive already wearing somebody
        /// else's equipment. It starts from its own sheet class, exactly like the buff list does.
        /// </summary>
        [Test]
        public void AClone_DoesNotInheritTheEquipment()
        {
            CharacterStats stats = Sheet(0, 100, 0, 0, EquipmentClass.Heavy);
            stats.UseEquipment(EquipmentComposition.Of(
                new List<ItemClass> { ItemClass.Light }, EquipmentClass.Heavy));

            CharacterStats copy = stats.Clone();
            copy.ApplyInstance(1, new AttributeGrowth(), 1f);

            Assert.AreEqual(100, stats.EvasionPoints, "The original is wearing light, so 1 point per AGI.");
            Assert.AreEqual(20, copy.EvasionPoints, "The copy falls back to its heavy sheet, so 0.2 per AGI.");
        }

        // --- Cooldown reduction: 60 x SPE / (SPE + constant), constant 60 / 40 / 300 ---

        [TestCase(EquipmentClass.Special, 20, 20.0f)]
        [TestCase(EquipmentClass.Special, 40, 30.0f)]
        [TestCase(EquipmentClass.Special, 120, 45.0f)]
        [TestCase(EquipmentClass.Special, 360, 54.0f)]
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
            Assert.Less(Sheet(0, 0, 1000000, 0, EquipmentClass.Special).CooldownReduction, 60f);
        }

        // --- Physical mitigation: 75 x armour / (armour + 50 x attacker level) ---

        [TestCase(500, 10, 37.5f)]
        [TestCase(1500, 10, 56.2f)]
        [TestCase(500, 50, 12.5f)]
        [TestCase(2500, 50, 37.5f)]
        public void PhysicalMitigation_MatchesTheExamplesInTheSpec(int armor, int attackerLevel, float expected)
        {
            CharacterStats stats = Armoured(armor);

            Assert.AreEqual(expected, stats.PhysicalMitigationAgainst(attackerLevel), Tolerance);
        }

        [Test]
        public void PhysicalMitigation_NeverReachesSeventyFive()
        {
            Assert.Less(Armoured(100000000).PhysicalMitigationAgainst(1), 75f);
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
