using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Stages;

namespace HerOClock.Tests
{
    /// <summary>
    /// Plays one stage many times, with different parties, different builds and different seeds,
    /// and reports **how many of them cleared it**.
    ///
    /// It exists because the number it replaces was a lie by omission. "The minimum level is 4"
    /// came from a single party, with the sheet's own point distribution, on one seed — a sample of
    /// one, dressed up as a property of the stage. Two players with different builds have entirely
    /// different afternoons on the same stage at the same level, and that is the thing worth
    /// measuring.
    ///
    /// **This is a sample and never a proof.** The space of parties, builds, orders and, later,
    /// items and skill trees is effectively infinite past the opening stages, and no sweep will
    /// come close to covering it. What it buys is a shape: at the recommended level most reasonable
    /// builds get through, and a couple of levels below most do not.
    ///
    /// The sample grows with the space rather than being a flat number. Early on there is genuinely
    /// nothing to vary — one hero at level 1 has no points to spend and no second body to bring —
    /// and spending thousands of runs there would only be slow.
    /// </summary>
    public static class StageSweep
    {
        public struct Result
        {
            public int Level;

            /// <summary>How many of the sampled parties cleared the stage.</summary>
            public int Cleared;

            /// <summary>How many were tried.</summary>
            public int Tried;

            public float ClearedFraction
            {
                get { return Tried <= 0 ? 0f : (float)Cleared / Tried; }
            }
        }

        /// <summary>
        /// The builds a sweep tries, as weights over POW, AGI, SPE and CON.
        ///
        /// The first is null, meaning the sheet's own distribution: the build a player who never
        /// opens the screen ends up with, and the one the content was written against. The rest are
        /// the corners and the common pairings, which is where a build actually goes wrong.
        /// </summary>
        private static readonly int[][] Builds =
        {
            null,
            new[] { 100, 0, 0, 0 },
            new[] { 0, 100, 0, 0 },
            new[] { 0, 0, 100, 0 },
            new[] { 0, 0, 0, 100 },
            new[] { 50, 0, 0, 50 },
            new[] { 0, 50, 50, 0 },
            new[] { 50, 0, 50, 0 },
            new[] { 0, 50, 0, 50 },
            new[] { 25, 25, 25, 25 },
            new[] { 40, 10, 10, 40 },
            new[] { 10, 40, 40, 10 }
        };

        /// <summary>Names for the table, in the same order as <see cref="Builds"/>.</summary>
        private static readonly string[] BuildNames =
        {
            "a da ficha", "só POW", "só AGI", "só SPE", "só CON",
            "POW/CON", "AGI/SPE", "POW/SPE", "AGI/CON",
            "tudo igual", "POW/CON pesado", "AGI/SPE pesado"
        };

        private static readonly int[] Seeds = { 20260813, 991, 40507 };

        /// <summary>How one build did, across every party and seed.</summary>
        public struct BuildResult
        {
            public string Name;
            public int Cleared;
            public int Tried;

            public bool ClearedAny
            {
                get { return Cleared > 0; }
            }
        }

        /// <summary>
        /// How each build fares on a stage, one row per build.
        ///
        /// This is the table that answers the question the project actually cares about: whether a
        /// stage is passable by several different builds or by one. An average hides exactly that —
        /// a stage cleared by half the builds and a stage cleared by one build twice as often look
        /// the same in a percentage.
        /// </summary>
        public static List<BuildResult> PerBuild(
            BattleGridConfig config,
            IReadOnlyList<StageSimulation.Placement> formation,
            int heroLevel,
            StageData stage,
            IReadOnlyDictionary<string, CharacterDefinition> charactersById,
            int builds)
        {
            List<BuildResult> results = new List<BuildResult>();
            List<List<StageSimulation.Placement>> parties = PartiesFor(formation, stage.heroLimit);

            builds = builds < 1 ? 1 : builds > Builds.Length ? Builds.Length : builds;

            for (int b = 0; b < builds; b++)
            {
                BuildResult row = new BuildResult { Name = BuildNames[b] };

                for (int p = 0; p < parties.Count; p++)
                {
                    List<StageSimulation.Placement> party = new List<StageSimulation.Placement>();

                    for (int i = 0; i < parties[p].Count; i++)
                    {
                        StageSimulation.Placement placement = parties[p][i];
                        placement.Build = Builds[b];
                        party.Add(placement);
                    }

                    for (int seed = 0; seed < Seeds.Length; seed++)
                    {
                        row.Tried++;

                        if (StageSimulation.Run(config, party, heroLevel, stage, charactersById, Seeds[seed]).Cleared)
                        {
                            row.Cleared++;
                        }
                    }
                }

                results.Add(row);
            }

            return results;
        }

