using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Progression;
using HerOClock.Stages;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Plays a whole stage in memory: wave after wave, damage carrying across them, villain last.
    ///
    /// It repeats the StageRunner's loop rather than driving it, for one reason: the runner
    /// destroys its enemies between waves, and the deferred Destroy never runs outside Play Mode.
    /// Everything else follows the same rules, and the parts that decide the outcome — the
    /// director, the attackers, the movers, the damage — are the real ones.
    ///
    /// The transitions are skipped, since the ground rolling and the party walking back change
    /// nothing about who wins. The reported time is fighting time only.
    /// </summary>
    public static class StageSimulation
    {
        public struct Placement
        {
            public CharacterDefinition Sheet;
            public int Column;
            public int Row;
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

            /// <summary>Level of the first hero at the end, which is where the experience went.</summary>
            public int FirstHeroLevel;

            /// <summary>True when a wave ran past the time limit instead of being decided.</summary>
            public bool TimedOut;
        }

        /// <summary>
        /// Runs the stage and reports what happened.
        ///
        /// The time limit is a safety net: two sides that cannot hurt each other would otherwise
        /// keep the loop going forever.
        /// </summary>
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

                for (int i = 0; i < formation.Count; i++)
                {
                    Placement placement = formation[i];
                    heroes.Add(Spawn(spawned, grid, placement.Sheet, Team.Heroes,
                        new GridPosition(placement.Column, placement.Row), heroLevel, 1f));
                }

                int waveCount = stage.waves.Length + 1;

                for (int index = 0; index < waveCount; index++)
                {
                    bool isVillainWave = index == stage.waves.Length;
                    StageWave wave = isVillainWave ? stage.villainWave : stage.waves[index];

                    List<Character> enemies = SpawnWave(spawned, grid, wave, stage, charactersById);

                    bool heroesWon = FightWave(grid, heroes, enemies, random, secondsPerWave, ref outcome, wallet);

                    ReleaseEnemies(enemies);

                    if (!heroesWon)
                    {
                        outcome.MoneyAwarded = wallet.Money;
                        outcome.FirstHeroLevel = heroes[0].Level;
                        return outcome;
                    }

                    if (!isVillainWave)
                    {
                        outcome.WavesCleared++;
                    }

                    ReturnHeroesToStart(heroes);
                }

                outcome.Cleared = true;
                outcome.MoneyAwarded = wallet.Money;
                outcome.FirstHeroLevel = heroes[0].Level;
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

        private static bool FightWave(
            BattleGrid grid,
            List<Character> heroes,
            List<Character> enemies,
            BattleRandom random,
            float secondsPerWave,
            ref Outcome outcome,
            PlayerWallet wallet)
        {
            List<Character> everyone = new List<Character>(heroes);
            everyone.AddRange(enemies);

            GameObject host = new GameObject("Director");

            try
            {
                BattleDirector director = host.AddComponent<BattleDirector>();

                long experience = 0;
                long money = 0;

                for (int i = 0; i < enemies.Count; i++)
                {
                    Character enemy = enemies[i];
                    enemy.Died += fallen =>
                    {
                        bool villain = fallen.Kind == CharacterKind.Villain;
                        int level = fallen.Level;

                        experience += villain
                            ? ExperienceTable.XpFromVillain(level)
                            : ExperienceTable.XpFromMinion(level);

                        money += villain
                            ? ExperienceTable.MoneyFromVillain(level)
                            : ExperienceTable.MoneyFromMinion(level);

                        for (int h = 0; h < heroes.Count; h++)
                        {
                            heroes[h].AwardExperience(villain
                                ? ExperienceTable.XpFromVillain(level)
                                : ExperienceTable.XpFromMinion(level));
                        }
                    };
                }

                bool? heroesWon = null;
                director.BattleEnded += won => heroesWon = won;

                director.Begin(grid, everyone, random);

                int limit = Mathf.RoundToInt(secondsPerWave / BattleDirector.FixedStep);
                int step = 0;

                while (step < limit && director.IsRunning)
                {
                    director.Tick(BattleDirector.FixedStep);
                    step++;
                }

                outcome.Seconds += step * BattleDirector.FixedStep;
                outcome.ExperienceAwarded += experience;
                wallet.Add(money);

                if (!heroesWon.HasValue)
                {
                    outcome.TimedOut = true;
                    return false;
                }

                return heroesWon.Value;
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        private static List<Character> SpawnWave(
            List<GameObject> spawned,
            BattleGrid grid,
            StageWave wave,
            StageData stage,
            IReadOnlyDictionary<string, CharacterDefinition> charactersById)
        {
            List<Character> enemies = new List<Character>();

            for (int i = 0; i < wave.placements.Length; i++)
            {
                StagePlacement placement = wave.placements[i];

                CharacterDefinition definition;
                if (!charactersById.TryGetValue(placement.character, out definition))
                {
                    continue;
                }

                GridPosition position = new GridPosition(placement.column, placement.row);

                if (!grid.IsFree(position))
                {
                    continue;
                }

                enemies.Add(Spawn(spawned, grid, definition, Team.Enemies, position,
                    stage.enemyLevel, placement.EffectiveMultiplier));
            }

            return enemies;
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
            GameObject instance = new GameObject(definition.DisplayName);
            Character character = instance.AddComponent<Character>();
            character.Initialize(definition, team, position, grid, level, multiplier);

            spawned.Add(instance);
            return character;
        }

        private static void ReleaseEnemies(List<Character> enemies)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].ClearFromGrid();
                Object.DestroyImmediate(enemies[i].gameObject);
            }
        }

        /// <summary>
        /// Everyone leaves the board before anyone is placed back, otherwise a hero standing on
        /// another hero's starting cell would make both claim the same one.
        /// </summary>
        private static void ReturnHeroesToStart(List<Character> heroes)
        {
            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i].ClearFromGrid();
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i].ReturnToStart();
            }
        }
    }
}
