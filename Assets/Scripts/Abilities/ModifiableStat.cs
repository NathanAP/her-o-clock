namespace HerOClock.Abilities
{
    /// <summary>
    /// Which stat a `modify_stat` effect can move.
    ///
    /// The four primaries and the derived values that content actually asks for. The list is
    /// closed on purpose: a sheet naming a stat that is not here is refused by
    /// <see cref="AbilityValidator"/> rather than quietly doing nothing, which is the failure
    /// mode a text field would otherwise have.
    ///
    /// More will be added when an ability needs them. Adding one it does not need yet would be
    /// writing a seam nobody has walked through.
    /// </summary>
    public enum ModifiableStat
    {
        Power = 0,
        Agility = 1,
        Specialty = 2,
        Constitution = 3,

        AttackSpeed = 4,
        MovementSpeed = 5,
        CooldownReduction = 6,
        PhysicalArmor = 7
    }
}
