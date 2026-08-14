using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Stages;
using HerOClock.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Runs the validators against the real content of the project, rather than against examples
    /// built in code.
    ///
    /// The other tests check that the rules work. These check that what actually ships obeys
    /// them. It is the cheapest test in the suite to write and the one that catches the most
    /// boring, most likely mistake: a typo in a stage file, or a sheet added to the project and
    /// forgotten in the database.
    /// </summary>
    public class ContentTests
    {
        private static T Load<T>() where T : ScriptableObject
        {
            string[] found = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            Assert.AreEqual(1, found.Length,
                "Expected exactly one " + typeof(T).Name + " in the project, found " + found.Length + ".");

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(found[0]));
        }

        // --- Character sheets ---

        [Test]
        public void EverySheetInTheProjectIsInTheDatabase()
        {
            CharacterDatabase database = Load<CharacterDatabase>();
            string[] found = AssetDatabase.FindAssets("t:CharacterDefinition");

            for (int i = 0; i < found.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(found[i]);
                CharacterDefinition definition = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path);

                Assert.Contains(definition, database.Characters,
                    path + " is not listed in the CharacterDatabase, so no stage file can use it.");
            }
        }

        [Test]
        public void EveryIdIsPresentAndUnique()
        {
            CharacterDatabase database = Load<CharacterDatabase>();
            HashSet<string> seen = new HashSet<string>();

            for (int i = 0; i < database.Characters.Count; i++)
            {
                CharacterDefinition definition = database.Characters[i];

                Assert.IsNotNull(definition, "Entry " + i + " of the CharacterDatabase is empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(definition.Id),
                    "The sheet at position " + i + " has no id, so nothing can refer to it.");
                Assert.IsTrue(seen.Add(definition.Id), "The id '" + definition.Id + "' is used more than once.");
            }
        }

        /// <summary>
        /// A sheet whose maximum health comes out at zero produces a character that is born dead
        /// and never acts, and Unity reports nothing at all about it.
        /// </summary>
        [Test]
        public void NoSheetIsBornDead()
        {
            CharacterDatabase database = Load<CharacterDatabase>();

            for (int i = 0; i < database.Characters.Count; i++)
            {
                CharacterDefinition definition = database.Characters[i];
                CharacterStats stats = definition.Stats.Clone();
                stats.ApplyInstance(definition.Level, definition.Growth, 1f);

                Assert.Greater(stats.MaxHealth, 0, definition.Id + " has no health at its starting level.");
            }
        }

        /// <summary>
        /// The growth percentages are normalised when the points are handed out, so a total other
        /// than 100 still works. It is almost always a typo, and the numbers stop reading as the
        /// percentages they claim to be.
        /// </summary>
        [Test]
        public void EverySheetSplitsExactlyOneHundredPercent()
        {
            CharacterDatabase database = Load<CharacterDatabase>();

            for (int i = 0; i < database.Characters.Count; i++)
            {
                CharacterDefinition definition = database.Characters[i];

                Assert.AreEqual(100, definition.Growth.Total,
                    definition.Id + " splits " + definition.Growth.Total + " points instead of 100.");
            }
        }

        [Test]
        public void NoSheetHasAMinimumRangeAboveItsMaximum()
        {
            CharacterDatabase database = Load<CharacterDatabase>();

            for (int i = 0; i < database.Characters.Count; i++)
            {
                CharacterDefinition definition = database.Characters[i];

                Assert.LessOrEqual(definition.MinRange, definition.MaxRange,
                    definition.Id + " can never reach any distance at all.");
            }
        }

        // --- Stage files ---

        [Test]
        public void EveryStageFileParsesAndPassesTheValidator()
        {
            StageDatabase stages = Load<StageDatabase>();
            CharacterDatabase characters = Load<CharacterDatabase>();

            Assert.Greater(stages.Stages.Count, 0, "The StageDatabase is empty.");

            Dictionary<string, CharacterKind> kinds =
                CharacterDatabase.KindsOf(characters.BuildIndex());

            for (int i = 0; i < stages.Stages.Count; i++)
            {
                StageData stage = stages.Load(i);

                Assert.IsNotNull(stage, "Stage at position " + i + " could not be read.");

                List<string> problems = StageValidator.Validate(stage, 6, 8, kinds);

                CollectionAssert.IsEmpty(problems,
                    "Stage '" + stage.id + "' has problems: " + string.Join(" | ", problems));
            }
        }

        [Test]
        public void EveryStageIdIsUnique()
        {
            StageDatabase stages = Load<StageDatabase>();
            HashSet<string> seen = new HashSet<string>();

            for (int i = 0; i < stages.Stages.Count; i++)
            {
                StageData stage = stages.Load(i);

                Assert.IsTrue(seen.Add(stage.id), "The stage id '" + stage.id + "' is used more than once.");
            }
        }

        /// <summary>
        /// game-objects/fases.md says minions and villains belong in the enemy half of the board.
        /// The validator does not enforce it, because a stage that breaks the rule still runs,
        /// but an enemy placed in the hero area would fight the formation before it even moves.
        /// </summary>
        [Test]
        public void EveryEnemyStartsInTheEnemyArea()
        {
            StageDatabase stages = Load<StageDatabase>();

            for (int i = 0; i < stages.Stages.Count; i++)
            {
                StageData stage = stages.Load(i);

                for (int w = 0; w < stage.waves.Length; w++)
                {
                    AssertWaveIsInEnemyArea(stage, stage.waves[w], "wave " + (w + 1));
                }

                AssertWaveIsInEnemyArea(stage, stage.villainWave, "the villain wave");
            }
        }

        private static void AssertWaveIsInEnemyArea(StageData stage, StageWave wave, string label)
        {
            for (int i = 0; i < wave.placements.Length; i++)
            {
                StagePlacement placement = wave.placements[i];

                Assert.Greater(placement.row, 4,
                    "Stage '" + stage.id + "', " + label + ": " + placement.character
                    + " starts on row " + placement.row + ", which is the hero area.");
            }
        }

        // --- Text ---

        /// <summary>
        /// The real strings file, checked against the real content: every character and every
        /// stage in the project has its text, and there is no text left over from something that
        /// was deleted.
        /// </summary>
        [Test]
        public void TheStringsFileMatchesTheContent()
        {
            List<string> problems = new List<string>();

            StringTable strings = StringTable.From(
                JsonUtility.FromJson<StringTableData>(StringsFile().text), problems);

            CharacterDatabase characters = Load<CharacterDatabase>();
            StageDatabase stages = Load<StageDatabase>();

            List<string> characterIds = new List<string>();
            for (int i = 0; i < characters.Characters.Count; i++)
            {
                characterIds.Add(characters.Characters[i].Id);
            }

            List<string> stageIds = new List<string>();
            for (int i = 0; i < stages.Stages.Count; i++)
            {
                stageIds.Add(stages.Load(i).id);
            }

            StringTableValidator.Validate(strings, characterIds, stageIds, problems);

            CollectionAssert.IsEmpty(problems, string.Join(" | ", problems));
        }

        /// <summary>
        /// No player facing text may be left inside an asset or a stage file. Catching a stray one
        /// here is what stops the strings file quietly becoming half the truth.
        /// </summary>
        [Test]
        public void NoPlayerFacingTextIsLeftInTheContentFiles()
        {
            string[] stageFiles = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Stages" });

            for (int i = 0; i < stageFiles.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(stageFiles[i]);
                string text = AssetDatabase.LoadAssetAtPath<TextAsset>(path).text;

                Assert.IsFalse(text.Contains("\"name\""), path + " still carries a name. Text belongs in the strings file.");
                Assert.IsFalse(text.Contains("\"lore\""), path + " still carries lore. Text belongs in the strings file.");
            }
        }

        private static TextAsset StringsFile()
        {
            TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Strings/en.json");

            Assert.IsNotNull(file, "Assets/Strings/en.json is missing.");
            return file;
        }

        /// <summary>
        /// The board the validator is told about has to be the board the game actually builds,
        /// otherwise every bounds check in the suite is checking the wrong rectangle.
        /// </summary>
        [Test]
        public void TheBoardIsSixByEightWithFourHeroRows()
        {
            HerOClock.Battle.BattleGridConfig config = Load<HerOClock.Battle.BattleGridConfig>();

            Assert.AreEqual(6, config.Columns);
            Assert.AreEqual(8, config.Rows);
            Assert.AreEqual(4, config.HeroRows);
        }
    }
}
