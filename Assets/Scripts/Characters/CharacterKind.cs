namespace HerOClock.Characters
{
    /// <summary>
    /// Type of character. Drives the placeholder colour and the rules that depend on type,
    /// such as minions vanishing on death while heroes stay fallen on the field.
    /// </summary>
    public enum CharacterKind
    {
        Hero,
        Minion,
        Villain
    }
}
