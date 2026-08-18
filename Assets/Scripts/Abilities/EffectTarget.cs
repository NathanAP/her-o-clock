namespace HerOClock.Abilities
{
    /// <summary>
    /// Who one effect lands on, from the `target` field of an effect in abilities.md.
    ///
    /// It is declared per effect rather than per ability so that a single ability can damage the
    /// enemies it found and buff whoever used it.
    /// </summary>
    public enum EffectTarget
    {
        /// <summary>Whoever used the ability.</summary>
        Self = 0,

        /// <summary>Every target the targeting block found.</summary>
        EachTarget = 1,

        /// <summary>Only the first one, which for a chain is where the sequence started.</summary>
        FirstTarget = 2
    }
}
