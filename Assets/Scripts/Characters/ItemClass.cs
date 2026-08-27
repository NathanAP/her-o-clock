namespace HerOClock.Characters
{
    /// <summary>
    /// Class of a single piece of equipment, from the `Classes` section of items.md.
    ///
    /// There are six here and only three in <see cref="EquipmentClass"/>, and that is not an
    /// oversight. The three are what attributes.md writes numbers for; these six are what an item
    /// can be. The three hybrids are each half of two pure ones, and
    /// <see cref="EquipmentComposition"/> is where that translation happens.
    /// </summary>
    public enum ItemClass
    {
        /// <summary>Unlocked by AGI.</summary>
        Light = 0,

        /// <summary>Unlocked by SPE.</summary>
        Special = 1,

        /// <summary>Unlocked by POW.</summary>
        Heavy = 2,

        /// <summary>Unlocked by POW and AGI. Half light, half heavy.</summary>
        Medium = 3,

        /// <summary>Unlocked by AGI and SPE. Half light, half special.</summary>
        LightSpecial = 4,

        /// <summary>Unlocked by POW and SPE. Half heavy, half special.</summary>
        HeavySpecial = 5
    }
}
