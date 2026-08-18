namespace HerOClock.Abilities
{
    /// <summary>
    /// The form an ability takes, from the `shape` field in abilities.md.
    /// </summary>
    public enum AbilityShape
    {
        /// <summary>Only whoever used it.</summary>
        Self = 0,

        /// <summary>A single target.</summary>
        Single = 1,

        /// <summary>Everyone inside a rectangle, centred on whoever used it.</summary>
        Area = 2,

        /// <summary>A sequence of targets, each one near the previous.</summary>
        Chain = 3,

        /// <summary>Everyone along one of the eight straight directions from whoever used it.</summary>
        Line = 4
    }
}
