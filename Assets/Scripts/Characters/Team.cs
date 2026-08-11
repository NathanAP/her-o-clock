namespace HerOClock.Characters
{
    /// <summary>
    /// Side of the fight. Minions and villains fight together, so they share a team.
    /// Which one is a hero and which is a minion lives in <see cref="CharacterKind"/>.
    /// </summary>
    public enum Team
    {
        Heroes,
        Enemies
    }
}
