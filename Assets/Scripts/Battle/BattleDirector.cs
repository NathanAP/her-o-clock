using System;
using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Movement;
using UnityEngine;

namespace HerOClock.Battle
{
    /// <summary>
    /// The single update loop of the battle.
    ///
    /// Characters deliberately have no Update of their own. With one loop, in a fixed order,
    /// combat always plays out the same way for the same starting state, which is what will
    /// let us simulate offline progression later without rewriting anything.
    /// </summary>
    public class BattleDirector : MonoBehaviour
    {
        private readonly List<Character> heroes = new List<Character>();
        private readonly List<Character> enemies = new List<Character>();
        private readonly List<Character> all = new List<Character>();
        private readonly List<CharacterMover> movers = new List<CharacterMover>();
        private readonly List<CharacterAttacker> attackers = new List<CharacterAttacker>();

        private float restartDelay;
        private float restartTimer;
        private bool running;

        /// <summary>Attacker, target and outcome. The view layer listens to this.</summary>
        public event Action<Character, Character, DamageResult> Attacked;

        public IReadOnlyList<Character> Heroes
        {
            get { return heroes; }
        }

        public IReadOnlyList<Character> Enemies
        {
            get { return enemies; }
        }

        public void Begin(BattleGrid grid, IReadOnlyList<Character> characters, BattleRandom random, float restartDelay)
        {
            this.restartDelay = restartDelay;

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

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (!running)
            {
                restartTimer -= deltaTime;

                if (restartTimer <= 0f)
                {
                    Restart();
                }

                return;
            }

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
            restartTimer = restartDelay;

            string winner = heroesAlive ? "heroes" : enemiesAlive ? "enemies" : "nobody";
            Debug.Log("Battle over. Winner: " + winner + ". Restarting in " + restartDelay + "s.", this);
        }

        /// <summary>
        /// Puts everyone back on their starting cells at full health. While stages do not
        /// exist, this is what lets us watch the same battle several times to get a feel
        /// for the balance.
        /// </summary>
        private void Restart()
        {
            for (int i = 0; i < all.Count; i++)
            {
                movers[i].Reset();
                attackers[i].Reset();
                all[i].ClearFromGrid();
            }

            // Characters are only placed back after everyone has left the board. Otherwise a
            // character standing on someone else's starting cell would make both claim it.
            for (int i = 0; i < all.Count; i++)
            {
                all[i].ResetForBattle();
            }

            running = true;
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
