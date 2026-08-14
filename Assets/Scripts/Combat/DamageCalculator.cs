using System;
using HerOClock.Characters;

namespace HerOClock.Combat
{
    /// <summary>
    /// Resolves one attack following the "Ordem do cálculo de dano" section of attributes.md.
    ///
    /// It is a pure function: numbers in, numbers out, and the only state it touches is the
    /// advance of the random sequence. That allows the whole thing to be tested outside Unity
    /// against the examples written in the spec.
    ///
    /// **Every step is computed in double, deliberately.** The inputs arrive as floats because
    /// that is what the sheets hold, but the arithmetic must not stay there. C# allows float
    /// operations to be carried out at a higher precision than float, and whether an intermediate
    /// gets rounded back is up to the runtime. That is enough to change a result: 40/100 lands
    /// either side of 0.4, which turned one attack of exactly 262.5 into 262 outside Unity and
    /// 263 inside it. Widening to double first removes the ambiguity, which matters because a
    /// battle is supposed to replay identically anywhere.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>Mitigation cap, both physical and elemental.</summary>
        public const float MitigationCap = 75f;

        /// <summary>A normal evasion removes this percentage of the damage.</summary>
        public const float NormalEvasionReduction = 40f;

        /// <summary>A perfect evasion removes this percentage of the damage.</summary>
        public const float PerfectEvasionReduction = 75f;

        /// <summary>Share of the evasion that becomes a chance of perfect evasion.</summary>
        public const float PerfectEvasionShare = 30f;

        public static DamageResult Resolve(DamageInput input, BattleRandom random)
        {
            DamageResult result = new DamageResult();

            if (input.BaseDamage <= 0)
            {
                return result;
            }

            // Step 2: thorns, computed on the base damage and before any mitigation.
            if (input.CanTriggerThorns && input.Type == DamageType.Physical && input.TargetThornsPercent > 0f)
            {
                result.Thorns = Round(input.BaseDamage * input.TargetThornsPercent / 100.0);
            }

            // Step 3: mitigation.
            double mitigation = Mitigation(input);

            // Once resistance goes past 100%, the attack turns into healing. In that case there
            // is no damage, so no evasion is rolled, no thorns trigger and no life is stolen.
            if (mitigation > 100.0)
            {
                result.Healing = Round(input.BaseDamage * (mitigation - 100.0) / 100.0);
                result.Thorns = 0;
                return result;
            }

            // Step 4: evasion, rolled exactly once per attack.
            double evasionFactor = 1.0;

            if (random != null && random.Roll(input.TargetEvasionChance))
            {
                result.Evaded = true;
                result.PerfectEvasion = random.Roll(PerfectEvasionShare);

                double reduction = result.PerfectEvasion ? PerfectEvasionReduction : NormalEvasionReduction;
                evasionFactor = 1.0 - reduction / 100.0;
            }

            // Step 5: final damage. Reductions always multiply together, they never add up.
            double damage = input.BaseDamage * (1.0 - mitigation / 100.0) * evasionFactor;
            result.Damage = Math.Max(0, Round(damage));

            // Step 6: life steal, on the final damage and only for physical attacks.
            if (input.Type == DamageType.Physical && input.AttackerLifeStealPercent > 0f)
            {
                result.LifeStolen = Math.Max(0, Round(result.Damage * input.AttackerLifeStealPercent / 100.0));
            }

            return result;
        }

        /// <summary>
        /// The target's mitigation percentage against this attack.
        /// Physical stays between 0 and the cap. Elemental can go past 100, turning the attack
        /// into healing, or down to -100 through stacked debuffs.
        /// </summary>
        private static double Mitigation(DamageInput input)
        {
            double constant = 50.0 * Math.Max(1, input.AttackerLevel);
            double fromCurve = CharacterStats.DiminishingReturns(MitigationCap, input.TargetMitigationPoints, constant);

            if (input.Type == DamageType.Physical)
            {
                return fromCurve;
            }

            double total = fromCurve + input.TargetResistanceBonus;
            return total < -100.0 ? -100.0 : total;
        }

        /// <summary>
        /// Rounds to the nearest integer using the language's default rounding.
        /// When a value falls exactly halfway it goes to the nearest even integer, as described
        /// in the "Arredondamento" section of attributes.md.
        /// </summary>
        public static int Round(double value)
        {
            return (int)Math.Round(value);
        }
    }
}
