using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Movement;
using UnityEngine;

namespace HerOClock.Battle
{
    /// <summary>
    /// Unico laco de atualizacao do combate.
    ///
    /// Os personagens nao possuem Update proprio de proposito. Com um laco so, em
    /// ordem fixa, o combate roda sempre igual para o mesmo estado inicial, o que
    /// vai permitir simular a progressao offline mais para frente sem reescrever nada.
    /// </summary>
    public class BattleDirector : MonoBehaviour
    {
        private readonly List<Character> heroes = new List<Character>();
        private readonly List<Character> enemies = new List<Character>();
        private readonly List<Character> all = new List<Character>();
        private readonly List<CharacterMover> movers = new List<CharacterMover>();

        private bool running;

        public IReadOnlyList<Character> Heroes
        {
            get { return heroes; }
        }

        public IReadOnlyList<Character> Enemies
        {
            get { return enemies; }
        }

        public void Begin(BattleGrid grid, IReadOnlyList<Character> characters)
        {
            heroes.Clear();
            enemies.Clear();
            all.Clear();
            movers.Clear();

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];

                all.Add(character);
                movers.Add(new CharacterMover(character, grid));

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
            if (!running)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            for (int i = 0; i < all.Count; i++)
            {
                IReadOnlyList<Character> opponents = all[i].Team == Team.Heroes ? enemies : heroes;
                movers[i].Tick(deltaTime, opponents);
            }
        }
    }
}
