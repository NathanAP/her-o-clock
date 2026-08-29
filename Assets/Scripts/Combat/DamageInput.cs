namespace HerOClock.Combat
{
    /// <summary>
    /// Everything the damage calculation needs to know about one attack.
    ///
    /// It is made of plain numbers on purpose, with no reference to characters or to Unity.
    /// That is what allows the whole calculation to be tested outside the editor, which is
    /// where we check that the formulas match the examples written in attributes.md.
    /// </summary>
    public struct DamageInput
    {
        /// <summary>Damage before any mitigation.</summary>
        public int BaseDamage;

        public DamageType Type;

        /// <summary>Attacker's level. Feeds the constant of the mitigation curve.</summary>
        public int AttackerLevel;

        /// <summary>Attacker's life steal, from 0 to 100.</summary>
        public float AttackerLifeStealPercent;

        /// <summary>
        /// The target's evasion points, which the calculator turns into a chance using the
        /// attacker's level.
        ///
        /// Points and not a chance, because the constant of the evasion curve is 50 x the
        /// attacker's level, exactly like the mitigation one below. The same defender dodges a
        /// weak enemy more often than a strong one, so the chance simply does not exist until
        /// both sides are known.
        /// </summary>
        public int TargetEvasionPoints;

        /// <summary>
        /// The target's physical armour for physical damage, or its resistance points against
        /// the attack's element for elemental damage. Goes through the diminishing returns curve.
        /// </summary>
        public int TargetMitigationPoints;

        /// <summary>
        /// Resistance from special sources, added after the curve and only for elemental
        /// damage. It is the only way to go past 75% and reach above 100%, where the attack
        /// turns into healing. Nothing in the game grants this yet.
        /// </summary>
        public float TargetResistanceBonus;

        /// <summary>
        /// Share of the target's defence the **attacker** ignores, from 0 to 100.
        ///
        /// It is the attacker's and not the target's, which is the whole difference between it and
        /// <see cref="TargetResistanceBonus"/> right above. And it cuts the target's **points**
        /// before the curve rather than the mitigation after it: cutting afterwards would make the
        /// gain grow the more defended the target is, turning the modifier into a requirement
        /// against any resistant enemy. Cutting before, diminishing returns keeps applying and the
        /// gain against a saturated target stays small.
        ///
        /// It applies to physical armour and to the three elemental resistances, because all four
        /// go through the same curve. It does **not** apply to evasion, which is a chance to avoid
        /// the attack rather than a defence against it.
        /// </summary>
        public float AttackerResistanceIgnoredPercent;

        /// <summary>Target's thorns, from 0 to 100.</summary>
        public float TargetThornsPercent;

        /// <summary>
        /// False when this attack is already a thorns reflection, to stop two characters from
        /// reflecting damage back and forth forever.
        /// </summary>
        public bool CanTriggerThorns;
    }
}
