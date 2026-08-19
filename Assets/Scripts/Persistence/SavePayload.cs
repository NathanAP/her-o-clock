using System;

namespace HerOClock.Persistence
{
    /// <summary>
    /// Everything a save file holds, as plain data.
    ///
    /// This is the format, and it is written to disk exactly as declared here. It holds fields
    /// rather than properties because that is what Unity's serialiser reads, and the field names
    /// are the keys in the file, so renaming one is a format change that needs a migration step.
    ///
    /// It stores the **instance** and never the mould: levels, experience and health, never the
    /// base attributes of a sheet or where a hero starts on the board. See save.md.
    /// </summary>
    [Serializable]
    public class SavePayload
    {
        /// <summary>
        /// Version of the format, not of the game. It only moves when the shape of this class
        /// changes in a way an older file cannot satisfy on its own.
        /// </summary>
        public const int CurrentVersion = 1;

        public const string IntegrityOk = "ok";
        public const string IntegrityBroken = "broken";

        public int version = CurrentVersion;

        /// <summary>
        /// When this file was written, in UTC and round-trip format.
        ///
        /// This is the instant the offline progression measures from, because it is the only one
        /// that can be trusted: a killed process never gets to record that it was closing.
        /// </summary>
        public string savedAtUtc;

        /// <summary>
        /// <see cref="IntegrityOk"/>, or <see cref="IntegrityBroken"/> once a file has failed its
        /// signature. Broken never goes back to ok: it is a fact about this save's history.
        /// </summary>
        public string integrity = IntegrityOk;

        public long money;

        public StageSave stage = new StageSave();

        public HeroSave[] heroes = new HeroSave[0];

        /// <summary>
        /// Team, bench and unlocked positions.
        ///
        /// Added without moving the format version, because a file written before it arrives with
        /// zero slots and the mapper rebuilds it from the heroes already in the file. That is the
        /// "a new field with a sensible default needs no conversion step" rule of
        /// <see cref="SaveMigration"/> being used rather than described.
        /// </summary>
        public RosterSave roster = new RosterSave();

        public ActivitySave activity = new ActivitySave();
    }
}
