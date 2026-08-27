using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Progression;
using HerOClock.Stages;
using HerOClock.Text;
using HerOClock.View;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Plays a whole stage in memory: wave after wave, damage carrying across them, villain last.
    ///
    /// It drives the **real** <see cref="StageRunner"/>. It used to repeat the runner's loop
    /// instead, because the runner destroys its enemies between waves and Unity's deferred
    /// <c>Destroy</c> never runs outside Play Mode. That copy was a liability: the balance numbers
    /// were measured against it, so a change to the real loop that nobody mirrored here would have
    /// been reported as "nothing moved". The runner now takes its destroyer as a parameter, which
    /// is all it took to delete the copy.
    ///
    /// What it still does not do is render, and it holds no scene. The transitions between waves do
    /// run, since the runner owns them, but the reported time counts only the fighting, which is
    /// what the balance tables have always compared.
    /// </summary>
    public static class StageSimulation
    {
        public struct Placement
        {
            public CharacterDefinition Sheet;
            public int Column;
            public int Row;

            /// <summary>
            /// How this hero spends its level points, as four weights over POW, AGI, SPE and CON.
            ///
            /// Null leaves the sheet's own distribution doing it, which is what a player who never
            /// touches the screen gets. Anything else is one of the many builds a player could have
            /// chosen instead, and sweeping them is the only way to learn what a stage really asks
            /// for rather than what it asks of one build.
            /// </summary>
            public int[] Build;
        }

        public struct Outcome
        {
            /// <summary>True when the villain fell.</summary>
            public bool Cleared;

            /// <summary>How many minion waves were beaten, not counting the villain.</summary>
            public int WavesCleared;

            /// <summary>Seconds of fighting, with the waiting between waves left out.</summary>
            public float Seconds;

            public long ExperienceAwarded;
            public long MoneyAwarded;

            /// <summary>
            /// Level of the first hero at the end, which is where the experience went.
            ///
            /// Read from the **record** and never from the combatant. A combatant is frozen at the
            /// level it entered with, so asking it would answer "what did this stage fight at",
            /// while what the walls table needs is "what does the player walk out with".
            /// </summary>
            public int FirstHeroLevel;

            /// <summary>True when a wave ran past the time limit instead of being decided.</summary>
            public bool TimedOut;
        }

        /// <summary>
        /// Runs the stage once and reports what happened.
        ///
        /// It stops at the first outcome. The real runner starts the stage over after a victory or
        /// a defeat, since stage selection does not exist yet, and a measurement of "how long does
        /// this stage take" must not include the second attempt.
        ///
        /// The time limit is a safety net: two sides that cannot hurt each other would otherwise
        /// keep the loop going forever.
        /// </summary>
        /// <summary>
        /// Spends this hero's level points by the given weights instead of the sheet's.
        ///
        /// Takes every point back first, so what is placed here is the whole allocation and not an
        /// addition on top of the automatic one. Leftovers from the division go to the attribute
        /// with the largest weight, so the total placed is always exactly what the level granted.
        /// </summary>
        private static void ApplyBuild(HeroRecord hero, int[] weights)
        {
            if (weights == null || weights.Length != 4)
            {
                return;
            }

            hero.Attributes.SetAutomatic(false);
            hero.Attributes.Reset();

            int points = hero.Attributes.Unspent;

            if (points <= 0)
            {
                return;
            }

            int total = 0;
            int heaviest = 0;

            for (int i = 0; i < 4; i++)
            {
                total += weights[i];

                if (weights[i] > weights[heaviest])
                {
                    heaviest = i;
                }
            }

            if (total <= 0)
            {
                return;
            }

            int placed = 0;

            for (int i = 0; i < 4; i++)
            {
                int share = points * weights[i] / total;
                hero.Attributes.Spend((Attribute)i, share);
                placed += share;
            }

            hero.Attributes.Spend((Attribute)heaviest, points - placed);
        }

        public static Outcome Run(
            BattleGridConfig config,
            IReadOnlyList<Placement> formation,
            int heroLevel,
            StageData stage,
            IReadOnlyDictionary<string, CharacterDefinition> charactersById,
            int seed,
            float secondsPerWave = 300f)
        {
            BattleGrid grid = new BattleGrid(config);
            BattleRandom random = new BattleRandom(seed);
            PlayerWallet wallet = new PlayerWallet();

            List<GameObject> spawned = new List<GameObject>();
            Outcome outcome = new Outcome();

            try
            {
                List<Character> heroes = new List<Character>();

                // The stage decides how many heroes walk in, taken from the front of the
                // formation, which is the same order the roster uses for the team. Without this
                // every stage would be measured with the full party, and the early ones would
                // look far easier than they are.
                int party = stage.heroLimit > 0 && stage.heroLimit < formation.Count
                    ? stage.heroLimit
                    : formation.Count;

                for (int i = 0; i < party; i++)
                {
                    Placement placement = formation[i];

                    // The record carries the build, and the combatant is built from it.
                    // Doing it the other way round is not possible any more, which is the point:
                    // a build cannot be applied to somebody already fighting.
                    HeroRecord record = RecordAt(placement.Sheet, heroLevel);
                    ApplyBuild(record, placement.Build);

                    Character hero = SpawnHero(spawned, grid, record,
                        new GridPosition(placement.Column, placement.Row), 1f);

                    heroes.Add(hero);
                }

                GameObject host = new GameObject("Stage");
                spawned.Add(host);

                BattleDirector director = host.AddComponent<BattleDirector>();
                StageRunner runner = host.AddComponent<StageRunner>();

                long experience = 0;
                bool? cleared = null;
                int wavesCleared = 0;

                runner.EnemyDefeated += (enemy, xp, money) => experience += xp;
                runner.WaveCleared += () => wavesCleared++;
                runner.StageEnded += won => cleared = won;

                runner.Configure(new StageContext
                {
                    Grid = grid,
                    Director = director,
                    CharactersById = charactersById,
                    Wallet = wallet,
                    Strings = TextFor(stage),
                    Random = random,
                    Scroller = new BoardScroller(host.transform, config.CellSize),
                    Spawn = (definition, team, position, level, multiplier) =>
                        Spawn(spawned, grid, definition, team, position, level, multiplier),

                    // The whole reason this can drive the real runner. Outside Play Mode the
                    // deferred Destroy never runs, so every enemy of every wave would stay on the
                    // board holding its cell.
                    Destroy = target => Object.DestroyImmediate(target),

                    // The party is already placed, so the simulation hands the runner the same
                    // list every time. It measures one stage and never advances, which is why the
                    // next stage is always this one.
                    BuildParty = forStage => new List<Character>(heroes),
                    NextStage = (current, wasCleared) => current
                });

                runner.StartStage(stage);

                // One budget per wave, so a stage with more waves is not given less room each.
                int limit = Mathf.RoundToInt(secondsPerWave * (stage.waves.Length + 1) / BattleDirector.FixedStep);
                int fightingSteps = 0;

                for (int step = 0; step < limit && !cleared.HasValue; step++)
                {
                    // Read before the step, because the blow that ends a wave lands inside it and
                    // that step is part of the fight.
                    if (runner.IsFighting)
                    {
                        fightingSteps++;
                    }

                    runner.Advance(BattleDirector.FixedStep);
                }

                outcome.Seconds = fightingSteps * BattleDirector.FixedStep;
                outcome.ExperienceAwarded = experience;
                outcome.MoneyAwarded = wallet.Money;
                outcome.WavesCleared = wavesCleared;
                outcome.FirstHeroLevel = heroes[0].Record != null
                    ? heroes[0].Record.Level
                    : heroes[0].Level;
                outcome.TimedOut = !cleared.HasValue;
                outcome.Cleared = cleared.HasValue && cleared.Value;

                return outcome;
            }
            finally
            {
                for (int i = 0; i < spawned.Count; i++)
                {
                    if (spawned[i] != null)
                    {
                        Object.DestroyImmediate(spawned[i]);
                    }
                }
            }
        }

        /// <summary>
        /// A strings table holding what the runner announces, so a headless run does not fill the
        /// Console with missing keys. A missing key comes back readable rather than empty, but there
        /// is no reason to make the noise.
        /// </summary>
        private static StringTable TextFor(StageData stage)
        {
            StringTableData data = new StringTableData
            {
                language = "en",
                entries = new[]
                {
                    new StringEntry { key = StringTable.StageName(stage.id), value = stage.id },
                    new StringEntry { key = StringTable.StageLore(stage.id), value = "Simulated." }
                }
            };

            return StringTable.From(data, new List<string>());
        }

        private static Character Spawn(
            List<GameObject> spawned,
            BattleGrid grid,
            CharacterDefinition definition,
            Team team,
            GridPosition position,
            int level,
            float multiplier)
        {
            GameObject instance = new GameObject(definition.Id);
            Character character = instance.AddComponent<Character>();
            character.Initialize(definition, team, position, grid, level, multiplier);

            spawned.Add(instance);
            return character;
        }

        /// <summary>A hero record already at the level this run is measuring.</summary>
        private static HeroRecord RecordAt(CharacterDefinition sheet, int level)
        {
            HeroRecord record = new HeroRecord(sheet);

            // From wherever the sheet starts, so a sheet that begins above level 1 does not
            // overshoot. The table is cumulative, so the difference is what is owed.
            long owed = ExperienceTable.TotalXpTo(level) - ExperienceTable.TotalXpTo(record.Level);

            if (owed > 0L)
            {
                record.AwardExperience(owed);
            }

            return record;
        }

        /// <summary>The combatant a record sends into the stage being measured.</summary>
        private static Character SpawnHero(
            List<GameObject> spawned,
            BattleGrid grid,
            HeroRecord record,
            GridPosition position,
            float multiplier)
        {
            GameObject instance = new GameObject(record.Id);
            Character character = instance.AddComponent<Character>();
            character.InitializeFrom(record, Team.Heroes, position, grid, multiplier);

            spawned.Add(instance);
            return character;
        }
    }
}
