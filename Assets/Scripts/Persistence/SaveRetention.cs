using System;
using System.Collections.Generic;

namespace HerOClock.Persistence
{
    /// <summary>
    /// Which save files stay and which go.
    ///
    /// The previous saves **are** the backup, so there is no separate backup mechanism. What there
    /// is instead is a ladder, and it has two rungs for two different accidents:
    ///
    /// - The **5 newest** cover a disk that dropped a write moments ago.
    /// - The **newest of each of the 3 most recent days** cover the accident that actually costs a
    ///   player their progress: a bug writing a save that is valid but wrong. A save lands every
    ///   few seconds, so keeping only the newest ones would cover under a minute, and someone who
    ///   noticed the problem the next day would have nothing to go back to.
    ///
    /// Days are the days that have saves in them, not the last three days of the calendar, so
    /// somebody who plays once a month still keeps three sessions.
    ///
    /// A pure function of the file names, with no clock and no disk involved. That is deliberate:
    /// the rule is the part worth checking, and it can be checked by handing it a list of strings.
    /// </summary>
    public static class SaveRetention
    {
        public const int NewestKept = 5;
        public const int DaysKept = 3;

        /// <summary>
        /// The names that should be deleted, out of the ones given.
        ///
        /// A name that does not parse is never returned. Whatever else is in that folder is not
        /// ours to remove, and a save whose name we cannot read is evidence rather than rubbish.
        /// </summary>
        public static List<string> ToDelete(IEnumerable<string> fileNames)
        {
            List<string> deleting = new List<string>();

            if (fileNames == null)
            {
                return deleting;
            }

            List<KeyValuePair<DateTime, string>> saves = new List<KeyValuePair<DateTime, string>>();

            foreach (string name in fileNames)
            {
                DateTime utc;

                if (SaveFileName.TryParse(name, out utc))
                {
                    saves.Add(new KeyValuePair<DateTime, string>(utc, name));
                }
            }

            // Newest first. The name breaks a tie, so the result never depends on the order the
            // folder happened to be listed in.
            saves.Sort((a, b) =>
            {
                int byTime = b.Key.CompareTo(a.Key);
                return byTime != 0 ? byTime : string.CompareOrdinal(b.Value, a.Value);
            });

            HashSet<string> keeping = new HashSet<string>();

            for (int i = 0; i < saves.Count && i < NewestKept; i++)
            {
                keeping.Add(saves[i].Value);
            }

            List<DateTime> days = new List<DateTime>();

            for (int i = 0; i < saves.Count && days.Count < DaysKept; i++)
            {
                DateTime day = saves[i].Key.Date;

                if (days.Contains(day))
                {
                    continue;
                }

                // The list is newest first, so the first file seen on a day is that day's newest.
                days.Add(day);
                keeping.Add(saves[i].Value);
            }

            for (int i = 0; i < saves.Count; i++)
            {
                if (!keeping.Contains(saves[i].Value))
                {
                    deleting.Add(saves[i].Value);
                }
            }

            return deleting;
        }
    }
}
