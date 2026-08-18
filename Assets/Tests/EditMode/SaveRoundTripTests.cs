using System;
using System.IO;
using HerOClock.Characters;
using HerOClock.Persistence;
using HerOClock.Progression;
using NUnit.Framework;
using Attribute = HerOClock.Characters.Attribute;

namespace HerOClock.Tests
{
    /// <summary>
    /// Saving and loading a real party, all the way through a file on disk.
    ///
    /// This is the category that only exists in this version, and it is the one that stops a release
    /// from corrupting the progress of somebody who already plays. Everything else about persistence
    /// can be wrong in a way that is annoying; this can be wrong in a way that is unrecoverable.
    ///
    /// It goes through <see cref="SaveStore"/> rather than handing a payload straight back, because
    /// the serialiser is part of what is being checked: a field the format does not actually carry
    /// would pass a test that never wrote anything down.
    /// </summary>
    public class SaveRoundTripTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

        private TestBattle battle;
        private string folder;
        private SaveStore store;

        [SetUp]
        public void SetUp()
        {
            battle = new TestBattle();
            folder = Path.Combine(Path.GetTempPath(), "her-o-clock-tests", Guid.NewGuid().ToString("N"));
            store = new SaveStore(folder);
        }

        [TearDown]
        public void TearDown()
        {
            battle.Dispose();

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        private CharacterDefinition Sheet()
        {
            return battle.Sheet("some-hero", CharacterKind.Hero,
                power: 10, agility: 5, specialty: 3, constitution: 20);
        }

        /// <summary>Writes a party and reads it back into a party built the way a new session builds one.</summary>
        private Character SaveAndReload(Character hero, out SaveReadResult read)
        {
            PlayerWallet wallet = new PlayerWallet();
            wallet.Add(4321);

            SavePayload payload = SaveMapper.Capture(
                new[] { hero }, wallet, new ActivityLog(), "act1-stage2", SavePayload.IntegrityOk);

            store.Write(payload, Noon);

            Character reborn = battle.Spawn(hero.Definition, Team.Heroes, 4, 2);

            read = store.Load();
            Assert.IsTrue(read.Found, "The save that was just written could not be read back.");

            SaveMapper.ApplyHeroes(read.Payload, new[] { reborn });

            return reborn;
        }

        // --- The whole party ---

        /// <summary>
        /// A hero with everything at once: levels gained, a build placed by hand, points still
        /// unspent, and damage taken. Anything the format forgets shows up here.
        /// </summary>
        [Test]
        public void AHeroComesBackExactlyAsItWasSaved()
        {
            Character hero = battle.Spawn(Sheet(), Team.Heroes, 2, 1, level: 10);

            // Both origins of the points are exercised: some placed by hand, the rest still waiting.
            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();
            hero.Attributes.Spend(Attribute.Power, 20);

            hero.AwardExperience(ExperienceTable.XpToNextLevel(10) + 7);

            int expectedMaxHealth = hero.Stats.MaxHealth;

            SaveReadResult read;
            Character reborn = SaveAndReload(hero, out read);

            Assert.AreEqual(11, reborn.Progress.Level, "The level was lost.");
            Assert.AreEqual(7L, reborn.Progress.CurrentXp, "The experience towards the next level was lost.");
            Assert.AreEqual(1, reborn.Progress.SkillPoints, "The skill point earned by levelling was lost.");

            Assert.IsFalse(reborn.Attributes.IsAutomatic, "The automatic distribution came back switched on.");
            Assert.AreEqual(20, reborn.Attributes.ManualOn(Attribute.Power), "The build placed by hand was lost.");
            Assert.AreEqual(hero.Attributes.Unspent, reborn.Attributes.Unspent, "The unspent points were lost.");
            Assert.AreEqual(hero.Attributes.AutomaticPoints, reborn.Attributes.AutomaticPoints);
            Assert.AreEqual(hero.Attributes.Granted, reborn.Attributes.Granted);

            for (int i = 0; i < 4; i++)
            {
                Attribute attribute = (Attribute)i;
                Assert.AreEqual(hero.Attributes.SpentOn(attribute), reborn.Attributes.SpentOn(attribute),
                    "The points on " + attribute + " came back different.");
            }

            Assert.AreEqual(expectedMaxHealth, reborn.Stats.MaxHealth,
                "The stats were not rebuilt from the restored level, so every derived number is wrong.");

            Assert.AreEqual(4321L, read.Payload.money, "The money was lost.");
            Assert.AreEqual("act1-stage2", read.Payload.stage.id);
        }

        /// <summary>
        /// The save carries no health at all, because a stage always begins from its first wave with
        /// everybody whole. A field that is written down and then ignored is worse than no field: it
        /// reads like a promise the game does not keep.
        /// </summary>
        [Test]
        public void TheSaveHoldsNoHealthAndNoPositionInsideAStage()
        {
            Character hero = battle.Spawn(Sheet(), Team.Heroes, 2, 1, level: 5);
            hero.TakeDamage(hero.Stats.MaxHealth - 1);

            SavePayload payload = SaveMapper.Capture(
                new[] { hero }, new PlayerWallet(), new ActivityLog(), "act1-stage1", null);

            store.Write(payload, Noon);

            string written = System.IO.File.ReadAllText(
                System.IO.Path.Combine(folder, store.FileNames()[0]));

            Assert.IsFalse(written.Contains("currentHealth"), "The save is writing health down again.");
            Assert.IsFalse(written.Contains("waveIndex"), "The save is writing a position inside a stage again.");
        }

        /// <summary>
        /// The rule from architecture.md, checked through the file: the automatic share is stored as
        /// a count of points and redistributed whole, so a restored character matches one that
        /// climbed to the same level while playing. Store the split instead and replaying a battle
        /// from a seed quietly stops working.
        /// </summary>
        [Test]
        public void ARestoredHeroMatchesOneThatClimbedToTheSameLevel()
        {
            Character climbed = battle.Spawn(Sheet(), Team.Heroes, 2, 1);
            climbed.AwardExperience(ExperienceTable.TotalXpTo(40));

            Assert.AreEqual(40, climbed.Progress.Level, "The setup did not reach level 40.");

            SaveReadResult read;
            Character reborn = SaveAndReload(climbed, out read);

            Character bornAtForty = battle.Spawn(Sheet(), Team.Heroes, 5, 3, level: 40);

            for (int i = 0; i < 4; i++)
            {
                Attribute attribute = (Attribute)i;

                Assert.AreEqual(bornAtForty.Attributes.SpentOn(attribute), reborn.Attributes.SpentOn(attribute),
                    "A restored level 40 hero and one created at level 40 differ on " + attribute + ".");
            }

            Assert.AreEqual(bornAtForty.Stats.MaxHealth, reborn.Stats.MaxHealth);
        }

        /// <summary>
        /// A party can hold two heroes built from the same sheet, and the formation the game ships
        /// with today holds two of each. Matching a saved entry to the first hero with that id gave
        /// both of them the same progress and quietly lost one on every load. Found by reading an
        /// actual save file, not by reading the code.
        /// </summary>
        [Test]
        public void TwoHeroesFromTheSameSheetKeepTheirOwnProgress()
        {
            CharacterDefinition sheet = Sheet();

            Character first = battle.Spawn(sheet, Team.Heroes, 1, 1, level: 10);
            Character second = battle.Spawn(sheet, Team.Heroes, 2, 1, level: 10);

            second.AwardExperience(ExperienceTable.TotalXpTo(20) - ExperienceTable.TotalXpTo(10));
            first.TakeDamage(10);

            Assert.AreEqual(20, second.Progress.Level, "The setup did not tell the two heroes apart.");

            SavePayload payload = SaveMapper.Capture(
                new[] { first, second }, new PlayerWallet(), new ActivityLog(), "act1-stage1", null);

            store.Write(payload, Noon);

            Character rebornFirst = battle.Spawn(sheet, Team.Heroes, 3, 1);
            Character rebornSecond = battle.Spawn(sheet, Team.Heroes, 4, 1);

            SaveMapper.ApplyHeroes(store.Load().Payload, new[] { rebornFirst, rebornSecond });

            Assert.AreEqual(10, rebornFirst.Progress.Level, "The first hero took the second one's progress.");
            Assert.AreEqual(20, rebornSecond.Progress.Level, "The second hero of the same sheet was never restored.");
        }

        // --- When the save and the content disagree ---

        /// <summary>
        /// A hero the save never heard of starts where its sheet says. This is a hero being added to
        /// the game, not an error.
        /// </summary>
        [Test]
        public void AHeroTheSaveDoesNotKnowKeepsItsSheetLevel()
        {
            CharacterDefinition known = Sheet();
            CharacterDefinition added = battle.Sheet("hero-new", CharacterKind.Hero, power: 5, constitution: 10);

            Character hero = battle.Spawn(known, Team.Heroes, 2, 1, level: 10);

            SavePayload payload = SaveMapper.Capture(
                new[] { hero }, new PlayerWallet(), new ActivityLog(), "act1-stage1", null);

            Character newcomer = battle.Spawn(added, Team.Heroes, 3, 1);
            SaveMapper.ApplyHeroes(payload, new[] { newcomer });

            Assert.AreEqual(1, newcomer.Progress.Level, "A hero missing from the save was given somebody else's level.");
        }

        /// <summary>
        /// A saved hero whose sheet no longer exists is reported and left aside. Nothing else in the
        /// save is affected, because content is edited while the game is being built and none of that
        /// is the player's fault.
        /// </summary>
        [Test]
        public void ASavedHeroWhoseSheetIsGoneDoesNotStopTheRest()
        {
            Character hero = battle.Spawn(Sheet(), Team.Heroes, 2, 1, level: 10);

            SavePayload payload = SaveMapper.Capture(
                new[] { hero }, new PlayerWallet(), new ActivityLog(), "act1-stage1", null);

            HeroSave[] withAGhost = new HeroSave[2];
            withAGhost[0] = new HeroSave { id = "hero-deleted", level = 60 };
            withAGhost[1] = payload.heroes[0];
            payload.heroes = withAGhost;

            Character reborn = battle.Spawn(hero.Definition, Team.Heroes, 3, 1);

            Assert.DoesNotThrow(() => SaveMapper.ApplyHeroes(payload, new[] { reborn }));
            Assert.AreEqual(10, reborn.Progress.Level, "The hero that does exist was not restored.");
        }

        // --- The buckets ---

        [Test]
        public void TheBucketsOfTheLastHourSurviveTheRoundTrip()
        {
            ActivityLog activity = new ActivityLog();
            activity.RecordMoney(100);
            activity.RecordExperience(500);
            activity.RecordEnemyDefeated();
            activity.RecordDamageDealt(700);
            activity.RecordDamageTaken(300);
            activity.RecordHealing(20);
            activity.Advance(ActivityLog.BucketSeconds * 2f);

            SavePayload payload = SaveMapper.Capture(
                new Character[0], new PlayerWallet(), activity, "act1-stage1", null);

            store.Write(payload, Noon);

            ActivityLog reloaded = new ActivityLog();
            SaveMapper.ApplyActivity(store.Load().Payload, reloaded);

            Assert.AreEqual(activity.Buckets.Count, reloaded.Buckets.Count);
            Assert.AreEqual(activity.CoveredSeconds, reloaded.CoveredSeconds, 0.001f);
            Assert.AreEqual(activity.TotalMoney, reloaded.TotalMoney);
            Assert.AreEqual(activity.TotalExperience, reloaded.TotalExperience);
            Assert.AreEqual(activity.TotalEnemiesDefeated, reloaded.TotalEnemiesDefeated);
            Assert.AreEqual(activity.TotalDamageDealt, reloaded.TotalDamageDealt);
            Assert.AreEqual(activity.TotalDamageTaken, reloaded.TotalDamageTaken);
            Assert.AreEqual(activity.TotalHealing, reloaded.TotalHealing);

            Assert.AreEqual(activity.MoneyPerHour, reloaded.MoneyPerHour, 0.001,
                "The rate came back different, so the whole offline credit would be wrong.");
        }

        // --- The mark of a broken save ---

        /// <summary>
        /// Once a save has failed its signature, every save written afterwards says so. It is a fact
        /// about this save's history, not a state it recovers from.
        /// </summary>
        [Test]
        public void TheMarkOfABrokenSaveIsCarriedIntoTheNextOne()
        {
            SavePayload payload = SaveMapper.Capture(
                new Character[0], new PlayerWallet(), new ActivityLog(),
                "act1-stage1", SavePayload.IntegrityBroken);

            store.Write(payload, Noon);

            SaveReadResult read = store.Load();

            Assert.IsTrue(read.SignatureMatched, "The file itself is fine; only the mark is being carried.");
            Assert.AreEqual(SavePayload.IntegrityBroken, read.Payload.integrity);
        }

        [Test]
        public void AFreshSaveIsMarkedAsIntact()
        {
            SavePayload payload = SaveMapper.Capture(
                new Character[0], new PlayerWallet(), new ActivityLog(), "act1-stage1", null);

            store.Write(payload, Noon);

            Assert.AreEqual(SavePayload.IntegrityOk, store.Load().Payload.integrity);
        }
    }
}
