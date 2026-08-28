using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Stages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// The board has to agree with itself on every simulation step.
    ///
    /// Occupancy is written down twice — each character's own `Position`, and the table inside
    /// <see cref="BattleGrid"/> — and nothing ever rebuilds one from the other. So a single
    /// disagreement is permanent, and what a player sees is two bodies drawn in the same square,
    /// because `IsFree` has started lying.
    ///
    /// These tests exist to **find** that moment rather than argue about which of several
    /// plausible causes produces it. They drive the real stage runner across real stages, and
    /// when one fails it names the step and the two characters involved.
    /// </summary>
    public class GridOccupancyTests
    {
        /// <summary>The same fixed seed the balance tests use, so a failure here is reproducible.</summary>
        private const int Seed = 20260813;

        private static T Load<T>() where T : ScriptableObject
        {
            string[] found = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            Assert.AreEqual(1, found.Length,
                "Expected exactly one " + typeof(T).Name + " in the project, found " + found.Length + ".");

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(found[0]));
        }

        private static List<StageSimulation.Placement> Formation()
        {
            string[] found = AssetDatabase.FindAssets("t:BattleFormation");

            for (int i = 0; i < found.Length; i++)
            {
                Setup.BattleFormation formation = AssetDatabase.LoadAssetAtPath<Setup.BattleFormation>(
                    AssetDatabase.GUIDToAssetPath(found[i]));

                if (formation == null || formation.Team != Team.Heroes)
                {
                    continue;
                }

                List<StageSimulation.Placement> placements = new List<StageSimulation.Placement>();

                for (int p = 0; p < formation.Placements.Count; p++)
                {
                    Setup.BattleFormation.Placement placement = formation.Placements[p];

                    placements.Add(new StageSimulation.Placement
                    {
                        Sheet = placement.Character,
                        Column = placement.Column,
                        Row = placement.Row
                    });
                }

                return placements;
            }

            Assert.Fail("No BattleFormation with Team set to Heroes was found.");
            return null;
        }

        /// <summary>
        /// Every stage of the act, at a spread of levels.
        ///
        /// The levels matter as much as the stages: a low level party loses heroes, and a fallen
        /// hero is the one thing that stays on the board while dead, holding its cell. A high
        /// level party clears waves fast, which is where the transitions between them pile up.
        /// </summary>
        [Test]
        public void TheBoardAgreesWithItselfThroughEveryStage(
            [Values(0, 1, 2, 3)] int stageIndex,
            [Values(1, 4, 8)] int heroLevel)
        {
            StageDatabase stages = Load<StageDatabase>();

            if (stageIndex >= stages.Stages.Count)
            {
                Assert.Ignore("There is no stage at position " + stageIndex + ".");
            }

            CharacterDatabase characters = Load<CharacterDatabase>();
            BattleGridConfig config = Load<BattleGridConfig>();

            StageSimulation.Outcome outcome = StageSimulation.Run(
                config,
                Formation(),
                heroLevel,
                stages.Load(stageIndex),
                characters.BuildIndex(),
                Seed,
                watchTheGrid: true);

            Assert.IsNull(outcome.GridProblem,
                "At step " + outcome.GridProblemStep + " of stage " + stageIndex
                + " with a level " + heroLevel + " party: " + outcome.GridProblem);
        }

        /// <summary>
        /// The same stage played by different builds.
        ///
        /// A build changes who walks where and who dies, so it changes which cells are contested.
        /// One party clearing without trouble says nothing about a party that spends the fight
        /// walking into each other.
        /// </summary>
        [Test]
        public void TheBoardAgreesWithItselfAcrossBuilds(
            [Values(0, 1, 2, 3)] int build)
        {
            StageDatabase stages = Load<StageDatabase>();
            CharacterDatabase characters = Load<CharacterDatabase>();
            BattleGridConfig config = Load<BattleGridConfig>();

            List<StageSimulation.Placement> formation = Formation();

            // One attribute at a time, which is the same set the balance sweep uses.
            for (int i = 0; i < formation.Count; i++)
            {
                StageSimulation.Placement placement = formation[i];
                int[] weights = new int[4];
                weights[build] = 100;
                placement.Build = weights;
                formation[i] = placement;
            }

            StageSimulation.Outcome outcome = StageSimulation.Run(
                config,
                formation,
                6,
                stages.Load(stages.Stages.Count - 1),
                characters.BuildIndex(),
                Seed,
                watchTheGrid: true);

            Assert.IsNull(outcome.GridProblem,
                "At step " + outcome.GridProblemStep + " with build " + build + ": " + outcome.GridProblem);
        }
    }
}
