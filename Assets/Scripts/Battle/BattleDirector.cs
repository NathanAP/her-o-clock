using System;
using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Movement;
using UnityEngine;

namespace HerOClock.Battle
{
    /// <summary>
    /// The single update loop of one battle.
    ///
    /// Characters deliberately have no Update of their own. With one loop, in a fixed order and
    /// on a fixed step, the same starting state and the same seed always produce the same fight,
    /// blow by blow. That is what lets a reported oddity be replayed instead of guessed at.
    ///
    /// It does not read the clock. <see cref="Tick"/> is called by the StageRunner, which owns
    /// the only accumulator in the game, so a whole battle can also be run headless by a test
    /// simply by calling Tick in a loop.
    ///
    /// It runs a single fight and stops. What happens next belongs to the StageRunner.
    /// </summary>
    public class BattleDirector : MonoBehaviour
    {
        /// <summary>
        /// Size of one simulation step, in seconds.
        ///
        /// Everything in combat advances by exactly this amount, never by the frame's delta.
        /// A variable delta would make the fight depend on the frame rate of the machine, which
        /// breaks replaying a seed and makes the developer speed control distort the simulation.
        ///
        /// 60 steps per second is finer than the 30 frames the game renders, so the simulation
        /// is never coarser than what the player sees, and it divides cleanly into the frame
        /// rates that matter.
        /// </summary>
        public const float FixedStep = 1f / 60f;

        private readonly List<Character> heroes = new List<Character>();
        private readonly List<Character> enemies = new List<Character>();
        private readonly List<Character> all = new List<Character>();
        private readonly List<CharacterMover> movers = new List<CharacterMover>();
        private readonly List<CharacterAttacker> attackers = new List<CharacterAttacker>();
        private readonly List<AbilityCaster> casters = new List<AbilityCaster>();

        private bool running;
        private int heroCount;
        private long stepCount;

        /// <summary>Attacker, target and outcome. The view layer listens to this.</summary>
        public event Action<Character, Character, DamageResult> Attacked;

        /// <summary>
        /// Raised once per basic attack, naming who swung at whom, before the damage is resolved.
        ///
        /// Separate from <see cref="Attacked"/> on purpose: that one also carries the thorns coming
        /// back and every blow an ability lands, so it cannot answer "was this a basic attack".
        /// Drawing the swing needs that answer.
        /// </summary>
        public event Action<Character, Character> BasicAttackLanded;

        /// <summary>Raised once when a side runs out of living members. True means the heroes won.</summary>
        public event Action<bool> BattleEnded;

        public bool IsRunning
        {
            get { return running; }
        }

        public void Begin(BattleGrid grid, IReadOnlyList<Character> characters, BattleRandom random)
        {
            heroes.Clear();
            enemies.Clear();
            all.Clear();
            movers.Clear();
            attackers.Clear();
            casters.Clear();

            // Heroes are added first and enemies second, so each side occupies a contiguous
            // half of the list and the side that resolves first can be swapped by swapping the
            // order of two loops. The caller's ordering is deliberately not trusted for this.
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i].Team == Team.Heroes)
                {
                    Enlist(characters[i], grid, random);
                }
            }

            heroCount = all.Count;

            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i].Team != Team.Heroes)
                {
                    Enlist(characters[i], grid, random);
                }
            }

            // The step counter belongs to the battle, so two identical battles alternate the
            // resolution order identically.
            stepCount = 0;
            running = true;
        }

        private void Enlist(Character character, BattleGrid grid, BattleRandom random)
        {
            all.Add(character);
            movers.Add(new CharacterMover(character, grid));

            CharacterAttacker attacker = new CharacterAttacker(character, random);
            attacker.Attacked += RaiseAttacked;
            attacker.Struck += RaiseBasicAttack;
            attackers.Add(attacker);

            AbilityCaster caster = new AbilityCaster(character, grid, random);
            caster.Damaged += RaiseAttacked;
            casters.Add(caster);

            if (character.Team == Team.Heroes)
            {
                heroes.Add(character);
            }
            else
            {
                enemies.Add(character);
            }
        }

        public void Stop()
        {
            running = false;
        }

        /// <summary>
        /// Advances the battle by exactly one simulation step.
        ///
        /// Public and driven from outside on purpose: the StageRunner owns the clock, and a test
        /// can run an entire fight by calling this in a loop with no scene and no rendering.
        /// </summary>
        public void Tick(float step)
        {
            if (!running)
            {
                return;
            }

            // Which side resolves first alternates every step. Heroes are always first in the
            // list, so without this they would win every exact tie: their blow would land and
            // kill before the enemy whose own blow was due in the same step ever swung. The
            // alternation is driven by the step counter, so it stays reproducible.
            bool heroesFirst = (stepCount & 1L) == 0L;
            stepCount++;

            // Regeneration is not an action against an opponent, so it is applied to everyone in
            // one pass and is deliberately left out of the alternation above. Buffs, debuffs and
            // named states count down here for the same reason: they are not somebody's turn.
            for (int i = 0; i < all.Count; i++)
            {
                all[i].Regenerate(step);
                all[i].TickEffects(step);
            }

            if (heroesFirst)
            {
                TickRange(0, heroCount, step);
                TickRange(heroCount, all.Count, step);
            }
            else
            {
                TickRange(heroCount, all.Count, step);
                TickRange(0, heroCount, step);
            }

            CheckForEnd();
        }

        /// <summary>
        /// One side's turn inside a step. The order within a character is what the spec's priority
        /// rules turn into:
        ///
        /// 1. The caster advances, so being busy with an ability is already true for this step.
        /// 2. While busy, the character neither moves nor attacks — the three ability phases are
        ///    one occupied block.
        /// 3. The attacker runs **before** a new ability can start, which is what "the basic attack
        ///    takes priority" means in practice. Its timer keeps running while busy and is held at
        ///    zero rather than banking blows, so it lands in the first gap.
        /// </summary>
        private void TickRange(int from, int to, float step)
        {
            for (int i = from; i < to; i++)
            {
                IReadOnlyList<Character> opponents = all[i].Team == Team.Heroes ? enemies : heroes;
                IReadOnlyList<Character> friends = all[i].Team == Team.Heroes ? heroes : enemies;

                casters[i].Tick(step, friends, opponents);

                bool busy = casters[i].IsBusy;

                if (!busy)
                {
                    movers[i].Tick(step, opponents);
                }

                attackers[i].Tick(step, opponents, busy || movers[i].IsMoving);

                if (!busy && !movers[i].IsMoving)
                {
                    casters[i].TryStart(friends, opponents);
                }
            }
        }

        private void CheckForEnd()
        {
            bool heroesAlive = AnyAlive(heroes);
            bool enemiesAlive = AnyAlive(enemies);

            if (heroesAlive && enemiesAlive)
            {
                return;
            }

            running = false;
            BattleEnded?.Invoke(heroesAlive);
        }

        private void RaiseAttacked(Character attacker, Character target, DamageResult result)
        {
            Attacked?.Invoke(attacker, target, result);
        }

        private void RaiseBasicAttack(Character attacker, Character target)
        {
            BasicAttackLanded?.Invoke(attacker, target);
        }

        private static bool AnyAlive(List<Character> characters)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i].IsAlive)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
