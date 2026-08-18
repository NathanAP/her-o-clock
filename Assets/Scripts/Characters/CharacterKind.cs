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
        Villain,

        /// <summary>
        /// Fights beside the party to tell a story, and belongs to nobody.
        ///
        /// It earns no experience, and it does not count towards defeat: the stage is lost when
        /// the player's party falls, even with the NPC still standing. Otherwise an NPC could win
        /// a fight the player had already lost.
        /// </summary>
        Npc
    }
}
