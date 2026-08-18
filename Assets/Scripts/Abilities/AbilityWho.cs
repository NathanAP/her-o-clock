namespace HerOClock.Abilities
{
    /// <summary>
    /// Who an ability is looking for, from the `who` field in abilities.md.
    ///
    /// It decides among whom the target is chosen, and **it protects nobody**: an area catches
    /// everyone standing in it, ally or enemy alike.
    /// </summary>
    public enum AbilityWho
    {
        Self = 0,
        Allies = 1,
        Enemies = 2
    }
}
