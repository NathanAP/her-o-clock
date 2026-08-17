using HerOClock.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Whether a save survives a game update, against "### Versão e migração" in save.md.
    ///
    /// There are no conversion steps yet, and these are not tests of nothing: the two rules being
    /// checked are what decide whether adding a field to the format costs every player their
    /// progress.
    /// </summary>
    public class SaveMigrationTests
    {
        /// <summary>
        /// The realistic old save: a file written before some section existed, so the field is
        /// simply absent from the text. It has to load, and what is missing has to come out as an
        /// empty one, never as an invented value.
        /// </summary>
        [Test]
        public void ASaveWrittenBeforeASectionExistedStillLoads()
        {
            SavePayload payload = JsonUtility.FromJson<SavePayload>(
                "{\"version\": 1, \"money\": 500, \"savedAtUtc\": \"2026-08-17T14:30:12.4870000Z\"}");

            string problem;
            Assert.IsTrue(SaveMigration.TryUpgrade(payload, out problem), problem);

            Assert.AreEqual(500L, payload.money, "The progress that was in the file was lost.");
            Assert.AreEqual(SavePayload.CurrentVersion, payload.version);

            Assert.IsNotNull(payload.heroes);
            Assert.IsEmpty(payload.heroes);

            Assert.IsNotNull(payload.activity);
            Assert.IsNotNull(payload.activity.buckets);
            Assert.IsEmpty(payload.activity.buckets);

            Assert.IsNotNull(payload.stage);
        }

        /// <summary>
        /// The same guarantee for a payload whose sections really are null, so that no reader
        /// downstream has to check for it. The one that forgot would be the one crashing on
        /// somebody's old save.
        /// </summary>
        [Test]
        public void MissingSectionsBecomeEmptyOnes()
        {
            SavePayload payload = new SavePayload
            {
                stage = null,
                heroes = null,
                activity = null,
                integrity = null
            };

            string problem;
            Assert.IsTrue(SaveMigration.TryUpgrade(payload, out problem), problem);

            Assert.IsNotNull(payload.stage);
            Assert.IsNotNull(payload.heroes);
            Assert.IsNotNull(payload.activity);
            Assert.IsNotNull(payload.activity.buckets);
            Assert.AreEqual(SavePayload.IntegrityOk, payload.integrity);
        }

        [Test]
        public void AHeroWithNoAttributeArrayGetsAFullOne()
        {
            SavePayload payload = new SavePayload
            {
                heroes = new[] { new HeroSave { id = "hero-tank", manualPoints = null } }
            };

            string problem;
            Assert.IsTrue(SaveMigration.TryUpgrade(payload, out problem), problem);

            Assert.AreEqual(4, payload.heroes[0].manualPoints.Length);
        }

        // --- What is refused ---

        /// <summary>
        /// A file from a newer build is skipped rather than read. Guessing what its fields mean is
        /// the easiest way to destroy the progress of somebody who tried a newer version and came
        /// back.
        /// </summary>
        [Test]
        public void ASaveFromANewerFormatIsRefused()
        {
            SavePayload payload = new SavePayload { version = SavePayload.CurrentVersion + 1 };

            string problem;
            Assert.IsFalse(SaveMigration.TryUpgrade(payload, out problem));
            Assert.IsNotNull(problem);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void SomethingWithNoVersionIsNotASave(int version)
        {
            SavePayload payload = new SavePayload { version = version };

            string problem;
            Assert.IsFalse(SaveMigration.TryUpgrade(payload, out problem));
        }

        [Test]
        public void NothingAtAllIsRefusedRatherThanCrashing()
        {
            string problem;
            Assert.IsFalse(SaveMigration.TryUpgrade(null, out problem));
            Assert.IsNotNull(problem);
        }
    }
}