        /// <summary>
        /// How many builds are worth trying at a level.
        ///
        /// A character at level 1 has spent no points at all, so every build is the same character
        /// and one run says everything there is to say. From there the space opens up with every
        /// level, and the sample follows it instead of being a number somebody picked.
        ///
        /// Called with the level a **stage** recommends, never with the level of one row.
        /// </summary>
        public static int BuildsAt(int heroLevel)
        {
            if (heroLevel <= 1)
            {
                return 1;
            }

            int wanted = 1 + heroLevel;

            return wanted > Builds.Length ? Builds.Length : wanted;
        }

        /// <summary>
        /// Every party a stage could be entered with, in order, up to its hero limit.
        ///
        /// Order matters and is not a detail: it decides who stands in front, and the formation
        /// gives each cell to whoever is there. A replay of an early stage with a full team enters
        /// with that stage's limit, so the smaller parties are real cases and not hypotheticals.
        /// </summary>
        public static List<List<StageSimulation.Placement>> PartiesFor(
            IReadOnlyList<StageSimulation.Placement> formation, int heroLimit)
        {
            List<List<StageSimulation.Placement>> parties = new List<List<StageSimulation.Placement>>();

            int limit = heroLimit > 0 && heroLimit < formation.Count ? heroLimit : formation.Count;

            for (int size = 1; size <= limit; size++)
            {
                Permute(formation, new List<StageSimulation.Placement>(), size, parties);
            }

            return parties;
        }

        private static void Permute(
            IReadOnlyList<StageSimulation.Placement> pool,
            List<StageSimulation.Placement> current,
            int size,
            List<List<StageSimulation.Placement>> into)
        {
            if (current.Count == size)
            {
                into.Add(new List<StageSimulation.Placement>(current));
                return;
            }

            for (int i = 0; i < pool.Count; i++)
            {
                if (current.Contains(pool[i]))
                {
                    continue;
                }

                current.Add(pool[i]);
                Permute(pool, current, size, into);
                current.RemoveAt(current.Count - 1);
            }
        }

        /// <summary>
        /// Runs every combination at one level and counts the clears.
        ///
        /// <paramref name="builds"/> is fixed by the caller and **must be the same for every level
        /// of the same table**. Sizing it per row instead looks reasonable and is a trap: the extra
        /// builds that enter at higher levels are the strange corners, so the share of clears drops
        /// as the level rises and the rows stop being comparable to each other. The sample grows
        /// with the space of the *stage*, not of the row.
        /// </summary>
        public static Result At(
            BattleGridConfig config,
            IReadOnlyList<StageSimulation.Placement> formation,
            int heroLevel,
            StageData stage,
            IReadOnlyDictionary<string, CharacterDefinition> charactersById,
            int builds)
        {
            Result result = new Result { Level = heroLevel };

            List<List<StageSimulation.Placement>> parties = PartiesFor(formation, stage.heroLimit);

            if (builds < 1)
            {
                builds = 1;
            }

            if (builds > Builds.Length)
            {
                builds = Builds.Length;
            }

            for (int p = 0; p < parties.Count; p++)
            {
                for (int b = 0; b < builds; b++)
                {
                    List<StageSimulation.Placement> party = new List<StageSimulation.Placement>();

                    for (int i = 0; i < parties[p].Count; i++)
                    {
                        StageSimulation.Placement placement = parties[p][i];
                        placement.Build = Builds[b];
                        party.Add(placement);
                    }

                    for (int s = 0; s < Seeds.Length; s++)
                    {
                        StageSimulation.Outcome outcome = StageSimulation.Run(
                            config, party, heroLevel, stage, charactersById, Seeds[s]);

                        result.Tried++;

                        if (outcome.Cleared)
                        {
                            result.Cleared++;
                        }
                    }
                }
            }

            return result;
        }
    }
}
