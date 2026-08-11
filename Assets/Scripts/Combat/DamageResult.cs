namespace HerOClock.Combat
{
    /// <summary>
    /// What happened in one attack, with every value already rounded.
    ///
    /// It keeps the full breakdown rather than only the final damage, because the view needs
    /// to know whether the attack was evaded in order to show it, and because that is how we
    /// check on screen that the formulas are coming out right.
    /// </summary>
    public struct DamageResult
    {
        /// <summary>Damage applied to the target. Never below 0.</summary>
        public int Damage;

        /// <summary>Health recovered by the target for having above 100% resistance to the element.</summary>
        public int Healing;

        public bool Evaded;

        public bool PerfectEvasion;

        /// <summary>Health recovered by the attacker through life steal.</summary>
        public int LifeStolen;

        /// <summary>Physical damage the target reflects back at the attacker through thorns.</summary>
        public int Thorns;
    }
}
