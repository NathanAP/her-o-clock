using System;
using System.Globalization;

namespace HerOClock.Persistence
{
    /// <summary>
    /// The name of a save file, which is also its timestamp.
    ///
    /// Every save is a new file rather than a rewrite of the same one, so the name has to carry
    /// the instant: <c>save_20260817T143012487Z.json</c>.
    ///
    /// Two details are not free choices:
    ///
    /// - **UTC.** The order of the files is what decides which one is the newest, and local time
    ///   would reorder them when the player changes timezone or daylight saving ends.
    /// - **Milliseconds.** A save lands at the end of every wave, and two of those can fall inside
    ///   the same second.
    ///
    /// The layout is fixed width and big endian, so sorting the names as text sorts them by time,
    /// and nothing has to read a file to know which is the newest.
    /// </summary>
    public static class SaveFileName
    {
        public const string Prefix = "save_";
        public const string Extension = ".json";

        /// <summary>Extension of a file being written and not yet finished.</summary>
        public const string PendingExtension = ".tmp";

        private const string Stamp = "yyyyMMdd'T'HHmmssfff'Z'";

        public static string For(DateTime utc)
        {
            return Prefix + utc.ToUniversalTime().ToString(Stamp, CultureInfo.InvariantCulture) + Extension;
        }

        /// <summary>
        /// Reads the instant back out of a name.
        ///
        /// Anything that does not fit the layout is refused rather than guessed at, and the caller
        /// is expected to leave such a file alone: save.md forbids deleting a file whose name is
        /// not understood.
        /// </summary>
        public static bool TryParse(string fileName, out DateTime utc)
        {
            utc = default(DateTime);

            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (!fileName.StartsWith(Prefix, StringComparison.Ordinal)
                || !fileName.EndsWith(Extension, StringComparison.Ordinal))
            {
                return false;
            }

            int start = Prefix.Length;
            int length = fileName.Length - Prefix.Length - Extension.Length;

            if (length <= 0)
            {
                return false;
            }

            return DateTime.TryParseExact(
                fileName.Substring(start, length),
                Stamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out utc);
        }
    }
}
