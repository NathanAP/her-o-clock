using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Stages;
using HerOClock.Text;
using HerOClock.View;
using NUnit.Framework;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Where the game goes when a stage ends: forward on a win, the same stage again on a defeat.
    ///
    /// The runner never picks the stage itself. It knows one at a time and asks whoever owns the
    /// database, which is what lets the menus of 0.11.0.0 add "stay here and farm" without the
    /// runner learning anything new.
    ///
    /// The party is also read at the start of **every** stage rather than once. That is the rule
    /// that makes a change to the team, the formation or the equipment land on the next stage and
    /// never in the middle of one.
    /// </summary>
    public class StageAdvanceTests
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

        private static StageWave Wave(string character, int column, int row)
        {
            return new StageWave
            {
                placements = new[] { new StagePlacement { character = character, column = column, row = row } }
            };
        }

        private static StageData Stage(string id)
        {
            return new StageData
            {
                id = id,
                enemyLevel = 1,
                waves = new[] { Wave("minion", 2, 6) },
                villainWave = Wave("villain", 2, 6)
            };
        }

        /// <summary>A hero strong enough to clear, or weak enough to be wiped, on demand.</summary>
        private Character Hero(int power)
        {
            return battle.Spawn(
                battle.Sheet("hero", CharacterKind.Hero, power: power, agility: 40, constitution: power > 0 ? 40 : 1),
                Team.Heroes, 2, 1);
        }

        private StageRunner Runner(List<Character> heroes, System.Func<StageData, bool, StageData> next)
        {
            Dictionary<string, CharacterDefinition> sheets = new Dictionary<string, CharacterDefinition>
            {
                { "minion", battle.Sheet("minion", CharacterKind.Minion, power: 2, constitution: 5) },
                { "villain", battle.Sheet("villain", CharacterKind.Villain, power: 40, constitution: 10) }
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
                Wallet = new Progression.PlayerWallet(),
                Strings = StringTable.From(new StringTableData { language = "en", entries = new StringEntry[0] }, new List<string>()),
                Random = new BattleRandom(7),
                Scroller = new BoardScroller(host.transform, battle.Config.CellSize),
                Spawn = (definition, team, position, level, multiplier) =>
                    battle.Spawn(definition, team, position.Column, position.Row, level, multiplier),
                Destroy = target => Object.DestroyImmediate(target),
                BuildParty = forStage => new List<Character>(heroes),
                NextStage = next
            });

            return runner;
        }

        /// <summary>
        /// The runner asks rather than decides, and it asks with the outcome, so a win and a defeat
        /// can lead to different places.
        /// </summary>
        [Test]
        public void TheRunnerAsksWhereToGoWhenAStageEnds()
        {
            List<Character> party = new List<Character> { Hero(200) };

            StageData first = Stage("first");
            StageData second = Stage("second");

            bool asked = false;
            bool toldItWasCleared = false;

            StageRunner runner = Runner(party, (current, cleared) =>
            {
                asked = true;
                toldItWasCleared = cleared;
                return second;
            });

            runner.StartStage(first);

            for (int step = 0; step < 6000 && !asked; step++)
            {
                runner.Advance(BattleDirector.FixedStep);
            }

            Assert.IsTrue(asked, "The stage ended and nobody was asked what comes next.");
            Assert.IsTrue(toldItWasCleared, "The party cleared it, and the answer said otherwise.");
            Assert.AreEqual("second", runner.Stage.id, "It did not move on to the stage it was given.");
        }

        /// <summary>
        /// Losing keeps the player where they are. It is the same entry a first attempt uses, which
        /// is why a save never has to record where inside a stage anybody was.
        /// </summary>
        [Test]
        public void ADefeatIsToldSoAndStaysOnTheSameStage()
        {
            List<Character> party = new List<Character> { Hero(0) };

            StageData only = Stage("only");

            bool asked = false;
            bool toldItWasCleared = true;

            StageRunner runner = Runner(party, (current, cleared) =>
            {
                asked = true;
                toldItWasCleared = cleared;
                return current;
            });

            runner.StartStage(only);

            for (int step = 0; step < 6000 && !asked; step++)
            {
                runner.Advance(BattleDirector.FixedStep);
            }

            Assert.IsTrue(asked, "The party was wiped and nobody was asked what comes next.");
            Assert.IsFalse(toldItWasCleared, "A wipe was reported as a clear.");
            Assert.AreEqual("only", runner.Stage.id);
        }

        /// <summary>
        /// The party is composed per stage, not once. Without this a hero unlocked between stages
        /// would never reach the board, and a stage with a smaller limit would keep whoever the
        /// previous one happened to bring.
        /// </summary>
        [Test]
        public void ThePartyIsReadAgainAtTheStartOfEveryStage()
        {
            List<Character> party = new List<Character> { Hero(200) };

            int timesAsked = 0;

            StageData first = Stage("first");
            StageData second = Stage("second");

            GameObject host = new GameObject("Stage");
            owned.Add(host);

            BattleDirector director = host.AddComponent<BattleDirector>();
            StageRunner runner = host.AddComponent<StageRunner>();

            Dictionary<string, CharacterDefinition> sheets = new Dictionary<string, CharacterDefinition>
            {
                { "minion", battle.Sheet("minion", CharacterKind.Minion, power: 2, constitution: 5) },
                { "villain", battle.Sheet("villain", CharacterKind.Villain, power: 40, constitution: 10) }
            };

            runner.Configure(new StageContext
            {
                Grid = battle.Grid,
                Director = director,
                CharactersById = sheets,
                Wallet = new Progression.PlayerWallet(),
                Strings = StringTable.From(new StringTableData { language = "en", entries = new StringEntry[0] }, new List<string>()),
                Random = new BattleRandom(7),
                Scroller = new BoardScroller(host.transform, battle.Config.CellSize),
                Spawn = (definition, team, position, level, multiplier) =>
                    battle.Spawn(definition, team, position.Column, position.Row, level, multiplier),
                Destroy = target => Object.DestroyImmediate(target),
                BuildParty = forStage =>
                {
                    timesAsked++;
                    return new List<Character>(party);
                },
                NextStage = (current, cleared) => second
            });

            runner.StartStage(first);

            Assert.AreEqual(1, timesAsked, "The party was not read when the first stage began.");

            for (int step = 0; step < 6000 && timesAsked < 2; step++)
            {
                runner.Advance(BattleDirector.FixedStep);
            }

            Assert.AreEqual(2, timesAsked, "Moving to the next stage did not read the party again.");
        }
    }
}
