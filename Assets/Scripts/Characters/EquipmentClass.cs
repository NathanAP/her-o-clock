namespace HerOClock.Characters
{
    /// <summary>
    /// Class of the equipment worn by the character, as described in items.md.
    /// It defines how primary attributes turn into secondary ones.
    ///
    /// These three are the ones `attributes.md` writes a constant for. The six classes an
    /// **item** can belong to are a different list, and the rule that collapses a whole set of
    /// worn items into one of the values below does not exist yet.
    ///
    /// The order is the serialised value: a sheet asset stores the number, never the name, so a
    /// member may be renamed but never moved.
    /// </summary>
    public enum EquipmentClass
    {
        /// <summary>Unlocked by AGI. Best at evasion and attack speed.</summary>
        Light,

        /// <summary>Unlocked by SPE. Best at cooldown reduction.</summary>
        Special,

        /// <summary>Unlocked by POW. Best at armour and resistance.</summary>
        Heavy
    }
}
