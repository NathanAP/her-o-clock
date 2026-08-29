namespace HerOClock.Characters
{
    /// <summary>
    /// How a stat modifier is applied, from the `mode` field of `modifyStat` in abilities.md.
    /// </summary>
    public enum StatModifierMode
    {
        /// <summary>A share of what the character already has. 20 means plus a fifth.</summary>
        Percent = 0,

        /// <summary>Points added straight on top.</summary>
        Flat = 1
    }
}
