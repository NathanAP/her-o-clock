namespace HerOClock.Characters
{
    /// <summary>
    /// Everything a character's **active** equipment adds up to.
    ///
    /// It is a bag of numbers and nothing else. <see cref="CharacterStats"/> takes it and folds
    /// each line into the value it belongs to, which is what keeps items out of every formula in
    /// that class: one new term per stat, and no branch anywhere asking whether something is worn.
    ///
    /// **It lives here and not with the items on purpose.** `Items` already depends on this
    /// namespace for <see cref="Attribute"/> and <see cref="ItemClass"/>, so putting the bag on the
    /// other side would make the two depend on each other. What fills it is
    /// `HerOClock.Items.ItemContribution`, and the arrow only ever points one way.
    /// </summary>
    public class EquipmentTotals
    {
        private readonly int[] attributes = new int[4];

        public int Life { get; private set; }

        public int Armour { get; private set; }

        public int Evasion { get; private set; }

        public int FireResistance { get; private set; }

        public int WaterResistance { get; private set; }

        public int ElectricResistance { get; private set; }

        /// <summary>Share of physical damage sent back, added to whatever the sheet declares.</summary>
        public float ThornsPercent { get; private set; }

        /// <summary>
        /// Share of the target's defence this character ignores when attacking, from 0 to 100.
        ///
        /// It belongs to the **attacker** and cuts the target's points before the mitigation curve,
        /// which is not the same thing as lowering the mitigation afterwards. The reasoning and the
        /// worked example are in "Resistência ignorada" in attributes.md.
        /// </summary>
        public float ResistanceIgnoredPercent { get; private set; }

        /// <summary>
        /// Extra ability damage per element, from 0 upwards, as a percentage.
        ///
        /// Four properties rather than one indexed by damage type, because `DamageType` lives in
        /// `Combat` and this namespace must not depend on it — `Combat` already depends on this
        /// one. Whoever knows about damage types does the switch, exactly as
        /// `AbilityResolver.MitigationPointsOf` already does.
        /// </summary>
        public float FireAbilityDamagePercent { get; private set; }

        /// <inheritdoc cref="FireAbilityDamagePercent"/>
        public float WaterAbilityDamagePercent { get; private set; }

        /// <inheritdoc cref="FireAbilityDamagePercent"/>
        public float ElectricAbilityDamagePercent { get; private set; }

        /// <inheritdoc cref="FireAbilityDamagePercent"/>
        public float PhysicalAbilityDamagePercent { get; private set; }

        public int Of(Attribute attribute)
        {
            return attributes[(int)attribute];
        }

        public void AddAttribute(Attribute attribute, int amount)
        {
            attributes[(int)attribute] += amount;
        }

        public void AddLife(int amount)
        {
            Life += amount;
        }

        public void AddArmour(int amount)
        {
            Armour += amount;
        }

        public void AddEvasion(int amount)
        {
            Evasion += amount;
        }

        /// <summary>
        /// Adds to all three elements at once.
        ///
        /// It is its own method because the class defence of a special item covers the three
        /// together, and so does the `allResistance` modifier. Writing it three times at each call
        /// site is how one of them eventually gets forgotten.
        /// </summary>
        public void AddAllResistances(int amount)
        {
            FireResistance += amount;
            WaterResistance += amount;
            ElectricResistance += amount;
        }

        public void AddFireResistance(int amount)
        {
            FireResistance += amount;
        }

        public void AddWaterResistance(int amount)
        {
            WaterResistance += amount;
        }

        public void AddElectricResistance(int amount)
        {
            ElectricResistance += amount;
        }

        public void AddThornsPercent(float amount)
        {
            ThornsPercent += amount;
        }

        public void AddResistanceIgnoredPercent(float amount)
        {
            ResistanceIgnoredPercent += amount;
        }

        public void AddFireAbilityDamagePercent(float amount)
        {
            FireAbilityDamagePercent += amount;
        }

        public void AddWaterAbilityDamagePercent(float amount)
        {
            WaterAbilityDamagePercent += amount;
        }

        public void AddElectricAbilityDamagePercent(float amount)
        {
            ElectricAbilityDamagePercent += amount;
        }

        public void AddPhysicalAbilityDamagePercent(float amount)
        {
            PhysicalAbilityDamagePercent += amount;
        }
    }
}
