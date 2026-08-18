namespace HerOClock.Characters
{
    /// <summary>
    /// How a character's basic attack presents itself, from the `autoAttacks` block in
    /// characters.md.
    ///
    /// It is **purely visual**, and changes no rule of combat: who decides reach are `MinRange`
    /// and `MaxRange`. A character with a long reach and a melee attack simply hits from far away
    /// without anything flying, which is a perfectly valid thing to want.
    /// </summary>
    public enum AutoAttackType
    {
        /// <summary>The blow lands where the target stands, with nothing travelling.</summary>
        Melee = 0,

        /// <summary>A projectile is drawn flying from the attacker to the target.</summary>
        Ranged = 1
    }
}
