using System.Collections.Generic;
using HerOClock.Persistence;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The retention ladder, against the worked example in save.md.
    ///
    /// A pure function of the file names, so the rule that decides whether a player can go back to
    /// yesterday's progress is checked without touching a disk.
    /// </summary>
    public class SaveRetentionTests
    {
        /// <summary>The folder written out in "### Retenção" in save.md, newest first.</summary>
        private static readonly string[] Folder =
        {
            "save_20260817T120000000Z.json",
            "save_20260817T115950000Z.json",
            "save_20260817T115940000Z.json",
            "save_20260817T115930000Z.json",
            "save_20260817T115920000Z.json",
            "save_20260817T115910000Z.json",
            "save_20260816T220000000Z.json",
            "save_20260816T100000000Z.json",
            "save_20260814T090000000Z.json",
            "save_20260811T080000000Z.json"
        };

        /// <summary>
        /// The three names the spec says go, and only those. Copied from the example rather than
        /// from a run of the code.
        /// </summary>
        [Test]
        public void TheSpecExampleDeletesExactlyThreeFiles()
        {
            List<string> deleting = SaveRetention.ToDelete(Folder);

            Assert.AreEqual(3, deleting.Count, "Deleting: " + string.Join(", ", deleting));

            Assert.Contains("save_20260817T115910000Z.json", deleting, "The sixth newest is past the five kept.");
            Assert.Contains("save_20260816T100000000Z.json", deleting, "The 16th already has a newer save.");
            Assert.Contains("save_20260811T080000000Z.json", deleting, "The 11th is outside the three most recent days with saves.");
        }

        [Test]
        public void TheFiveNewestAreAlwaysKept()
        {
            List<string> deleting = SaveRetention.ToDelete(Folder);

            for (int i = 0; i < SaveRetention.NewestKept; i++)
            {
                Assert.IsFalse(deleting.Contains(Folder[i]), Folder[i] + " is one of the newest and was deleted.");
            }
        }

        /// <summary>
        /// The rung that matters most. A save lands every few seconds, so without it a bug that
        /// wrote a valid but wrong save would be unrecoverable by the next day.
        /// </summary>
        [Test]
        public void TheNewestSaveOfEachRecentDayIsKept()
        {
            List<string> deleting = SaveRetention.ToDelete(Folder);

            Assert.IsFalse(deleting.Contains("save_20260816T220000000Z.json"),
                "Yesterday's last save went, so there is nothing to go back to.");
            Assert.IsFalse(deleting.Contains("save_20260814T090000000Z.json"),
                "The third most recent day of play went.");
        }

        /// <summary>
        /// Days with saves, not days of the calendar. Somebody who plays once a month still keeps
        /// three sessions.
        /// </summary>
        [Test]
        public void TheDaysCountedAreTheDaysThatHaveSaves()
        {
            string[] monthsApart =
            {
                "save_20260817T120000000Z.json",
                "save_20260717T120000000Z.json",
                "save_20260617T120000000Z.json"
            };

            Assert.IsEmpty(SaveRetention.ToDelete(monthsApart));
        }

        [Test]
        public void AFolderWithinTheLadderLosesNothing()
        {
            string[] few =
            {
                "save_20260817T120000000Z.json",
                "save_20260817T115950000Z.json"
            };

            Assert.IsEmpty(SaveRetention.ToDelete(few));
        }

        // --- What it must not touch ---

        /// <summary>
        /// save.md forbids deleting a file whose name it does not understand. Whatever else is in
        /// that folder is not ours, and a save we cannot read is evidence rather than rubbish.
        /// </summary>
        [Test]
        public void FilesItCannotParseAreNeverDeleted()
        {
            List<string> names = new List<string>(Folder);
            names.Add("readme.txt");
            names.Add("save_20260817T120000000Z.json.tmp");
            names.Add("settings.json");

            List<string> deleting = SaveRetention.ToDelete(names);

            Assert.IsFalse(deleting.Contains("readme.txt"));
            Assert.IsFalse(deleting.Contains("save_20260817T120000000Z.json.tmp"));
            Assert.IsFalse(deleting.Contains("settings.json"));
            Assert.AreEqual(3, deleting.Count, "Unknown files changed what the ladder decided.");
        }

        [Test]
        public void AnEmptyFolderDeletesNothing()
        {
            Assert.IsEmpty(SaveRetention.ToDelete(new string[0]));
            Assert.IsEmpty(SaveRetention.ToDelete(null));
        }

        /// <summary>
        /// The order the folder was listed in must not change the answer, since a file system makes
        /// no promise about it.
        /// </summary>
        [Test]
        public void TheOrderTheNamesArriveInChangesNothing()
        {
            List<string> reversed = new List<string>(Folder);
            reversed.Reverse();

            List<string> fromOrdered = SaveRetention.ToDelete(Folder);
            List<string> fromReversed = SaveRetention.ToDelete(reversed);

            fromOrdered.Sort();
            fromReversed.Sort();

            Assert.AreEqual(fromOrdered, fromReversed);
        }
    }
}
