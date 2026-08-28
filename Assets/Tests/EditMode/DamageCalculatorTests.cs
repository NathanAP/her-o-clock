using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks one attack being resolved against the "Ordem do cálculo de dano" section of
    /// attributes.md, example by example.
    ///
    /// A null random source means no evasion is ever rolled, which is how the deterministic
    /// parts of the calculation are isolated from the one part that is a dice throw.
    /// </summary>
    public class DamageCalculatorTests
    {
        private static DamageInput Attack(int baseDamage, DamageType type)
        {
            return new DamageInput
            {
                BaseDamage = baseDamage,
                Type = type,
                AttackerLevel = 1,
                CanTriggerThorns = true
            };
        }

        // --- Rounding, as described in "Arredondamento" ---

        [TestCase(0.4, 0)]
        [TestCase(0.6, 1)]
        [TestCase(12.3, 12)]
        [TestCase(12.7, 13)]
        public void Round_GoesToTheNearestInteger(double value, int expected)
        {
            Assert.AreEqual(expected, DamageCalculator.Round(value));
        }

        /// <summary>
        /// An exact half goes to the nearest even integer. It is the language's default, and the
        /// spec was written to describe it rather than fight it, because always rounding up would
        /// build an upward bias over thousands of blows.
        /// </summary>
        [TestCase(0.5, 0)]
        [TestCase(1.5, 2)]
        [TestCase(2.5, 2)]
        [TestCase(3.5, 4)]
        public void Round_BreaksAnExactHalfTowardsTheEvenInteger(double value, int expected)
        {
            Assert.AreEqual(expected, DamageCalculator.Round(value));
        }

        // --- Mitigation ---

        [Test]
        public void WithoutArmour_TheFullDamageLands()
        {
            DamageResult result = DamageCalculator.Resolve(Attack(100, DamageType.Physical), null);

            Assert.AreEqual(100, result.Damage);
        }

        /// <summary>
        /// 500 armour against a level 10 attacker is 37.5% mitigation, so 100 damage becomes 62.5,
        /// which rounds to 62 by the even rule.
        /// </summary>
        [Test]
        public void PhysicalArmour_MitigatesByTheCurve()
        {
            DamageInput input = Attack(100, DamageType.Physical);
            input.AttackerLevel = 10;
            input.TargetMitigationPoints = 500;

            Assert.AreEqual(62, DamageCalculator.Resolve(input, null).Damage);
        }

        [Test]
        public void ZeroBaseDamage_ResolvesToNothingAtAll()
        {
            DamageResult result = DamageCalculator.Resolve(Attack(0, DamageType.Physical), null);

            Assert.AreEqual(0, result.Damage);
            Assert.AreEqual(0, result.Thorns);
            Assert.AreEqual(0, result.Healing);
        }

        // --- Elemental resistance above 100% turns the attack into healing ---

        [TestCase(200f, 1000)]
        [TestCase(150f, 500)]
        public void ResistanceAboveOneHundred_HealsInsteadOfHurting(float totalResistance, int expectedHealing)
        {
            DamageInput input = Attack(1000, DamageType.Fire);
            // Nothing grants points here, so the bonus is the whole resistance.
            input.TargetResistanceBonus = totalResistance;

            DamageResult result = DamageCalculator.Resolve(input, null);

            Assert.AreEqual(expectedHealing, result.Healing);
            Assert.AreEqual(0, result.Damage);
        }

        /// <summary>
        /// When the attack turns into healing there is no damage, so nothing is evaded, nothing
        /// is reflected and nothing is stolen.
        /// </summary>
        [Test]
        public void WhenTheAttackHeals_ThornsAndLifeStealDoNotHappen()
        {
            DamageInput input = Attack(1000, DamageType.Fire);
            input.TargetResistanceBonus = 200f;
            input.TargetThornsPercent = 50f;
            input.AttackerLifeStealPercent = 50f;

            DamageResult result = DamageCalculator.Resolve(input, null);

            Assert.AreEqual(0, result.Thorns);
            Assert.AreEqual(0, result.LifeStolen);
            Assert.IsFalse(result.Evaded);
        }

        // --- Negative resistance increases the damage taken ---

        [TestCase(-50f, 1500)]
        [TestCase(-100f, 2000)]
        public void NegativeResistance_IncreasesTheDamageTaken(float resistance, int expected)
        {
            DamageInput input = Attack(1000, DamageType.Fire);
            input.TargetResistanceBonus = resistance;

            Assert.AreEqual(expected, DamageCalculator.Resolve(input, null).Damage);
        }

        /// <summary>
        /// Resistance never goes below -100%, so stacked debuffs cannot produce infinite damage.
        /// </summary>
        [Test]
        public void ResistanceIsFlooredAtMinusOneHundred()
        {
            DamageInput input = Attack(1000, DamageType.Fire);
            input.TargetResistanceBonus = -500f;

            Assert.AreEqual(2000, DamageCalculator.Resolve(input, null).Damage);
        }

        // --- Life steal ---

        [Test]
        public void LifeSteal_IsTakenFromTheFinalPhysicalDamage()
        {
            DamageInput input = Attack(100, DamageType.Physical);
            input.AttackerLifeStealPercent = 3f;

            Assert.AreEqual(3, DamageCalculator.Resolve(input, null).LifeStolen);
        }

        /// <summary>
        /// The spec's own example: an attack mitigated down to 10 steals 0.3, which rounds to
        /// nothing. Life steal is computed on what actually landed, not on what was attempted.
        /// </summary>
        [Test]
        public void LifeSteal_RoundsDownToNothingOnASmallHit()
        {
            DamageInput input = Attack(10, DamageType.Physical);
            input.AttackerLifeStealPercent = 3f;

            Assert.AreEqual(0, DamageCalculator.Resolve(input, null).LifeStolen);
        }

        [Test]
        public void LifeSteal_DoesNotHappenOnElementalDamage()
        {
            DamageInput input = Attack(100, DamageType.Fire);
            input.AttackerLifeStealPercent = 3f;

            Assert.AreEqual(0, DamageCalculator.Resolve(input, null).LifeStolen);
        }

        // --- Thorns ---

        /// <summary>
        /// Thorns is computed on the base damage, before any mitigation, so that a very defensive
        /// character still reflects a meaningful amount.
        /// </summary>
        [Test]
        public void Thorns_IsComputedBeforeMitigation()
        {
            DamageInput input = Attack(100, DamageType.Physical);
            input.AttackerLevel = 10;
            input.TargetMitigationPoints = 500;
            input.TargetThornsPercent = 10f;

            DamageResult result = DamageCalculator.Resolve(input, null);

            Assert.AreEqual(10, result.Thorns, "Thorns should come from the 100 base, not the 62 that landed.");
            Assert.AreEqual(62, result.Damage);
        }

        [Test]
        public void Thorns_DoesNotReactToElementalDamage()
        {
            DamageInput input = Attack(100, DamageType.Fire);
            input.TargetThornsPercent = 10f;

            Assert.AreEqual(0, DamageCalculator.Resolve(input, null).Thorns);
        }

        /// <summary>
        /// A reflection never reflects back, which is what stops two thorny characters from
        /// bouncing damage between them forever.
        /// </summary>
        [Test]
        public void Thorns_DoesNotReactToAnotherThorns()
        {
            DamageInput input = Attack(100, DamageType.Physical);
            input.TargetThornsPercent = 10f;
            input.CanTriggerThorns = false;

            Assert.AreEqual(0, DamageCalculator.Resolve(input, null).Thorns);
        }

        // --- Evasion, and the rule that reductions multiply ---

        /// <summary>
        /// Enough evasion points that the chance is 99.995% against a level 1 attacker.
        ///
        /// It cannot be 100%: attributes.md says the curve only approaches its cap, so an evasion
        /// that always happens does not exist. The tests below need one anyway, because what they
        /// are checking is what an evasion does to the damage and not whether it happens.
        /// </summary>
        private const int CertainEvasion = 1000000;

        /// <summary>
        /// The evasion roll draws from the sequence, so the perfect evasion roll is the **second**
        /// number drawn. That is what the two seeds below are chosen against.
        /// </summary>
        [Test]
        public void AnAlmostCertainEvasion_IsRolled()
        {
            DamageInput input = Attack(100, DamageType.Physical);
            input.TargetEvasionPoints = CertainEvasion;

            DamageResult result = DamageCalculator.Resolve(input, new BattleRandom(SeedOfNormalEvasion));

            Assert.IsTrue(result.Evaded);
        }

        /// <summary>Seed whose second draw is above 30, so the perfect evasion roll fails.</summary>
        private const int SeedOfNormalEvasion = 2;

        /// <summary>Seed whose second draw is below 30, so the perfect evasion roll succeeds.</summary>
        private const int SeedOfPerfectEvasion = 1;

        /// <summary>
        /// The spec's worked example uses 75% mitigation, which the curve only approaches and
        /// never reaches, so the numbers here are the closest reachable ones. What is being
        /// checked is the rule, not the illustration: 150 armour against a level 1 attacker is
        /// 56.25% mitigation, and a normal evasion removes 40%, so 800 damage becomes
        /// 800 x 0.4375 x 0.6 = 210.
        ///
        /// Adding the reductions instead of multiplying them would leave 30, so the two
        /// possibilities are far apart and this cannot pass by accident. 800 is chosen over a
        /// round 1000 because 1000 lands on exactly 262.5, and a test of the multiplication rule
        /// should not also be a test of how a half is broken.
        /// </summary>
        [Test]
        public void ReductionsMultiplyRatherThanAddingUp()
        {
            DamageInput input = Attack(800, DamageType.Physical);
            input.AttackerLevel = 1;
            input.TargetMitigationPoints = 150;
            input.TargetEvasionPoints = CertainEvasion;

            DamageResult result = DamageCalculator.Resolve(input, new BattleRandom(SeedOfNormalEvasion));

            Assert.IsTrue(result.Evaded);
            Assert.IsFalse(result.PerfectEvasion, "This seed is chosen for a normal evasion.");
            Assert.AreEqual(210, result.Damage);
        }

        /// <summary>
        /// An attack whose exact value lands on a half must resolve to the same number everywhere.
        ///
        /// This one used to come out 262 outside Unity and 263 inside it. The arithmetic was done
        /// in float, and C# lets a runtime carry an intermediate at higher precision: whether
        /// 40/100 was rounded back to float before the subtraction decided which side of 262.5
        /// the result fell on. The calculation was moved to double so there is nothing left to
        /// decide, and a battle replayed from a seed cannot drift between editor and build.
        /// </summary>
        [Test]
        public void AnAttackThatLandsOnAHalfResolvesTheSameEverywhere()
        {
            DamageInput input = Attack(1000, DamageType.Physical);
            input.AttackerLevel = 1;
            input.TargetMitigationPoints = 150;
            input.TargetEvasionPoints = CertainEvasion;

            DamageResult result = DamageCalculator.Resolve(input, new BattleRandom(SeedOfNormalEvasion));

            Assert.IsFalse(result.PerfectEvasion);
            Assert.AreEqual(262, result.Damage, "1000 x 0.4375 x 0.6 is 262.5, and a half goes to the even integer.");
        }

        /// <summary>
        /// A perfect evasion removes 75% of the damage instead of 40%.
        /// </summary>
        [Test]
        public void APerfectEvasionRemovesThreeQuarters()
        {
            DamageInput input = Attack(1000, DamageType.Physical);
            input.TargetEvasionPoints = CertainEvasion;

            DamageResult result = DamageCalculator.Resolve(input, new BattleRandom(SeedOfPerfectEvasion));

            Assert.IsTrue(result.PerfectEvasion, "This seed is chosen for a perfect evasion.");
            Assert.AreEqual(250, result.Damage);
        }

        [Test]
        public void WithoutARandomSource_NothingIsEvaded()
        {
            DamageInput input = Attack(100, DamageType.Physical);
            input.TargetEvasionPoints = CertainEvasion;

            Assert.IsFalse(DamageCalculator.Resolve(input, null).Evaded);
        }
    }
}
