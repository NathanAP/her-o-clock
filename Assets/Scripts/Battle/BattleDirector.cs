using System;
using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Movement;
using UnityEngine;

namespace HerOClock.Battle
{
    /// <summary>
    /// The single update loop of one battle.
    ///
    /// Characters deliberately have no Update of their own. With one loop, in a fixed order,
    /// combat always plays out the same way for the same starting state, which is what will
    /// let us simulate offline progression later without rewriting anything.
    ///
    /// It runs a single fight and stops. What happens next belongs to the StageRunner.
    /// </summary>
    public class BattleDirector : MonoBehaviour
    {
        private readonly List<Character> heroes = new List<Character>();
        private readonly List<Character> enemies = new List<Character>();
        private readonly List<Character> all = new List<Character>();
        private readonly List<CharacterMover> movers = new List<CharacterMover>();
        private readonly List<CharacterAttacker> attackers = new List<CharacterAttacker>();

        private bool running;

        /// <summary>Attacker, target and outcome. The view layer listens to this.</summary>
        public event Action<Character, Character, DamageResult> Attacked;

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

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];

                all.Add(character);
                movers.Add(new CharacterMover(character, grid));

                CharacterAttacker attacker = new CharacterAttacker(character, random);
                attacker.Attacked += RaiseAttacked;
                attackers.Add(attacker);

                if (character.Team == Team.Heroes)
                {
                    heroes.Add(character);
                }
                else
                {
                    enemies.Add(character);
                }
            }

            running = true;
        }

        public void Stop()
        {
            running = false;
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            for (int i = 0; i < all.Count; i++)
            {
                IReadOnlyList<Character> opponents = all[i].Team == Team.Heroes ? enemies : heroes;

                movers[i].Tick(deltaTime, opponents);
                attackers[i].Tick(deltaTime, opponents, movers[i].IsMoving);
            }

            CheckForEnd();
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
