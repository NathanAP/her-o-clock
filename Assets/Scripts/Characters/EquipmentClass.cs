namespace HerOClock.Characters
{
    /// <summary>
    /// Classe do equipamento usado pelo personagem, conforme items.md.
    /// Ela define como os atributos principais viram atributos secundarios.
    /// </summary>
    public enum EquipmentClass
    {
        /// <summary>Liberado por AGI. Melhor em evasao e velocidade de ataque.</summary>
        Light,

        /// <summary>Liberado por SPE. Melhor em reducao de recarga.</summary>
        Magic,

        /// <summary>Liberado por POW. Melhor em armadura e resistencia.</summary>
        Heavy
    }
}
