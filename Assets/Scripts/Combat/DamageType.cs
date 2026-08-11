namespace HerOClock.Combat
{
    /// <summary>
    /// Nature of an attack. A single attack is never physical and elemental at once.
    ///
    /// Physical damage is mitigated by physical armour. The three elements are mitigated by
    /// the resistance to that specific element. The earth element counts as physical damage,
    /// as stated in attributes.md, so it does not appear here.
    /// </summary>
    public enum DamageType
    {
        Physical,
        Fire,
        Water,
        Electric
    }
}
