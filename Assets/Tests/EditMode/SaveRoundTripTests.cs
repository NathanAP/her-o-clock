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

        /// <summary>An item with one rolled modifier, enough to prove the values survive.</summary>
        private static Items.Item Piece(string id, string slot, string itemClass, int level, float value)
        {
            return new Items.Item(id, slot, itemClass, "shiv", "conventional", level,
                new System.Collections.Generic.List<Items.ItemModifierRoll>
                {
                    new Items.ItemModifierRoll("con", "hardware", 3, value)
                });
        }

        /// <summary>
        /// An equipped item survives a write and a read with **the same rolled values**.
        ///
        /// This is the one thing about items the save cannot get wrong. Re-rolling on load would
        /// make an item a different item every time the game opens, and the player would never be
        /// able to keep anything.
        /// </summary>
        [Test]
        public void AnEquippedItemComesBackWithTheSameValues()
        {
            CharacterDefinition sheet = Sheet();
            HeroRecord saved = RecordAt(sheet, 5);
            saved.Equipment.Put(Piece("item-1", "chassis", "heavy", 12, 7.5f));

            store.Write(SaveMapper.Capture(new[] { saved }, new PlayerWallet(), null, "act1Stage1",
                SavePayload.IntegrityOk), Noon);

            HeroRecord loaded = battle.Record(sheet);
            SaveReadResult read = store.Load();

            SaveMapper.ApplyHeroes(read.Payload, new[] { loaded });
            SaveMapper.ApplyItems(read.Payload, new[] { loaded });

            Items.Item back = loaded.Equipment.In("chassis");

            Assert.IsNotNull(back, "The item did not come back at all.");
            Assert.AreEqual("item-1", back.Id);
            Assert.AreEqual("heavy", back.ClassId);
            Assert.AreEqual("shiv", back.SubtypeId);
            Assert.AreEqual(12, back.Level);
            Assert.AreEqual(1, back.Modifiers.Count);
            Assert.AreEqual("con", back.Modifiers[0].Id);
            Assert.AreEqual(3, back.Modifiers[0].Tier, "The tier travels, since the name is built from it.");
            Assert.AreEqual(7.5f, back.Modifiers[0].Value, 0.0001f);
        }

        /// <summary>
        /// A save written before items existed loads with nobody wearing anything, and no
        /// conversion step.
        ///
        /// It is the rule of <see cref="SaveMigration"/> being used rather than described: a new
        /// field takes the value a game that never had it would hold, so the format version does
        /// not move.
        /// </summary>
        [Test]
        public void ASaveWithoutItemsLoadsWithEmptySlots()
        {
            CharacterDefinition sheet = Sheet();
            HeroRecord saved = RecordAt(sheet, 5);

            store.Write(SaveMapper.Capture(new[] { saved }, new PlayerWallet(), null, "act1Stage1",
                SavePayload.IntegrityOk), Noon);

            HeroRecord loaded = battle.Record(sheet);
            SaveReadResult read = store.Load();

            SaveMapper.ApplyHeroes(read.Payload, new[] { loaded });
            SaveMapper.ApplyItems(read.Payload, new[] { loaded });

            Assert.AreEqual(SavePayload.CurrentVersion, read.Payload.version,
                "Items arrived without moving the format version.");
            Assert.AreEqual(0, loaded.Equipment.Count);
        }

        /// <summary>
        /// The same item worn by two heroes is written **once**, since the list holds items and the
        /// heroes hold references. It is what makes moving a piece around cost one line.
        /// </summary>
        [Test]
        public void AnItemIsWrittenOnceHoweverManyHoldersNameIt()
        {
            CharacterDefinition sheet = Sheet();
            HeroRecord first = RecordAt(sheet, 5);
            HeroRecord second = RecordAt(sheet, 5);

            Items.Item shared = Piece("item-1", "chassis", "heavy", 12, 7.5f);
            first.Equipment.Put(shared);
            second.Equipment.Put(shared);

            SavePayload payload = SaveMapper.Capture(new[] { first, second }, new PlayerWallet(), null,
                "act1Stage1", SavePayload.IntegrityOk);

            Assert.AreEqual(1, payload.items.Length);
            Assert.AreEqual("item-1", payload.heroes[0].equipment[0].itemId);
            Assert.AreEqual("item-1", payload.heroes[1].equipment[0].itemId);
        }

        private CharacterDefinition Sheet()
        {
            return battle.Sheet("some-hero", CharacterKind.Hero,
                power: 10, agility: 5, specialty: 3, constitution: 20);
        }

        /// <summary>A record at a given level, the way experience would have got it there.</summary>
        private HeroRecord RecordAt(CharacterDefinition sheet, int level)
        {
            HeroRecord record = battle.Record(sheet);

            // From wherever the sheet starts, so a sheet that begins above level 1 does not
            // overshoot. The table is cumulative, so the difference is what is owed.
            long owed = ExperienceTable.TotalXpTo(level) - ExperienceTable.TotalXpTo(record.Level);

            if (owed > 0L)
            {
                record.AwardExperience(owed);
            }

            return record;
        }

        /// <summary>
        /// Writes a record and reads it back into one built the way a new session builds it.
        ///
        /// Records and not combatants, because that is the whole of what a save holds. A combatant
        /// is built afterwards, from whatever the record ended up saying, and it is built whole --
        /// so there is no health in the file and nothing is missing by its absence.
        /// </summary>
        private HeroRecord SaveAndReload(HeroRecord hero, out SaveReadResult read)
        {
            PlayerWallet wallet = new PlayerWallet();
            wallet.Add(4321);

            SavePayload payload = SaveMapper.Capture(
                new[] { hero }, wallet, new ActivityLog(), "act1Stage2", SavePayload.IntegrityOk);

            store.Write(payload, Noon);

            HeroRecord reborn = battle.Record(hero.Definition);

            read = store.Load();
            Assert.IsTrue(read.Found, "The save that was just written could not be read back.");

            SaveMapper.ApplyHeroes(read.Payload, new[] { reborn });

            return reborn;
        }

        // --- What the reloaded hero fights with ---

        /// <summary>
        /// A reloaded hero fights at the level the file says.
        ///
        /// This was 0.10.2.4: the level lived in two places and the restore moved only one, so the
        /// party fought with the armour of level 1 while holding the points of level 9. The bug
        /// cannot be written any more, since the level lives only on the record and the combatant
        /// is built from it. The assertion stays anyway, because the consequence is what matters
        /// and a later refactor could reintroduce the gap by another route.
        /// </summary>
        [Test]
        public void AReloadedHeroFightsAtTheLevelTheFileSays()
        {
            CharacterDefinition sheet = battle.Sheet("armoured-hero", CharacterKind.Hero,
                power: 10, agility: 5, specialty: 3, constitution: 20, physicalArmor: 20);
            sheet.Stats.PhysicalArmorPerLevel = 20;

            HeroRecord hero = RecordAt(sheet, 9);

            Character before = battle.SpawnHero(hero, Team.Heroes, 2, 1);
            int armourAtNine = before.Stats.PhysicalArmor;
            battle.Disband(before);

            SaveReadResult read;
            HeroRecord reborn = SaveAndReload(hero, out read);

            Assert.AreEqual(9, reborn.Level, "The save lost the level.");

            Character fighting = battle.SpawnHero(reborn, Team.Heroes, 2, 1);

            Assert.AreEqual(9, fighting.Level,
                "The combatant was built at a level the record does not claim.");
            Assert.AreEqual(armourAtNine, fighting.Stats.PhysicalArmor,
                "The reloaded hero came back with the armour of a level 1 character.");
        }

        /// <summary>
        /// A loaded party starts whole, and nothing had to arrange that.
        ///
        /// The format carries no current health and does not need to: health belongs to a
        /// combatant, and the combatant a loaded record sends into its stage has never been hit.
        ///
        /// The interesting part is what is **not** here. There is no top up anywhere. 0.10.2.4
        /// added one to `SaveMapper` and 0.10.3.0 found it was a second answer to a question that
        /// already had one. Now there is not even a question.
        /// </summary>
        [Test]
        public void AReloadedHeroStartsAtFullHealth()
        {
            HeroRecord hero = RecordAt(Sheet(), 9);

            SaveReadResult read;
            HeroRecord reborn = SaveAndReload(hero, out read);

            Character fighting = battle.SpawnHero(reborn, Team.Heroes, 2, 1);

            Assert.AreEqual(fighting.Stats.MaxHealth, fighting.CurrentHealth,
                "The party came back already hurt by the health it gained while loading.");
        }

        // --- The whole party ---

        /// <summary>
        /// A hero with everything at once: levels gained, a build placed by hand, points still
        /// unspent, and damage taken. Anything the format forgets shows up here.
        /// </summary>
        [Test]
        public void AHeroComesBackExactlyAsItWasSaved()
        {
            HeroRecord hero = RecordAt(Sheet(), 10);

            // Both origins of the points are exercised: some placed by hand, the rest still waiting.
            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();
            hero.Attributes.Spend(Attribute.Power, 20);

            hero.AwardExperience(ExperienceTable.XpToNextLevel(10) + 7);

            Character before = battle.SpawnHero(hero, Team.Heroes, 2, 1);
            int expectedMaxHealth = before.Stats.MaxHealth;
            battle.Disband(before);

            SaveReadResult read;
            HeroRecord reborn = SaveAndReload(hero, out read);

            Assert.AreEqual(11, reborn.Progress.Level, "The level was lost.");
            Assert.AreEqual(7L, reborn.Progress.CurrentXp, "The experience towards the next level was lost.");
            // Ten, and not one: this hero climbed from level 1 to level 11, and progress.md grants
            // one skill point per level. The old expectation of one belonged to a hero that was
            // *created* at level 10 and then gained a single level, which is not a thing a record
            // can be any more — a hero gets to a level by earning it.
            Assert.AreEqual(10, reborn.Progress.SkillPoints, "The skill points earned by levelling were lost.");

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

            Character fighting = battle.SpawnHero(reborn, Team.Heroes, 2, 1);

            Assert.AreEqual(expectedMaxHealth, fighting.Stats.MaxHealth,
                "The combatant built from the restored record does not match the one saved.");

            Assert.AreEqual(4321L, read.Payload.money, "The money was lost.");
            Assert.AreEqual("act1Stage2", read.Payload.stage.id);
        }

        /// <summary>
        /// The save carries no health at all, because a stage always begins from its first wave with
        /// everybody whole. A field that is written down and then ignored is worse than no field: it
        /// reads like a promise the game does not keep.
        /// </summary>
        [Test]
        public void TheSaveHoldsNoHealthAndNoPositionInsideAStage()
        {
            HeroRecord hero = RecordAt(Sheet(), 5);

            // The combatant is hurt and the record is what gets written, which is the point: a
            // record has no health to leak into the file even if somebody wanted it to.
            Character fighting = battle.SpawnHero(hero, Team.Heroes, 2, 1);
            fighting.TakeDamage(fighting.Stats.MaxHealth - 1);

            SavePayload payload = SaveMapper.Capture(
                new[] { hero }, new PlayerWallet(), new ActivityLog(), "act1Stage1", null);

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
            HeroRecord climbed = battle.Record(Sheet());
            climbed.AwardExperience(ExperienceTable.TotalXpTo(40));

            Assert.AreEqual(40, climbed.Level, "The setup did not reach level 40.");

            SaveReadResult read;
            HeroRecord reborn = SaveAndReload(climbed, out read);

            Character bornAtForty = battle.Spawn(Sheet(), Team.Heroes, 5, 3, level: 40);
            Character restored = battle.SpawnHero(reborn, Team.Heroes, 2, 1);

            for (int i = 0; i < 4; i++)
            {
                Attribute attribute = (Attribute)i;

                Assert.AreEqual(bornAtForty.Stats.TotalOf(attribute), restored.Stats.TotalOf(attribute),
                    "A restored level 40 hero and one created at level 40 differ on " + attribute + ".");
            }

            Assert.AreEqual(bornAtForty.Stats.MaxHealth, restored.Stats.MaxHealth);
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

            HeroRecord first = RecordAt(sheet, 10);
            HeroRecord second = RecordAt(sheet, 10);

            second.AwardExperience(ExperienceTable.TotalXpTo(20) - ExperienceTable.TotalXpTo(10));

            Assert.AreEqual(20, second.Level, "The setup did not tell the two heroes apart.");

            SavePayload payload = SaveMapper.Capture(
                new[] { first, second }, new PlayerWallet(), new ActivityLog(), "act1Stage1", null);

            store.Write(payload, Noon);

            HeroRecord rebornFirst = battle.Record(sheet);
            HeroRecord rebornSecond = battle.Record(sheet);

            SaveMapper.ApplyHeroes(store.Load().Payload, new[] { rebornFirst, rebornSecond });

            Assert.AreEqual(10, rebornFirst.Level, "The first hero took the second one's progress.");
            Assert.AreEqual(20, rebornSecond.Level, "The second hero of the same sheet was never restored.");
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

            HeroRecord hero = RecordAt(known, 10);

            SavePayload payload = SaveMapper.Capture(
                new[] { hero }, new PlayerWallet(), new ActivityLog(), "act1Stage1", null);

            HeroRecord newcomer = battle.Record(added);
            SaveMapper.ApplyHeroes(payload, new[] { newcomer });

            Assert.AreEqual(1, newcomer.Level, "A hero missing from the save was given somebody else's level.");
        }

        /// <summary>
        /// A saved hero whose sheet no longer exists is reported and left aside. Nothing else in the
        /// save is affected, because content is edited while the game is being built and none of that
        /// is the player's fault.
        /// </summary>
        [Test]
        public void ASavedHeroWhoseSheetIsGoneDoesNotStopTheRest()
        {
            HeroRecord hero = RecordAt(Sheet(), 10);

            SavePayload payload = SaveMapper.Capture(
                new[] { hero }, new PlayerWallet(), new ActivityLog(), "act1Stage1", null);

            HeroSave[] withAGhost = new HeroSave[2];
            withAGhost[0] = new HeroSave { id = "hero-deleted", level = 60 };
            withAGhost[1] = payload.heroes[0];
            payload.heroes = withAGhost;

            HeroRecord reborn = battle.Record(hero.Definition);

            Assert.DoesNotThrow(() => SaveMapper.ApplyHeroes(payload, new[] { reborn }));
            Assert.AreEqual(10, reborn.Level, "The hero that does exist was not restored.");
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
                new HeroRecord[0], new PlayerWallet(), activity, "act1Stage1", null);

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
                new HeroRecord[0], new PlayerWallet(), new ActivityLog(),
                "act1Stage1", SavePayload.IntegrityBroken);

            store.Write(payload, Noon);

            SaveReadResult read = store.Load();

            Assert.IsTrue(read.SignatureMatched, "The file itself is fine; only the mark is being carried.");
            Assert.AreEqual(SavePayload.IntegrityBroken, read.Payload.integrity);
        }

        [Test]
        public void AFreshSaveIsMarkedAsIntact()
        {
            SavePayload payload = SaveMapper.Capture(
                new HeroRecord[0], new PlayerWallet(), new ActivityLog(), "act1Stage1", null);

            store.Write(payload, Noon);

            Assert.AreEqual(SavePayload.IntegrityOk, store.Load().Payload.integrity);
        }
    }
}
