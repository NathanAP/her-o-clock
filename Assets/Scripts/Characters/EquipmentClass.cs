namespace HerOClock.Characters
{
    /// <summary>
    /// Class of the equipment worn by the character, as described in items.md.
    /// It defines how primary attributes turn into secondary ones.
    /// </summary>
    public enum EquipmentClass
    {
        /// <summary>Unlocked by AGI. Best at evasion and attack speed.</summary>
        Light,

        /// <summary>Unlocked by SPE. Best at cooldown reduction.</summary>
        Magic,

        /// <summary>Unlocked by POW. Best at armour and resistance.</summary>
        Heavy
    }
}
