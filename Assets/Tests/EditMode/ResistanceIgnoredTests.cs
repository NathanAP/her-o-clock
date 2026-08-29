using HerOClock.Characters;
using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Ignored resistance: the attacker cutting the target's defence points **before** the curve.
    ///
    /// Every number here is copied from "### Resistência ignorada" in attributes.md, which carries
    /// the formula and the worked example. Reading them out of the code instead would only prove
    /// that the code does what the code does.
    /// </summary>
    public class ResistanceIgnoredTests
    {
        /// <summary>
        /// The random source is left out so no evasion is rolled. Every assertion here is about
        /// mitigation, and an evasion landing in the middle of one would make it about luck.
        /// </summary>
        private static DamageResult Hit(
            int baseDamage, DamageType type, int level, int mitigationPoints, float ignored)
        {
            DamageInput input = new DamageInput
            {
                BaseDamage = baseDamage,
                Type = type,
                AttackerLevel = level,
                TargetMitigationPoints = mitigationPoints,
                AttackerResistanceIgnoredPercent = ignored
            };

            return DamageCalculator.Resolve(input, null);
        }

        /// <summary>
        /// The example written in attributes.md: an attacker of level 10 (constant 500) with 20%
        /// ignored, against a target holding 1000 points.
        ///
        /// The considered points become 800, so the mitigation is `75 x 800 / 1300` = 46.2%,
        /// against the 50% the target would have had. On 1000 of base damage that is 538 through
        /// instead of 500.
        /// </summary>
        [Test]
        public void TheWorkedExampleOfTheSpec()
        {
            Assert.AreEqual(500, Hit(1000, DamageType.Physical, 10, 1000, 0f).Damage,
                "Without any cut the target sits on exactly half.");

            Assert.AreEqual(538, Hit(1000, DamageType.Physical, 10, 1000, 20f).Damage,
                "1000 points cut to 800 gives 46.2% mitigation, so 538 of 1000 lands.");
        }

        /// <summary>
        /// It cuts the points and not the mitigation, and the difference is the whole reason the
        /// rule is written this way.
        ///
        /// Cutting the 50% mitigation by 20% would leave 40%, and 600 through. Cutting the points
        /// leaves 46.2% and 538 — a far smaller gain, which is what stops the modifier from
        /// becoming mandatory against every resistant enemy.
        /// </summary>
        [Test]
        public void CuttingThePointsIsWorthLessThanCuttingTheMitigation()
        {
            int cutPoints = Hit(1000, DamageType.Physical, 10, 1000, 20f).Damage;

            Assert.AreEqual(538, cutPoints);
            Assert.Less(cutPoints, 600, "Cutting after the curve would have given 600.");
        }

        /// <summary>
        /// attributes.md: it works on physical armour and on the three elemental resistances,
        /// because all four go through the same curve.
        /// </summary>
        [TestCase(DamageType.Physical)]
        [TestCase(DamageType.Fire)]
        [TestCase(DamageType.Water)]
        [TestCase(DamageType.Electric)]
        public void ItWorksOnAllFourDefences(DamageType type)
        {
            Assert.AreEqual(538, Hit(1000, type, 10, 1000, 20f).Damage);
        }

        /// <summary>
        /// With everything ignored the points reach zero, the curve answers nothing, and the whole
        /// blow lands. Nothing is clamped: past 100% the points would go negative and the curve
        /// already answers zero for that.
        /// </summary>
        [TestCase(100f)]
        [TestCase(150f)]
        public void IgnoringEverythingLeavesTheTargetBare(float ignored)
        {
            Assert.AreEqual(1000, Hit(1000, DamageType.Physical, 10, 1000, ignored).Damage);
        }

        /// <summary>
        /// It does **not** touch evasion. Evasion is a chance to avoid the attack rather than a
        /// defence against it, and attributes.md lists armour and the three resistances only.
        ///
        /// Asserted against the same seed on both sides, so an identical sequence of rolls proves
        /// the evasion chance itself never moved.
        /// </summary>
        [Test]
        public void ItDoesNotTouchEvasion()
        {
            for (int seed = 1; seed <= 20; seed++)
            {
                DamageInput bare = new DamageInput
                {
                    BaseDamage = 1000,
                    Type = DamageType.Physical,
                    AttackerLevel = 10,
                    TargetEvasionPoints = 1000,
                    TargetMitigationPoints = 0
                };

                DamageInput piercing = bare;
                piercing.AttackerResistanceIgnoredPercent = 80f;

                DamageResult without = DamageCalculator.Resolve(bare, new BattleRandom(seed));
                DamageResult with = DamageCalculator.Resolve(piercing, new BattleRandom(seed));

                Assert.AreEqual(without.Evaded, with.Evaded, "Seed " + seed + " evaded differently.");
                Assert.AreEqual(without.Damage, with.Damage, "Seed " + seed + " dealt differently.");
            }
        }

        /// <summary>
        /// A character with nothing equipped ignores nothing, which is every minion and villain in
        /// the game. It comes from items only, and that is deliberate rather than an oversight.
        /// </summary>
        [Test]
        public void ACharacterWithNoItemsIgnoresNothing()
        {
            using (TestBattle battle = new TestBattle())
            {
                CharacterDefinition sheet = battle.Sheet("minion", CharacterKind.Minion);
                Character minion = battle.Spawn(sheet, Team.Enemies, 0, 4);

                Assert.AreEqual(0f, minion.Stats.ResistanceIgnoredPercent, 0.0001f);
            }
        }
    }
}
