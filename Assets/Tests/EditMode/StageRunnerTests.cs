using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Progression;
using HerOClock.Stages;
using HerOClock.Text;
using HerOClock.View;
using NUnit.Framework;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// How a stage is entered and when it asks for a save, against save.md and the "Desgaste"
    /// section of gameplay.md.
    ///
    /// A stage has exactly one way in, and every route uses it: the first attempt, the restart after
    /// a defeat, and the game being reopened. That is what makes "where inside a stage was I" a
    /// question the save never has to answer.
    /// </summary>
    public class StageRunnerTests
    {
        private TestBattle battle;
        private readonly List<Object> owned = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            battle = new TestBattle();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < owned.Count; i++)
            {
                if (owned[i] != null)
                {
                    Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
            battle.Dispose();
        }

        /// <summary>A stage of two minion waves and a villain, built in code.</summary>
        private static StageData Stage()
        {
            return new StageData
            {
                id = "test-stage",
                enemyLevel = 1,
                waves = new[]
                {
                    Wave("minion", 2, 6),
                    Wave("minion", 3, 6)
                },
                villainWave = Wave("villain", 2, 6)
            };
        }

        private static StageWave Wave(string character, int column, int row)
        {
            return new StageWave
            {
                placements = new[] { new StagePlacement { character = character, column = column, row = row } }
            };
        }

        private Character Hero(int power = 10)
        {
            return battle.Spawn(
                battle.Sheet("hero", CharacterKind.Hero, power: power, agility: 40, constitution: 20),
                Team.Heroes, 2, 1);
        }

        private StageRunner Runner(IReadOnlyList<Character> heroes)
        {
            Dictionary<string, CharacterDefinition> sheets = new Dictionary<string, CharacterDefinition>
            {
                { "minion", battle.Sheet("minion", CharacterKind.Minion, power: 2, constitution: 5) },
                { "villain", battle.Sheet("villain", CharacterKind.Villain, power: 4, constitution: 10) }
            };

            GameObject host = new GameObject("Stage");
            owned.Add(host);

            BattleDirector director = host.AddComponent<BattleDirector>();
            StageRunner runner = host.AddComponent<StageRunner>();

            runner.Configure(new StageContext
            {
                Grid = battle.Grid,
                Director = director,
                CharactersById = sheets,
                Wallet = new PlayerWallet(),
                Strings = StringTable.From(new StringTableData { language = "en", entries = new StringEntry[0] },
                    new List<string>()),
                Random = new BattleRandom(20260817),
                Scroller = new BoardScroller(host.transform, battle.Config.CellSize),
                Spawn = (definition, team, position, level, multiplier) =>
                {
                    GameObject instance = new GameObject(definition.Id);
                    Character character = instance.AddComponent<Character>();
                    character.Initialize(definition, team, position, battle.Grid, level, multiplier);
                    owned.Add(instance);
                    return character;
                },
                Destroy = target => Object.DestroyImmediate(target)
            }, heroes);

            return runner;
        }

        // --- Entering a stage ---

        [Test]
        public void AStageAlwaysBeginsOnItsFirstWave()
        {
            StageRunner runner = Runner(new[] { Hero() });
            runner.StartStage(Stage());

            Assert.AreEqual(0, runner.WaveIndex);
            Assert.IsTrue(runner.IsFighting, "The first wave was not actually begun.");
        }

        /// <summary>
        /// The attrition of gameplay.md is a rule about the inside of a stage: damage carries from
        /// wave to wave, and a stage that starts over starts whole. Being reopened uses this same
        /// entry, so closing the game is worth exactly what losing is worth.
        /// </summary>
        [Test]
        public void StartingAStageBringsEverybodyBackToFullHealth()
        {
            Character hero = Hero();
            hero.TakeDamage(hero.Stats.MaxHealth - 3);

            Assert.AreEqual(3, hero.CurrentHealth, "The setup did not hurt the hero.");

            Runner(new[] { hero }).StartStage(Stage());

            Assert.AreEqual(hero.Stats.MaxHealth, hero.CurrentHealth);
        }

        // --- When a save is asked for ---

        [Test]
        public void BeginningAStageAsksForASave()
        {
            StageRunner runner = Runner(new[] { Hero() });

            int asked = 0;
            runner.StageStarted += () => asked++;

            runner.StartStage(Stage());

            Assert.AreEqual(1, asked);
        }

        /// <summary>
        /// A cleared wave is a save point because of what it paid — experience, money and the
        /// buckets of the last hour — and not because of where the party is standing.
        /// </summary>
        [Test]
        public void ClearingAWaveAsksForASave()
        {
            StageRunner runner = Runner(new[] { Hero(power: 50) });

            int asked = 0;
            runner.WaveCleared += () => asked++;

            runner.StartStage(Stage());

            for (int step = 0; step < 60 * 120 && asked == 0; step++)
            {
                runner.Advance(BattleDirector.FixedStep);
            }

            Assert.AreEqual(1, asked, "No save was asked for when the first wave was cleared.");
        }

        /// <summary>
        /// The reward for an enemy has to reach whoever is counting, since the buckets of the last
        /// hour are what the whole offline progression multiplies.
        /// </summary>
        [Test]
        public void ADefeatedEnemyIsReportedWithWhatItWasWorth()
        {
            StageRunner runner = Runner(new[] { Hero(power: 50) });

            long experience = 0;
            long money = 0;
            int enemies = 0;

            runner.EnemyDefeated += (enemy, xp, cash) =>
            {
                experience += xp;
                money += cash;
                enemies++;
            };

            runner.StartStage(Stage());

            for (int step = 0; step < 60 * 120 && enemies == 0; step++)
            {
                runner.Advance(BattleDirector.FixedStep);
            }

            Assert.AreEqual(1, enemies, "No enemy was reported as defeated.");
            Assert.AreEqual(ExperienceTable.XpFromMinion(1), experience);
            Assert.AreEqual(ExperienceTable.MoneyFromMinion(1), money);
        }
    }
}
