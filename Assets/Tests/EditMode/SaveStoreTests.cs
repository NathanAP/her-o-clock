using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using HerOClock.Persistence;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The folder of save files: writing, finding the newest usable one, and clearing out the rest.
    ///
    /// This one really touches a disk, in a throwaway folder. Everything else about persistence is
    /// pure code and checked without files, but the behaviour that matters here — a broken newest
    /// save falling back to the one before it — only exists in the presence of several files.
    /// </summary>
    public class SaveStoreTests
    {
        private string folder;
        private SaveStore store;

        private static readonly DateTime Noon = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "her-o-clock-tests", Guid.NewGuid().ToString("N"));
            store = new SaveStore(folder);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        private static SavePayload Payload(long money, string stageId)
        {
            SavePayload payload = new SavePayload();
            payload.money = money;
            payload.stage = new StageSave { id = stageId };
            return payload;
        }

        private string WriteRaw(DateTime utc, string payloadText)
        {
            Directory.CreateDirectory(folder);

            string name = SaveFileName.For(utc);
            File.WriteAllText(Path.Combine(folder, name), SaveEnvelope.Wrap(payloadText));

            return name;
        }

        /// <summary>
        /// Edits the money in a save on disk, the way somebody with a text editor would.
        ///
        /// The whole field is matched rather than the bare number, because the signature is
        /// hexadecimal and could happen to contain the same digits.
        /// </summary>
        private void EditMoney(string name, long to)
        {
            string path = Path.Combine(folder, name);
            string before = File.ReadAllText(path);
            string after = Regex.Replace(before, "\"money\": *-?[0-9]+", "\"money\": " + to);

            Assert.AreNotEqual(before, after, "The money field was not found, so nothing was tampered with.");

            File.WriteAllText(path, after);
        }

        // --- Writing and reading back ---

        [Test]
        public void ASaveComesBackWithWhatWasPutIntoIt()
        {
            store.Write(Payload(1500, "act1-stage2"), Noon);

            SaveReadResult read = store.Load();

            Assert.IsTrue(read.Found);
            Assert.AreEqual(1500L, read.Payload.money);
            Assert.AreEqual("act1-stage2", read.Payload.stage.id);
            Assert.IsTrue(read.SignatureMatched, "A file the game just wrote failed its own signature.");
        }

        [Test]
        public void TheInstantOfTheSaveIsRecordedInTheFile()
        {
            store.Write(Payload(0, "act1-stage1"), Noon);

            SaveReadResult read = store.Load();

            DateTime savedAt;
            Assert.IsTrue(DateTime.TryParse(read.Payload.savedAtUtc, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out savedAt));

            Assert.AreEqual(Noon, savedAt.ToUniversalTime(),
                "The offline progression measures from this instant, so it has to be the one written.");
        }

        [Test]
        public void EverySaveIsANewFileAndTheNewestIsTheOneLoaded()
        {
            store.Write(Payload(100, "act1-stage1"), Noon);
            store.Write(Payload(200, "act1-stage1"), Noon.AddSeconds(10));
            store.Write(Payload(300, "act1-stage1"), Noon.AddSeconds(20));

            Assert.AreEqual(3, store.FileNames().Count, "A save overwrote another instead of being a new file.");
            Assert.AreEqual(300L, store.Load().Payload.money);
        }

        /// <summary>
        /// Two saves inside the same millisecond must not collide, or one would silently overwrite
        /// the generation that is supposed to be the backup.
        /// </summary>
        [Test]
        public void TwoSavesAtTheSameInstantAreStillTwoFiles()
        {
            store.Write(Payload(100, "act1-stage1"), Noon);
            store.Write(Payload(200, "act1-stage1"), Noon);

            Assert.AreEqual(2, store.FileNames().Count);
            Assert.AreEqual(200L, store.Load().Payload.money);
        }

        [Test]
        public void AWriteLeavesNoTemporaryFileBehind()
        {
            store.Write(Payload(100, "act1-stage1"), Noon);

            Assert.IsEmpty(Directory.GetFiles(folder, "*" + SaveFileName.PendingExtension));
        }

        [Test]
        public void AFolderWithNothingReadableIsSomebodysFirstGame()
        {
            Assert.IsFalse(store.Load().Found);
        }

        // --- A save that was edited ---

        /// <summary>
        /// The rule from save.md: a save that fails its signature is loaded anyway and marked, and
        /// the file is left alone. Refusing would punish the player whose disk dropped a byte, which
        /// is the case that actually happens.
        /// </summary>
        [Test]
        public void AnEditedSaveIsLoadedAndMarkedRatherThanRefused()
        {
            EditMoney(store.Write(Payload(1500, "act1-stage1"), Noon), 999999);

            SaveReadResult read = store.Load();

            Assert.IsTrue(read.Found, "An edited save was refused, which loses the progress of a damaged file.");
            Assert.IsFalse(read.SignatureMatched);
            Assert.AreEqual(999999L, read.Payload.money, "The save was not actually loaded.");
            Assert.AreEqual(SavePayload.IntegrityBroken, read.Payload.integrity);
        }

        [Test]
        public void AnEditedSaveIsNeverDeleted()
        {
            string name = store.Write(Payload(1500, "act1-stage1"), Noon);
            string path = Path.Combine(folder, name);

            EditMoney(name, 999999);
            store.Load();

            // Enough newer saves to push it well past the ladder.
            for (int i = 1; i <= SaveRetention.NewestKept + 2; i++)
            {
                store.Write(Payload(i, "act1-stage1"), Noon.AddSeconds(i));
                store.Prune();
            }

            Assert.IsTrue(File.Exists(path),
                "The evidence of a broken save was thrown away, so there is nothing left to diagnose.");
        }

        // --- Falling back through the generations ---

        /// <summary>
        /// The reason previous saves are kept at all. A newest file that cannot be read must not be
        /// the end of the story.
        /// </summary>
        [Test]
        public void ANewestFileThatIsNotASaveFallsBackToThePreviousOne()
        {
            store.Write(Payload(1500, "act1-stage1"), Noon);

            File.WriteAllText(Path.Combine(folder, SaveFileName.For(Noon.AddSeconds(10))), "half a fi");

            SaveReadResult read = store.Load();

            Assert.IsTrue(read.Found);
            Assert.AreEqual(1500L, read.Payload.money);
            Assert.AreEqual("act1-stage1", read.Payload.stage.id);
        }

        /// <summary>
        /// A save from a newer build is skipped rather than misread, and the previous generation is
        /// what the player comes back to.
        /// </summary>
        [Test]
        public void ASaveFromANewerFormatIsSkipped()
        {
            store.Write(Payload(1500, "act1-stage1"), Noon);

            WriteRaw(Noon.AddSeconds(10), "{\n    \"version\": 99,\n    \"money\": 7\n}");

            SaveReadResult read = store.Load();

            Assert.IsTrue(read.Found);
            Assert.AreEqual(1500L, read.Payload.money, "The file from the future was read anyway.");
        }

        // --- Pruning ---

        [Test]
        public void PruningKeepsTheLadderAndNothingElse()
        {
            // Ten saves a few seconds apart, all on the same day.
            for (int i = 0; i < 10; i++)
            {
                store.Write(Payload(i, "act1-stage1"), Noon.AddSeconds(i * 10));
            }

            store.Prune();

            // Same day throughout, so the day rung falls inside the five newest.
            Assert.AreEqual(SaveRetention.NewestKept, store.FileNames().Count);
            Assert.AreEqual(9L, store.Load().Payload.money, "Pruning removed the newest save.");
        }

        [Test]
        public void PruningKeepsTheLastSaveOfPreviousDays()
        {
            store.Write(Payload(1, "act1-stage1"), Noon.AddDays(-2));
            store.Write(Payload(2, "act1-stage1"), Noon.AddDays(-1));

            for (int i = 0; i < 8; i++)
            {
                store.Write(Payload(100 + i, "act1-stage1"), Noon.AddSeconds(i * 10));
            }

            store.Prune();

            List<string> names = store.FileNames();

            Assert.AreEqual(SaveRetention.NewestKept + 2, names.Count,
                "Expected the five newest plus one for each of the two earlier days. Kept: " + string.Join(", ", names));

            Assert.IsTrue(names.Contains(SaveFileName.For(Noon.AddDays(-1))), "Yesterday's save went.");
            Assert.IsTrue(names.Contains(SaveFileName.For(Noon.AddDays(-2))), "The day before yesterday's save went.");
        }

        [Test]
        public void PruningRemovesALeftoverTemporaryFile()
        {
            store.Write(Payload(1, "act1-stage1"), Noon);

            string leftover = Path.Combine(folder,
                SaveFileName.For(Noon.AddSeconds(5)) + SaveFileName.PendingExtension);
            File.WriteAllText(leftover, "interrupted");

            store.Prune();

            Assert.IsFalse(File.Exists(leftover), "A write that was interrupted left rubbish in the folder for good.");
        }

        [Test]
        public void PruningLeavesFilesThatAreNotOursAlone()
        {
            store.Write(Payload(1, "act1-stage1"), Noon);

            string stranger = Path.Combine(folder, "notes.txt");
            File.WriteAllText(stranger, "not ours");

            store.Prune();

            Assert.IsTrue(File.Exists(stranger));
        }

        [Test]
        public void PruningAnEmptyFolderDoesNothing()
        {
            Assert.DoesNotThrow(() => store.Prune());
        }

        /// <summary>Files that are not save files never appear in the listing.</summary>
        [Test]
        public void OnlySaveFilesAreListed()
        {
            store.Write(Payload(1, "act1-stage1"), Noon);

            File.WriteAllText(Path.Combine(folder, "notes.txt"), "not ours");
            File.WriteAllText(Path.Combine(folder, "save_nonsense.json"), "not ours either");

            Assert.AreEqual(1, store.FileNames().Count);
        }
    }
}
