using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Setup;
using HerOClock.Stages;
using HerOClock.Text;
using HerOClock.View;
using NUnit.Framework;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Where a hero is standing when a wave begins.
    ///
    /// The promise is simple and easy to break without noticing: every wave starts from the
    /// formation, so a hero that chased an enemy across the board in wave two is back on its own
    /// cell for wave three. `gameplay.md` leans on it — the party regroups, the ground rolls, and
    /// the next group arrives against a formation the player arranged.
    ///
    /// The walk back has a **time limit** so that somebody boxed in cannot keep a stage waiting
    /// forever. That safety net is the thing most likely to quietly leave a hero out of place, and
    /// these tests exist to say whether it does.
    /// </summary>
    public class HeroRepositionTests
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

        private static StageWave Wave(params int[] columns)
        {
            StagePlacement[] placements = new StagePlacement[columns.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                placements[i] = new StagePlacement { character = "minion", column = columns[i], row = 8 };
            }

            return new StageWave { placements = placements };
        }

        /// <summary>
        /// Enemies start on the far row, so the heroes have to cross the board to reach them and
        /// genuinely end each wave far from where they started. A wave fought on the doorstep would
        /// pass this test without ever exercising the walk back.
        /// </summary>
        private static StageData Stage()
        {
            return new StageData
            {
                id = "reposition",
                enemyLevel = 1,
                waves = new[] { Wave(2), Wave(5), Wave(3) },
                villainWave = new StageWave
                {
                    placements = new[] { new StagePlacement { character = "villain", column = 4, row = 8 } }
                }
            };
        }

        private StageRunner Runner(List<Character> heroes)
        {
            Dictionary<string, CharacterDefinition> sheets = new Dictionary<string, CharacterDefinition>
            {
                { "minion", battle.Sheet("minion", CharacterKind.Minion, power: 1, constitution: 1) },
                { "villain", battle.Sheet("villain", CharacterKind.Villain, power: 1, constitution: 1) }
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
                Strings = StringTable.From(
                    new StringTableData { language = "en", entries = new StringEntry[0] }, new List<string>()),
                Random = new BattleRandom(4242),
                Scroller = new BoardScroller(host.transform, battle.Config.CellSize),
                Spawn = (definition, team, position, level, multiplier) =>
                    battle.Spawn(definition, team, position.Column, position.Row, level, multiplier),
                Destroy = target => Object.DestroyImmediate(target),
                BuildParty = forStage => new List<Character>(heroes),
                NextStage = (current, cleared) => current
            });

            return runner;
        }

        /// <summary>
        /// Runs the whole stage and checks, every time the fighting resumes, that nobody is standing
        /// anywhere other than the cell the formation gave them.
        /// </summary>
        [Test]
        public void EveryWaveStartsWithTheHeroesBackOnTheirCells()
        {
            List<Character> party = new List<Character>
            {
                battle.Spawn(battle.Sheet("a", CharacterKind.Hero, power: 60, agility: 30, constitution: 60),
                    Team.Heroes, 2, 1),
                battle.Spawn(battle.Sheet("b", CharacterKind.Hero, power: 60, agility: 30, constitution: 60),
                    Team.Heroes, 5, 2)
            };

            Dictionary<Character, GridPosition> cells = new Dictionary<Character, GridPosition>();

            StageRunner runner = Runner(party);
            runner.StartStage(Stage());

            for (int i = 0; i < party.Count; i++)
            {
                cells[party[i]] = party[i].InitialPosition;
            }

            bool wasFighting = true;
            int wavesSeen = 0;
            List<string> problems = new List<string>();

            for (int step = 0; step < 12000; step++)
            {
                runner.Advance(BattleDirector.FixedStep);

                bool fighting = runner.IsFighting;

                // The instant a new wave starts is the instant the fighting resumes.
                if (fighting && !wasFighting)
                {
                    wavesSeen++;

                    for (int i = 0; i < party.Count; i++)
                    {
                        if (!party[i].Position.Equals(cells[party[i]]))
                        {
                            problems.Add("At the start of wave " + (wavesSeen + 1) + ", the hero that belongs on "
                                + cells[party[i]] + " was standing on " + party[i].Position + ".");
                        }
                    }
                }

                wasFighting = fighting;

                if (wavesSeen >= 3)
                {
                    break;
                }
            }

            Assert.Greater(wavesSeen, 0, "No wave ever started again, so nothing was checked.");
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        /// <summary>
        /// The same promise, against the act the game actually ships: the real formation, the real
        /// stages and the real heroes.
        ///
        /// The synthetic stage above proves the walk back works. This one proves it works for the
        /// content, which is where a hero limit, an ally on the board and a formation written for
        /// two heroes all meet.
        /// </summary>
        [Test]
        public void EveryWaveOfTheRealActStartsWithTheHeroesBackOnTheirCells()
        {
            StageDatabase stages = LoadOne<StageDatabase>();
            CharacterDatabase characters = LoadOne<CharacterDatabase>();
            BattleGridConfig config = LoadOne<BattleGridConfig>();
            BattleFormation formation = LoadOne<BattleFormation>();

            List<string> problems = new List<string>();
            int checks = 0;

            for (int index = 0; index < stages.Stages.Count; index++)
            {
                StageData stage = stages.Load(index);

                if (stage == null)
                {
                    continue;
                }

                using (TestBattle fight = new TestBattle(config.Columns, config.Rows))
                {
                    List<Character> party = new List<Character>();
                    int limit = stage.heroLimit > 0 ? stage.heroLimit : formation.Placements.Count;

                    for (int i = 0; i < formation.Placements.Count && party.Count < limit; i++)
                    {
                        BattleFormation.Placement placement = formation.Placements[i];

                        if (placement.Character != null)
                        {
                            party.Add(fight.Spawn(placement.Character, Team.Heroes,
                                placement.Column, placement.Row, 8, 1f));
                        }
                    }

                    checks += RunAndCheck(fight, characters, stage, party, problems);
                }
            }

            Assert.Greater(checks, 0, "No wave of the real act ever started again, so nothing was checked.");
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        private static T LoadOne<T>() where T : ScriptableObject
        {
            string[] found = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name);

            Assert.AreEqual(1, found.Length, "Expected exactly one " + typeof(T).Name + ".");

            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(
                UnityEditor.AssetDatabase.GUIDToAssetPath(found[0]));
        }

        /// <summary>Plays a stage through and reports every hero that began a wave off its cell.</summary>
        private int RunAndCheck(
            TestBattle fight,
            CharacterDatabase characters,
            StageData stage,
            List<Character> party,
            List<string> problems)
        {
            GameObject host = new GameObject("Stage");
            owned.Add(host);

            BattleDirector director = host.AddComponent<BattleDirector>();
            StageRunner runner = host.AddComponent<StageRunner>();

            runner.Configure(new StageContext
            {
                Grid = fight.Grid,
                Director = director,
                CharactersById = characters.BuildIndex(),
                Wallet = new Progression.PlayerWallet(),
                Strings = StringTable.From(
                    new StringTableData { language = "en", entries = new StringEntry[0] }, new List<string>()),
                Random = new BattleRandom(4242),
                Scroller = new BoardScroller(host.transform, fight.Config.CellSize),
                Spawn = (definition, team, position, level, multiplier) =>
                    fight.Spawn(definition, team, position.Column, position.Row, level, multiplier),
                Destroy = target => Object.DestroyImmediate(target),
                BuildParty = forStage => new List<Character>(party),
                NextStage = (current, cleared) => current
            });

            Dictionary<Character, GridPosition> cells = new Dictionary<Character, GridPosition>();

            runner.StartStage(stage);

            for (int i = 0; i < party.Count; i++)
            {
                cells[party[i]] = party[i].InitialPosition;
            }

            bool wasFighting = true;
            int seen = 0;

            for (int step = 0; step < 40000 && seen < stage.waves.Length; step++)
            {
                runner.Advance(BattleDirector.FixedStep);

                if (runner.IsFighting && !wasFighting)
                {
                    seen++;

                    for (int i = 0; i < party.Count; i++)
                    {
                        if (!party[i].Position.Equals(cells[party[i]]))
                        {
                            problems.Add(stage.id + ", wave " + (seen + 1) + ": the hero that belongs on "
                                + cells[party[i]] + " was standing on " + party[i].Position + ".");
                        }
                    }
                }

                wasFighting = runner.IsFighting;
            }

            return seen;
        }

        /// <summary>
        /// A hero that walked away and had its cell taken by nobody still goes home. This is the
        /// plain case, and it failing would mean the walk back is not running at all.
        /// </summary>
        [Test]
        public void AHeroThatChasedAnEnemyIsBroughtHome()
        {
            Character hero = battle.Spawn(
                battle.Sheet("a", CharacterKind.Hero, power: 60, agility: 30, constitution: 60),
                Team.Heroes, 3, 1);

            GridPosition home = hero.InitialPosition;

            StageRunner runner = Runner(new List<Character> { hero });
            runner.StartStage(Stage());

            bool wasFighting = true;
            bool moved = false;

            for (int step = 0; step < 12000; step++)
            {
                runner.Advance(BattleDirector.FixedStep);

                if (!hero.Position.Equals(home))
                {
                    moved = true;
                }

                if (runner.IsFighting && !wasFighting)
                {
                    Assert.IsTrue(moved, "The hero never left its cell, so the walk back was never needed.");
                    Assert.AreEqual(home, hero.Position,
                        "The next wave began with the hero on " + hero.Position + " instead of " + home + ".");
                    return;
                }

                wasFighting = runner.IsFighting;
            }

            Assert.Fail("The first wave never ended, so the walk back was never exercised.");
        }
    }
}
