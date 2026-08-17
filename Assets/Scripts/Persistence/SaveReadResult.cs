namespace HerOClock.Persistence
{
    /// <summary>
    /// What came back from trying to load a save.
    /// </summary>
    public struct SaveReadResult
    {
        /// <summary>False when the folder held nothing readable, which is somebody's first game.</summary>
        public bool Found;

        public SavePayload Payload;

        /// <summary>Name of the file that was used, for the Console and for bug reports.</summary>
        public string FileName;

        /// <summary>
        /// False when the signature did not match the contents.
        ///
        /// The save is loaded anyway, and carries <see cref="SavePayload.IntegrityBroken"/> from
        /// then on. Refusing would punish the player whose disk dropped a byte, which is the case
        /// that really happens, and starting them over would be worse still.
        /// </summary>
        public bool SignatureMatched;
    }
}
