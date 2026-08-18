namespace HerOClock.Abilities
{
    /// <summary>
    /// The kinds of effect an ability can carry, from abilities.md.
    ///
    /// There are deliberately few of them, and each serves many different abilities. An ability is
    /// never a behaviour of its own: it is a combination of these pieces, and what belongs to the
    /// character is the numbers and which pieces it picked.
    /// </summary>
    public enum EffectType
    {
        /// <summary>Changes a stat for a while. Buff and debuff differ only by the sign of the value.</summary>
        ModifyStat = 0,

        DealDamage = 1,

        /// <summary>Applies a named state, such as untargetable or intangible.</summary>
        ApplyStatus = 2,

        /// <summary>Moves somebody.</summary>
        MoveTo = 3
    }
}
