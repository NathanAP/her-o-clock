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

        /// <summary>Target's evasion chance, from 0 to 100.</summary>
        public float TargetEvasionChance;

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

        /// <summary>Target's thorns, from 0 to 100.</summary>
        public float TargetThornsPercent;

        /// <summary>
        /// False when this attack is already a thorns reflection, to stop two characters from
        /// reflecting damage back and forth forever.
        /// </summary>
        public bool CanTriggerThorns;
    }
}
