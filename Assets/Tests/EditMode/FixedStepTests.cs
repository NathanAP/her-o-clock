using System;
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
    /// Checks that the simulation is deaf to how real time arrives.
    ///
    /// This is the promise the fixed step exists for. Before it, the loop ran on the frame's own
    /// delta, so the same seed produced a different fight on a different machine and the
    /// developer speed control quietly distorted what it was supposed to accelerate.
    ///
    /// A test that only fed regular deltas would prove nothing, because regular deltas are the
    /// easy case. The deltas here are deliberately ugly.
    /// </summary>
    public class FixedStepTests
    {
        [Test]
        public void TheStepSizeIsFinerThanTheFrameRateTheGameRendersAt()
        {
            Assert.LessOrEqual(BattleDirector.FixedStep, 1f / 30f,
                "The simulation must never be coarser than what the player sees.");
        }

        /// <summary>
        /// A frame rate of 30 delivers exactly two steps per frame, so it must produce the very
        /// same fight as feeding one step at a time, blow for blow and with the same ending.
        /// </summary>
        [Test]
        public void TwoStepsPerFrameGiveExactlyTheSameFightAsOne()
        {
            Run steady = RunStage(Repeat(1f / 60f, 1800));
            Run thirty = RunStage(Repeat(1f / 30f, 900));

            CollectionAssert.AreEqual(steady.Blows, thirty.Blows);
            Assert.AreEqual(steady.Ending, thirty.Ending);
        }

        /// <summary>
        /// Frames of wildly uneven length must produce the same fight as steady ones.
        ///
        /// What is compared is the sequence of blows, not its length. The accumulator turns wall
        /// clock time into steps, and adding up thirty seconds of ragged deltas lands within a
        /// step of adding up thirty seconds of even ones, so the ragged run can be one blow short
        /// at the very end. That is the accumulator being honest about float arithmetic, not the
        /// fight diverging. What must never happen is the two runs disagreeing about a blow they
        /// both simulated.
        /// </summary>
        [Test]
        public void AnIrregularFrameRateDoesNotChangeTheFight()
        {
            Run steady = RunStage(Repeat(1f / 60f, 1800));
            Run jittery = RunStage(Jitter(30f));

            Assert.AreEqual(steady.Blows.Count, jittery.Blows.Count, 2d,
                "The two runs covered very different amounts of simulated time.");

            int common = Math.Min(steady.Blows.Count, jittery.Blows.Count);

            for (int i = 0; i < common; i++)
            {
                Assert.AreEqual(steady.Blows[i], jittery.Blows[i], "The runs diverged at blow " + i + ".");
            }
        }

        [Test]
        public void TheSameDeltasTwiceGiveTheSameFight()
        {
            float[] deltas = Jitter(30f);

            Run first = RunStage(deltas);
            Run second = RunStage(deltas);

            CollectionAssert.AreEqual(first.Blows, second.Blows);
            Assert.AreEqual(first.Ending, second.Ending);
        }

        /// <summary>What one run produced: every blow in order, and where everyone ended up.</summary>
        private sealed class Run
        {
            public readonly List<string> Blows = new List<string>();
            public string Ending;
        }

        private static float[] Repeat(float delta, int count)
        {
            float[] deltas = new float[count];

            for (int i = 0; i < count; i++)
            {
                deltas[i] = delta;
            }

            return deltas;
        }

        /// <summary>
        /// Frames of very uneven length adding up to the requested number of seconds, including
        /// some far longer than any step and some far shorter.
        /// </summary>
        private static float[] Jitter(float seconds)
        {
            float[] pattern = { 0.004f, 0.016f, 0.041f, 0.007f, 0.120f, 0.033f, 0.002f, 0.061f };
            List<float> deltas = new List<float>();

            float total = 0f;
            int index = 0;

            while (total < seconds)
            {
                float delta = pattern[index % pattern.Length];
                index++;

                if (total + delta > seconds)
                {
                    delta = seconds - total;
                }

                deltas.Add(delta);
                total += delta;
            }

            return deltas.ToArray();
        }

        /// <summary>
        /// Runs a stage through the StageRunner, feeding it the given deltas, and writes down
        /// everything that happened.
        ///
        /// Nobody deals damage, so no wave ever ends. That keeps the run inside the walking and
        /// attacking part of the loop, and keeps the StageRunner from destroying anything, which
        /// outside Play Mode would not work anyway.
        /// </summary>
        private static Run RunStage(float[] deltas)
        {
            TestBattle battle = new TestBattle();
            GameObject host = new GameObject("StageRunner");
            Run run = new Run();

            try
            {
                // Both sides deal one point of damage against thousands of health, so the fight
                // cannot finish inside the run. A wave ending would make the StageRunner destroy
                // its enemies, and the deferred Destroy does not run outside Play Mode.
                // The agility is there so evasion is rolled constantly and the random sequence
                // genuinely drives the fight rather than sitting unused.
                CharacterDefinition heroSheet = battle.Sheet("hero", CharacterKind.Hero, power: 1, agility: 37, constitution: 3000);
                CharacterDefinition minionSheet = battle.Sheet("minion", CharacterKind.Minion, power: 1, agility: 13, constitution: 3000);

                Character heroA = battle.Spawn(heroSheet, Team.Heroes, 2, 1);
                Character heroB = battle.Spawn(heroSheet, Team.Heroes, 5, 2);
                List<Character> heroes = new List<Character> { heroA, heroB };

                BattleDirector director = host.AddComponent<BattleDirector>();
                StageRunner runner = host.AddComponent<StageRunner>();

                director.Attacked += (attacker, target, result) =>
                    run.Blows.Add(attacker.name + ">" + target.name + " " + result.Damage
                        + (result.Evaded ? result.PerfectEvasion ? " perfect" : " evaded" : ""));

                Dictionary<string, CharacterDefinition> byId = new Dictionary<string, CharacterDefinition>
                {
                    { minionSheet.Id, minionSheet }
                };

                GameObject board = new GameObject("Board");
                board.transform.SetParent(host.transform, false);

                runner.Configure(new StageContext
                {
                    Grid = battle.Grid,
                    Director = director,
                    CharactersById = byId,
                    Wallet = new PlayerWallet(),
                    Strings = Strings(minionSheet.Id),
                    Random = new BattleRandom(31337),
                    Scroller = new BoardScroller(board.transform, battle.Config.CellSize),
                    Spawn = (definition, team, position, level, multiplier) =>
                        battle.Spawn(definition, team, position.Column, position.Row, level, multiplier),
                    Destroy = target => UnityEngine.Object.DestroyImmediate(target)
                }, heroes);

                runner.StartStage(Stage(minionSheet.Id));

                for (int i = 0; i < deltas.Length; i++)
                {
                    runner.Advance(deltas[i]);
                }

                run.Ending = "heroA " + Describe(heroA) + " | heroB " + Describe(heroB);

                Assert.Greater(run.Blows.Count, 20, "Nothing much happened during the run, so this proves little.");

                return run;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                battle.Dispose();
            }
        }

        private static string Describe(Character character)
        {
            return character.Position + " health " + character.CurrentHealth;
        }

        private static StageData Stage(string minionId)
        {
            return new StageData
            {
                id = "fixed-step",
                enemyLevel = 1,
                waves = new[]
                {
                    new StageWave
                    {
                        placements = new[]
                        {
                            Place(minionId, 3, 7),
                            Place(minionId, 4, 6)
                        }
                    }
                },
                villainWave = new StageWave
                {
                    placements = new[] { Place(minionId, 4, 8) }
                }
            };
        }

        private static StagePlacement Place(string id, int column, int row)
        {
            return new StagePlacement { character = id, column = column, row = row };
        }

        /// <summary>
        /// Just enough text for the stage to announce itself. The StageRunner logs the stage's
        /// name and lore when it starts, and a missing key would come back as a visible marker
        /// rather than throwing, but there is no reason to make the log unreadable.
        /// </summary>
        private static StringTable Strings(string minionId)
        {
            StringTableData data = new StringTableData
            {
                language = "en",
                entries = new[]
                {
                    new StringEntry { key = StringTable.StageName("fixed-step"), value = "Fixed step" },
                    new StringEntry { key = StringTable.StageLore("fixed-step"), value = "A stage that never ends." },
                    new StringEntry { key = StringTable.CharacterName(minionId), value = "Minion" }
                }
            };

            return StringTable.From(data, new List<string>());
        }
    }
}
