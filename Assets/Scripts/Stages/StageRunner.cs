using System;
using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Movement;
using HerOClock.Progression;
using HerOClock.Text;
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
    /// stage stays inside the single update loop and can be run headless by a test.
    ///
    /// This is also the only place in the game that reads the clock. Real time comes in through
    /// <see cref="Advance"/> and is handed to the simulation in fixed steps, never as the frame's
    /// own delta. Everything downstream — the battle, the walk back, the ground rolling — sees
    /// exactly the same step size regardless of the frame rate of the machine.
    /// </summary>
    public class StageRunner : MonoBehaviour
    {
        private enum Phase
        {
            Fighting,
            Regrouping,
            Advancing,
            Celebrating,
            Restarting
        }

        private StageContext context;
        private Action<GameObject> destroy;

        private StageData stage;
        private readonly List<Character> heroes = new List<Character>();

        /// <summary>
        /// Story characters fighting on the hero side in this stage.
        ///
        /// Kept apart from <see cref="heroes"/> on purpose. That list is index-parallel with the
        /// regroup movers built when the party was configured, and an NPC arrives later, with the
        /// stage. They also answer to different rules: an NPC earns nothing and does not count
        /// towards defeat.
        /// </summary>
        private readonly List<Character> allies = new List<Character>();

        private readonly List<CharacterMover> allyRegroupMovers = new List<CharacterMover>();

        /// <summary>Whether the attempt that just finished cleared the stage, which decides where to go next.</summary>
        private bool lastAttemptCleared;
        private readonly List<Character> enemies = new List<Character>();

        private readonly List<CharacterMover> regroupMovers = new List<CharacterMover>();

        private int waveIndex;
        private Phase phase;
        private float timer;
        private float phaseDuration;
        private float accumulator;

        /// <summary>
        /// Most simulation steps allowed in a single frame.
        ///
        /// A long hitch, a breakpoint or a window left unfocused would otherwise ask for
        /// thousands of steps at once and freeze the game trying to catch up, which only makes
        /// the next frame worse. Past this limit the extra time is dropped: the simulation falls
        /// behind the wall clock, which is the right trade when the alternative is not drawing.
        ///
        /// 300 steps is five seconds of simulation, comfortably above the 16 steps that the
        /// highest developer speed asks for on a 30 frames per second budget.
        /// </summary>
        public const int MaxStepsPerFrame = 300;

        [Tooltip("Seconds the ground keeps rolling after the party has regrouped.")]
        [Min(0.1f)]
        public float AdvanceDuration = 3f;

        [Tooltip("Safety limit for the regrouping walk, in seconds. Whoever has not arrived by then is placed back.")]
        [Min(0.5f)]
        public float MaxRegroupDuration = 6f;

        [Tooltip("Seconds spent celebrating before the stage starts over.")]
        [Min(0.1f)]
        public float CelebrationDuration = 3f;

        [Tooltip("Seconds before a lost stage starts over.")]
        [Min(0.1f)]
        public float DefeatDuration = 2.5f;

        /// <summary>Raised on every simulation step, with the size of that step.</summary>
        public event Action<float> Stepped;

        /// <summary>
        /// Raised when a wave is beaten and the index has already moved to the next one.
        ///
        /// This is a save point, not because of where the party is — a save holds no position
        /// inside a stage — but because of what the wave paid: experience, money and the buckets of
        /// the last hour. The party then spends several seconds walking back and the ground rolls,
        /// so the write lands in a moment that is already a pause.
        /// </summary>
        public event Action WaveCleared;

        /// <summary>
        /// Raised whenever a stage begins, including the restart that follows a victory or a defeat.
        ///
        /// The end of a stage is deliberately not a save point. A defeat is followed by the stage
        /// starting over, so saving on the defeat itself would record the moment of losing rather
        /// than the state the player carries forward.
        /// </summary>
        public event Action StageStarted;

        /// <summary>Raised when the villain falls (true) or the party is wiped (false).</summary>
        public event Action<bool> StageEnded;

        /// <summary>Raised for each enemy that falls, with the experience and money it was worth.</summary>
        public event Action<Character, long, long> EnemyDefeated;

        /// <summary>True while a battle is being fought, false during the walking and the rolling.</summary>
        public bool IsFighting
        {
            get { return phase == Phase.Fighting; }
        }

        /// <summary>Which wave is being fought. Equal to the wave count it means the villain.</summary>
        public int WaveIndex
        {
            get { return waveIndex; }
        }

        public StageData Stage
        {
            get { return stage; }
        }

        public void Configure(StageContext context)
        {
            this.context = context;

            // The game leaves this alone and gets Unity's deferred Destroy. A test outside Play
            // Mode has to pass DestroyImmediate, because the deferred one never runs there.
            destroy = context.Destroy ?? (target => Destroy(target));

            context.Director.BattleEnded += OnBattleEnded;
        }

        /// <summary>
        /// Reads the party for this stage and gives each of them a mover to walk back with.
        ///
        /// Done at the start of every stage rather than once, because who walks in can change
        /// between them: a stage has its own hero limit, a hero can have just been unlocked, and
        /// the player may have rearranged the team. Reading it here is what makes those changes
        /// land on the next stage and never in the middle of one.
        /// </summary>
        private void BuildParty()
        {
            heroes.Clear();
            regroupMovers.Clear();

            if (context.BuildParty == null)
            {
                return;
            }

            List<Character> party = context.BuildParty(stage);

            for (int i = 0; party != null && i < party.Count; i++)
            {
                if (party[i] == null)
                {
                    continue;
                }

                heroes.Add(party[i]);
                regroupMovers.Add(new CharacterMover(party[i], context.Grid));
            }
        }

        private void OnDestroy()
        {
            if (context != null && context.Director != null)
            {
                context.Director.BattleEnded -= OnBattleEnded;
            }
        }

        /// <summary>
        /// Starts a stage from the first wave, with a party built from scratch.
        ///
        /// The only way in, and that is the point: a stage is entered from the top whether it is the
        /// first attempt, the restart after a defeat, or the game being reopened. There is no
        /// halfway state to resume into, so there is none to write down or get wrong.
        /// </summary>
        public void StartStage(StageData stage)
        {
            this.stage = stage;

            Debug.Log("Stage '" + context.Strings.Get(StringTable.StageName(stage.id)) + "' started. "
                + context.Strings.Get(StringTable.StageLore(stage.id)), this);

            DespawnEnemies();
            DespawnAllies();

            // The party is built fresh, and whoever built it is responsible for taking the last
            // stage's combatants off the board first. Nothing is put back here because nothing
            // was carried over: a combatant that has never fought has no buff to clear, no taunt
            // to drop and no damage to undo.
            BuildParty();

            for (int i = 0; i < regroupMovers.Count; i++)
            {
                regroupMovers[i].Reset();
            }

            context.Scroller.ResetPosition();

            // After the heroes are back on their cells, so an NPC cannot claim one of theirs.
            SpawnAllies();

            waveIndex = 0;
            BeginWave();

            StageStarted?.Invoke();
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        /// <summary>
        /// Feeds real time into the stage, which only ever advances in steps of
        /// <see cref="BattleDirector.FixedStep"/>.
        ///
        /// Public and separate from Update on purpose: a test drives a whole stage by calling
        /// this with whatever deltas it likes, including deliberately irregular ones, and the
        /// result has to come out identical either way.
        /// </summary>
        public void Advance(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            accumulator += deltaTime;

            float limit = MaxStepsPerFrame * BattleDirector.FixedStep;
            if (accumulator > limit)
            {
                accumulator = limit;
            }

            while (accumulator >= BattleDirector.FixedStep)
            {
                accumulator -= BattleDirector.FixedStep;
                Step(BattleDirector.FixedStep);
            }
        }

        /// <summary>
        /// One simulation step of the stage. Either the battle is running, and the step goes to
        /// the director, or the stage is between waves and the step advances the transition.
        /// </summary>
        private void Step(float step)
        {
            Stepped?.Invoke(step);

            if (phase == Phase.Fighting)
            {
                // The battle can end inside this call, which changes the phase. The next step
                // of the loop then picks up the transition, with no time lost in between.
                context.Director.Tick(step);
                return;
            }

            timer += step;

            // Regeneration keeps running between waves. It is measured per second, and the walk
            // back plus the ground rolling is the better part of ten seconds, which is exactly
            // where an item that regenerates health is supposed to pay off. This does not undo
            // the attrition rule: nobody is topped up, they only recover what their own rate
            // earns them.
            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i].Regenerate(step);

                // Buffs keep counting down between waves. A buff is a length of time, not a
                // number of fights, and stopping the clock here would make a long buff worth more
                // the more transitions it survived.
                heroes[i].TickEffects(step);
            }

            if (phase == Phase.Regrouping)
            {
                TickRegroup(step);
                return;
            }

            if (phase == Phase.Advancing)
            {
                context.Scroller.SetProgress(timer / phaseDuration);
            }

            if (timer < phaseDuration)
            {
                return;
            }

            switch (phase)
            {
                case Phase.Advancing:
                    context.Scroller.ResetPosition();
                    BeginWave();
                    break;

                case Phase.Celebrating:
                case Phase.Restarting:
                    // Winning moves on, losing starts the same stage again. Which stage that is
                    // belongs to whoever owns the database, never to the runner.
                    StartStage(context.NextStage != null
                        ? context.NextStage(stage, lastAttemptCleared)
                        : stage);
                    break;
            }
        }

        /// <summary>
        /// The party walks back into formation. The ground only starts rolling once everybody
        /// has arrived, so the transition reads as the group regrouping and then moving on.
        /// </summary>
        private void TickRegroup(float step)
        {
            bool everyoneArrived = true;

            for (int i = 0; i < regroupMovers.Count; i++)
            {
                regroupMovers[i].TickWalkBack(step);

                if (regroupMovers[i].IsWalkingBack)
                {
                    everyoneArrived = false;
                }
            }

            // The time limit is a safety net. Someone boxed in by fallen allies could
            // otherwise keep the stage waiting forever.
            if (!everyoneArrived && timer < phaseDuration)
            {
                return;
            }

            // Fallen heroes are carried back, and anyone still short of their cell is placed
            // there, so the next wave always starts from a clean formation.
            ReturnHeroesToStart();
            EnterPhase(Phase.Advancing, AdvanceDuration);
        }

        private void BeginWave()
        {
            StageWave wave = IsVillainWave ? stage.villainWave : stage.waves[waveIndex];

            SpawnWave(wave);

            List<Character> everyone = new List<Character>(heroes);
            everyone.AddRange(allies);
            everyone.AddRange(enemies);

            phase = Phase.Fighting;
            context.Director.Begin(context.Grid, everyone, context.Random);
        }

        private bool IsVillainWave
        {
            get { return waveIndex >= stage.waves.Length; }
        }

        private void OnBattleEnded(bool heroesWon)
        {
            lastAttemptCleared = false;

            if (!heroesWon)
            {
                Debug.Log("The heroes fell on wave " + (waveIndex + 1) + " of '" + stage.id
                    + "'. Starting the stage over.", this);
                EnterPhase(Phase.Restarting, DefeatDuration);
                StageEnded?.Invoke(false);
                return;
            }

            bool wasVillain = IsVillainWave;

            DespawnEnemies();

            if (wasVillain)
            {
                lastAttemptCleared = true;

                ReturnHeroesToStart();
                Debug.Log("Stage '" + stage.id + "' cleared.", this);
                EnterPhase(Phase.Celebrating, CelebrationDuration);
                StageEnded?.Invoke(true);
                return;
            }

            waveIndex++;

            // The heroes walk back into formation instead of blinking there. The advance only
            // begins once they arrive, which is what gives the transition a real length.
            for (int i = 0; i < regroupMovers.Count; i++)
            {
                regroupMovers[i].BeginWalkBack(heroes[i].InitialPosition);
            }

            for (int i = 0; i < allyRegroupMovers.Count; i++)
            {
                allyRegroupMovers[i].BeginWalkBack(allies[i].InitialPosition);
            }

            EnterPhase(Phase.Regrouping, MaxRegroupDuration);

            // After the index moved, so whoever saves records the wave that comes next.
            WaveCleared?.Invoke();
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
                if (!context.CharactersById.TryGetValue(placement.character, out definition))
                {
                    // Validation already reported this when the stage was loaded.
                    continue;
                }

                GridPosition position = new GridPosition(placement.column, placement.row);

                if (!context.Grid.IsFree(position))
                {
                    Debug.LogWarning("Stage '" + stage.id + "': " + placement.character
                        + " cannot take cell " + position + " because it is occupied.", this);
                    continue;
                }

                Character enemy = context.Spawn(
                    definition, Team.Enemies, position,
                    placement.EffectiveLevel(stage.enemyLevel), placement.EffectiveMultiplier);

                enemy.StartWoundedAt(placement.EffectiveStartingHealthPercent);

                enemy.Died += OnEnemyDied;
                enemies.Add(enemy);
            }
        }

        /// <summary>
        /// Hands out the reward for an enemy that fell.
        ///
        /// Every hero of the group receives it, **including the fallen ones**. Without that rule
        /// a weak hero carried by a strong group would gain almost nothing, since it would go
        /// down early in every wave.
        /// </summary>
        private void OnEnemyDied(Character enemy)
        {
            bool isVillain = enemy.Kind == CharacterKind.Villain;
            int level = enemy.Level;

            long experience = isVillain
                ? ExperienceTable.XpFromVillain(level)
                : ExperienceTable.XpFromMinion(level);

            int money = isVillain
                ? ExperienceTable.MoneyFromVillain(level)
                : ExperienceTable.MoneyFromMinion(level);

            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i].AwardExperience(experience);
            }

            context.Wallet.Add(money);

            EnemyDefeated?.Invoke(enemy, experience, money);
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

                enemy.Died -= OnEnemyDied;
                enemy.ClearFromGrid();
                destroy(enemy.gameObject);
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

            for (int i = 0; i < allies.Count; i++)
            {
                allies[i].ClearFromGrid();
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i].ReturnToStart();
            }

            for (int i = 0; i < allies.Count; i++)
            {
                allies[i].ReturnToStart();
            }
        }

        /// <summary>
        /// Places the stage's story characters on the hero side, once, at the start.
        ///
        /// They stay for the whole stage and walk back between waves like the party does, because
        /// characters.md says they advance the waves together with the heroes.
        /// </summary>
        private void SpawnAllies()
        {
            if (stage.allies == null)
            {
                return;
            }

            for (int i = 0; i < stage.allies.Length; i++)
            {
                StagePlacement placement = stage.allies[i];

                CharacterDefinition definition;
                if (!context.CharactersById.TryGetValue(placement.character, out definition))
                {
                    // Validation already reported this when the stage was loaded.
                    continue;
                }

                GridPosition position = new GridPosition(placement.column, placement.row);

                if (!context.Grid.IsFree(position))
                {
                    Debug.LogWarning("Stage '" + stage.id + "': " + placement.character
                        + " cannot take cell " + position + " because it is occupied.", this);
                    continue;
                }

                Character ally = context.Spawn(
                    definition, Team.Heroes, position,
                    placement.EffectiveLevel(stage.enemyLevel), placement.EffectiveMultiplier);

                ally.StartWoundedAt(placement.EffectiveStartingHealthPercent);

                allies.Add(ally);
                allyRegroupMovers.Add(new CharacterMover(ally, context.Grid));
            }
        }

        private void DespawnAllies()
        {
            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i] != null)
                {
                    allies[i].ClearFromGrid();
                    destroy(allies[i].gameObject);
                }
            }

            allies.Clear();
            allyRegroupMovers.Clear();
        }
    }
}
