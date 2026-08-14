using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Stages;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks that every mistake a stage file can contain is reported.
    ///
    /// The validator is the price of holding content in JSON: a reference is a piece of text, so
    /// a typo would otherwise show up at runtime as an empty battlefield with a clean Console.
    /// Each test here is one mistake that must never get through.
    /// </summary>
    public class StageValidatorTests
    {
        private const int Columns = 6;
        private const int Rows = 8;

        private static readonly Dictionary<string, CharacterKind> Known = new Dictionary<string, CharacterKind>
        {
            { "minion-standard", CharacterKind.Minion },
            { "villain-boss", CharacterKind.Villain },
            { "hero-tank", CharacterKind.Hero }
        };

        private static StagePlacement Place(string character, int column, int row, float multiplier = 0f)
        {
            return new StagePlacement
            {
                character = character,
                column = column,
                row = row,
                multiplier = multiplier
            };
        }

        private static StageWave Wave(params StagePlacement[] placements)
        {
            return new StageWave { placements = placements };
        }

        /// <summary>A stage with nothing wrong with it, used as the base of every other case.</summary>
        private static StageData ValidStage()
        {
            return new StageData
            {
                id = "act1-stage1",
                name = "A stage",
                lore = "Something happened.",
                enemyLevel = 1,
                waves = new[] { Wave(Place("minion-standard", 3, 6)) },
                villainWave = Wave(Place("villain-boss", 4, 8))
            };
        }

        private static List<string> Validate(StageData stage)
        {
            return StageValidator.Validate(stage, Columns, Rows, Known);
        }

        private static void AssertRejected(StageData stage, string because)
        {
            Assert.IsNotEmpty(Validate(stage), because);
        }

        [Test]
        public void AValidStagePassesWithNoComplaints()
        {
            CollectionAssert.IsEmpty(Validate(ValidStage()));
        }

        [Test]
        public void AMissingFileIsReportedRatherThanCrashing()
        {
            Assert.IsNotEmpty(Validate(null));
        }

        // --- Stage level fields ---

        [Test]
        public void AStageWithoutAnIdIsRejected()
        {
            StageData stage = ValidStage();
            stage.id = "";

            AssertRejected(stage, "Save games will refer to the stage by id.");
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void AnEnemyLevelBelowOneIsRejected(int level)
        {
            StageData stage = ValidStage();
            stage.enemyLevel = level;

            AssertRejected(stage, "The lowest level in the game is 1.");
        }

        [Test]
        public void AStageWithoutMinionWavesIsRejected()
        {
            StageData stage = ValidStage();
            stage.waves = new StageWave[0];

            AssertRejected(stage, "A stage needs at least one minion wave.");
        }

        [Test]
        public void AnEmptyWaveIsRejected()
        {
            StageData stage = ValidStage();
            stage.waves = new[] { Wave() };

            AssertRejected(stage, "A wave with nobody in it would end instantly.");
        }

        // --- The villain wave ---

        [Test]
        public void AStageWithoutAVillainWaveIsRejected()
        {
            StageData stage = ValidStage();
            stage.villainWave = null;

            AssertRejected(stage, "Every stage ends against a villain.");
        }

        [Test]
        public void AVillainWaveWithoutAVillainIsRejected()
        {
            StageData stage = ValidStage();
            stage.villainWave = Wave(Place("minion-standard", 3, 6));

            AssertRejected(stage, "The final fight has to contain a villain.");
        }

        [Test]
        public void AVillainWaveMayAlsoContainMinions()
        {
            StageData stage = ValidStage();
            stage.villainWave = Wave(Place("minion-standard", 2, 6), Place("villain-boss", 4, 8));

            CollectionAssert.IsEmpty(Validate(stage));
        }

        // --- References ---

        [Test]
        public void AnUnknownCharacterIsRejected()
        {
            StageData stage = ValidStage();
            stage.waves = new[] { Wave(Place("minion-standrad", 3, 6)) };

            AssertRejected(stage, "A typo in an id must not reach runtime.");
        }

        [Test]
        public void AnEmptyCharacterIdIsRejected()
        {
            StageData stage = ValidStage();
            stage.waves = new[] { Wave(Place("", 3, 6)) };

            AssertRejected(stage, "A placement with no character is meaningless.");
        }

        [Test]
        public void AHeroPlacedAsAnEnemyIsRejected()
        {
            StageData stage = ValidStage();
            stage.waves = new[] { Wave(Place("hero-tank", 3, 6)) };

            AssertRejected(stage, "Waves are made of minions and villains.");
        }

        // --- Cells ---

        [TestCase(0, 6)]
        [TestCase(7, 6)]
        [TestCase(3, 0)]
        [TestCase(3, 9)]
        public void ACellOutsideTheBoardIsRejected(int column, int row)
        {
            StageData stage = ValidStage();
            stage.waves = new[] { Wave(Place("minion-standard", column, row)) };

            AssertRejected(stage, "Cell " + column + ", " + row + " is off the board.");
        }

        [Test]
        public void TwoPlacementsOnTheSameCellOfOneWaveIsRejected()
        {
            StageData stage = ValidStage();
            stage.waves = new[] { Wave(Place("minion-standard", 3, 6), Place("minion-standard", 3, 6)) };

            AssertRejected(stage, "Two characters cannot start on the same cell.");
        }

        /// <summary>
        /// The same cell in different waves is fine, because waves never coexist. Getting this
        /// wrong would forbid perfectly good stage files.
        /// </summary>
        [Test]
        public void TheSameCellInDifferentWavesIsAllowed()
        {
            StageData stage = ValidStage();
            stage.waves = new[]
            {
                Wave(Place("minion-standard", 3, 6)),
                Wave(Place("minion-standard", 3, 6))
            };

            CollectionAssert.IsEmpty(Validate(stage));
        }

        /// <summary>
        /// Column and row must not be confused when checking bounds. On a 6 by 8 board, column 8
        /// is outside and row 8 is inside, so swapping them would let a bad file through.
        /// </summary>
        [Test]
        public void ColumnsAndRowsAreCheckedAgainstTheirOwnLimits()
        {
            StageData wide = ValidStage();
            wide.waves = new[] { Wave(Place("minion-standard", 8, 3)) };
            AssertRejected(wide, "Column 8 does not exist on a 6 column board.");

            StageData tall = ValidStage();
            tall.waves = new[] { Wave(Place("minion-standard", 3, 8)) };
            CollectionAssert.IsEmpty(Validate(tall), "Row 8 does exist on an 8 row board.");
        }

        // --- Multiplier ---

        [Test]
        public void ANegativeMultiplierIsRejected()
        {
            StageData stage = ValidStage();
            stage.waves = new[] { Wave(Place("minion-standard", 3, 6, -1f)) };

            AssertRejected(stage, "Leave it out for the normal strength.");
        }

        [Test]
        public void AMissingMultiplierMeansNormalStrength()
        {
            Assert.AreEqual(1f, Place("minion-standard", 3, 6).EffectiveMultiplier);
        }

        [Test]
        public void AGivenMultiplierIsUsedAsWritten()
        {
            Assert.AreEqual(0.5f, Place("minion-standard", 3, 6, 0.5f).EffectiveMultiplier);
        }

        // --- Reporting ---

        /// <summary>
        /// Every problem is reported at once. Stopping at the first would turn fixing a stage
        /// file into one round trip per mistake.
        /// </summary>
        [Test]
        public void EveryProblemIsReportedInOneGo()
        {
            StageData stage = new StageData
            {
                id = "",
                enemyLevel = 0,
                waves = new[] { Wave(Place("nope", 99, 99)) },
                villainWave = Wave(Place("minion-standard", 3, 6))
            };

            Assert.GreaterOrEqual(Validate(stage).Count, 4);
        }
    }
}
