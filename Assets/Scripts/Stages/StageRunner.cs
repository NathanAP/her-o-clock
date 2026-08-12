using System;
using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.View;
using UnityEngine;

namespace HerOClock.Stages
{
    /// <summary>
    /// Runs a stage from start to finish: minion waves in order, the villain at the end,
    /// victory, defeat and restart.
    ///
    /// Heroes live across the whole stage. Their damage carries from one wave to the next, as
    /// described in the "Desgaste" section of gameplay.md, and only a defeat brings them back
    /// to full health. Enemies are created per wave and thrown away when the wave is over.
    ///
    /// The waiting between waves is driven by a timer rather than a coroutine, so the whole
    /// stage stays inside the single update loop and can be simulated later without rendering.
    /// </summary>
    public class StageRunner : MonoBehaviour
    {
        private enum Phase
        {
            Fighting,
            Advancing,
            Celebrating,
            Restarting
        }

        private BattleGrid grid;
        private BattleDirector director;
        private IReadOnlyDictionary<string, CharacterDefinition> charactersById;
        private BattleRandom random;
        private BoardScroller scroller;
        private Func<CharacterDefinition, Team, GridPosition, int, Character> spawn;

        private StageData stage;
        private readonly List<Character> heroes = new List<Character>();
        private readonly List<Character> enemies = new List<Character>();

        private int waveIndex;
        private Phase phase;
        private float timer;
        private float phaseDuration;

        [Tooltip("Seconds spent walking to the next wave.")]
        [Min(0.1f)]
        public float AdvanceDuration = 1.2f;

        [Tooltip("Seconds spent celebrating before the stage starts over.")]
        [Min(0.1f)]
        public float CelebrationDuration = 3f;

        [Tooltip("Seconds before a lost stage starts over.")]
        [Min(0.1f)]
        public float DefeatDuration = 2.5f;

        public void Configure(
            BattleGrid grid,
            BattleDirector director,
            IReadOnlyDictionary<string, CharacterDefinition> charactersById,
            BattleRandom random,
            BoardScroller scroller,
            IReadOnlyList<Character> heroes,
            Func<CharacterDefinition, Team, GridPosition, int, Character> spawn)
        {
            this.grid = grid;
            this.director = director;
            this.charactersById = charactersById;
            this.random = random;
            this.scroller = scroller;
            this.spawn = spawn;

            this.heroes.Clear();
            this.heroes.AddRange(heroes);

            director.BattleEnded += OnBattleEnded;
        }

        private void OnDestroy()
        {
            if (director != null)
            {
                director.BattleEnded -= OnBattleEnded;
            }
        }

        public void StartStage(StageData stage)
        {
            this.stage = stage;

            Debug.Log("Stage '" + stage.name + "' started. " + stage.lore, this);

            DespawnEnemies();

            // Everyone leaves the board before anyone is placed back, otherwise a character
            // standing on someone else's starting cell would make both claim the same one.
            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i].ClearFromGrid();
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i].ResetForBattle();
            }

            scroller.ResetPosition();

            waveIndex = 0;
            BeginWave();
        }

        private void Update()
        {
            if (phase == Phase.Fighting)
            {
                return;
            }

            timer += Time.deltaTime;

            if (phase == Phase.Advancing)
            {
                scroller.SetProgress(timer / phaseDuration);
            }

            if (timer < phaseDuration)
            {
                return;
            }

            switch (phase)
            {
                case Phase.Advancing:
                    scroller.ResetPosition();
                    BeginWave();
                    break;

                case Phase.Celebrating:
                case Phase.Restarting:
                    StartStage(stage);
                    break;
            }
        }

        private void BeginWave()
        {
            StageWave wave = IsVillainWave ? stage.villainWave : stage.waves[waveIndex];

            SpawnWave(wave);

            List<Character> everyone = new List<Character>(heroes);
            everyone.AddRange(enemies);

            phase = Phase.Fighting;
            director.Begin(grid, everyone, random);
        }

        private bool IsVillainWave
        {
            get { return waveIndex >= stage.waves.Length; }
        }

        private void OnBattleEnded(bool heroesWon)
        {
            if (!heroesWon)
            {
                Debug.Log("The heroes fell on wave " + (waveIndex + 1) + " of '" + stage.name
                    + "'. Starting the stage over.", this);
                EnterPhase(Phase.Restarting, DefeatDuration);
                return;
            }

            bool wasVillain = IsVillainWave;

            DespawnEnemies();
            ReturnHeroesToStart();

            if (wasVillain)
            {
                Debug.Log("Stage '" + stage.name + "' cleared.", this);
                EnterPhase(Phase.Celebrating, CelebrationDuration);
                return;
            }

            waveIndex++;
            EnterPhase(Phase.Advancing, AdvanceDuration);
        }

        private void EnterPhase(Phase next, float duration)
        {
            phase = next;
            timer = 0f;
            phaseDuration = duration;
        }

        private void SpawnWave(StageWave wave)
        {
            for (int i = 0; i < wave.placements.Length; i++)
            {
                StagePlacement placement = wave.placements[i];

                CharacterDefinition definition;
                if (!charactersById.TryGetValue(placement.character, out definition))
                {
                    // Validation already reported this when the stage was loaded.
                    continue;
                }

                GridPosition position = new GridPosition(placement.column, placement.row);

                if (!grid.IsFree(position))
                {
                    Debug.LogWarning("Stage '" + stage.name + "': " + placement.character
                        + " cannot take cell " + position + " because it is occupied.", this);
                    continue;
                }

                enemies.Add(spawn(definition, Team.Enemies, position, stage.enemyLevel));
            }
        }

        private void DespawnEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Character enemy = enemies[i];

                if (enemy == null)
                {
                    continue;
                }

                enemy.ClearFromGrid();
                Destroy(enemy.gameObject);
            }

            enemies.Clear();
        }

        /// <summary>
        /// Sends every hero back to its starting cell keeping the health it has left, since
        /// damage carries across the whole stage. Fallen heroes come back too, so the enemy
        /// area is clear for the next wave.
        /// </summary>
        private void ReturnHeroesToStart()
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
