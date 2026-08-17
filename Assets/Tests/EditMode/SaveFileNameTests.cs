using System;
using HerOClock.Persistence;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The name of a save file, which is also its timestamp, against save.md.
    ///
    /// The property worth protecting is that sorting the names as text sorts them by time. The whole
    /// scheme rests on it: nothing reads a file to find out which save is the newest.
    /// </summary>
    public class SaveFileNameTests
    {
        /// <summary>The exact shape written in save.md.</summary>
        [Test]
        public void TheNameHasTheShapeTheSpecShows()
        {
            DateTime instant = new DateTime(2026, 8, 17, 14, 30, 12, 487, DateTimeKind.Utc);

            Assert.AreEqual("save_20260817T143012487Z.json", SaveFileName.For(instant));
        }

        [Test]
        public void ANameComesBackAsTheInstantThatMadeIt()
        {
            DateTime instant = new DateTime(2026, 8, 17, 14, 30, 12, 487, DateTimeKind.Utc);

            DateTime read;
            Assert.IsTrue(SaveFileName.TryParse(SaveFileName.For(instant), out read));

            Assert.AreEqual(instant, read);
            Assert.AreEqual(DateTimeKind.Utc, read.Kind, "A local instant would reorder the files.");
        }

        /// <summary>
        /// Local time is not used, and this is the reason: two instants an hour apart in the same
        /// timezone must not collide, and the same instant must produce one name anywhere.
        /// </summary>
        [Test]
        public void ALocalInstantIsWrittenAsItsUtcEquivalent()
        {
            DateTime utc = new DateTime(2026, 8, 17, 14, 30, 12, 487, DateTimeKind.Utc);
            DateTime local = utc.ToLocalTime();

            Assert.AreEqual(SaveFileName.For(utc), SaveFileName.For(local));
        }

        /// <summary>
        /// The one property the retention rule and the loader both depend on.
        /// </summary>
        [Test]
        public void SortingTheNamesAsTextSortsThemByTime()
        {
            DateTime early = new DateTime(2026, 8, 17, 9, 59, 59, 999, DateTimeKind.Utc);
            DateTime later = new DateTime(2026, 8, 17, 10, 0, 0, 0, DateTimeKind.Utc);
            DateTime nextYear = new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            Assert.Less(string.CompareOrdinal(SaveFileName.For(early), SaveFileName.For(later)), 0);
            Assert.Less(string.CompareOrdinal(SaveFileName.For(later), SaveFileName.For(nextYear)), 0);
        }

        [Test]
        public void TwoSavesInTheSameSecondGetDifferentNames()
        {
            DateTime first = new DateTime(2026, 8, 17, 14, 30, 12, 100, DateTimeKind.Utc);
            DateTime second = new DateTime(2026, 8, 17, 14, 30, 12, 900, DateTimeKind.Utc);

            Assert.AreNotEqual(SaveFileName.For(first), SaveFileName.For(second));
        }

        // --- What is not one of ours ---

        [TestCase("")]
        [TestCase(null)]
        [TestCase("save_.json")]
        [TestCase("save_20260817T143012487Z.txt")]
        [TestCase("backup_20260817T143012487Z.json")]
        [TestCase("save_20260817.json")]
        [TestCase("save_notadate.json")]
        [TestCase("save_20261317T143012487Z.json")]
        public void ANameThatIsNotOursIsRefused(string name)
        {
            DateTime read;
            Assert.IsFalse(SaveFileName.TryParse(name, out read));
        }
    }
}
