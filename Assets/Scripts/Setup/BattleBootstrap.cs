using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.View;
using UnityEngine;

namespace HerOClock.Setup
{
    /// <summary>
    /// Builds the whole battle at runtime from the configuration assets.
    ///
    /// The scene holds only this component. Everything else is created in code on purpose, so
    /// the .unity file stays small and almost never causes a git conflict.
    /// </summary>
    public class BattleBootstrap : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BattleGridConfig gridConfig;

        [Header("Formations")]
        [SerializeField] private BattleFormation heroFormation;
        [SerializeField] private BattleFormation enemyFormation;

        [Header("Placeholder look")]
        [SerializeField] private CharacterViewSettings viewSettings = new CharacterViewSettings();

        [Header("Combat")]
        [Tooltip("Random seed. The same value always produces the same battle. Leave 0 to draw a new seed on every Play.")]
        [SerializeField] private int randomSeed;

        [Tooltip("Seconds to wait before restarting the battle after one side is defeated.")]
        [Min(0f)]
        [SerializeField] private float restartDelay = 2f;

        [Header("Performance")]
        [Tooltip("The game sits open all day in a corner of the screen, so there is no point burning GPU.")]
        [Min(10)]
        [SerializeField] private int targetFrameRate = 30;

        private BattleGrid grid;
        private BattleDirector director;
        private readonly Dictionary<Character, CharacterView> views = new Dictionary<Character, CharacterView>();

        private void Start()
        {
            Application.targetFrameRate = targetFrameRate;

            if (gridConfig == null)
            {
                Debug.LogError("BattleBootstrap: the Grid Config reference is missing.", this);
                return;
            }

            grid = new BattleGrid(gridConfig);
            BoardRenderer.Build(grid, transform);

            List<Character> characters = new List<Character>();
            SpawnFormation(heroFormation, characters);
            SpawnFormation(enemyFormation, characters);

            if (characters.Count == 0)
            {
                Debug.LogWarning("BattleBootstrap: no character was created. Check the formations.", this);
                return;
            }

            if (!IsBattleValid(characters))
            {
                return;
            }

            int seed = randomSeed != 0 ? randomSeed : System.Environment.TickCount;
            Debug.Log("BattleBootstrap: random seed " + seed + ". Put this value in the Random Seed field to replay this exact battle.", this);

            director = gameObject.AddComponent<BattleDirector>();
            director.Attacked += OnAttacked;
            director.Begin(grid, characters, new BattleRandom(seed), restartDelay);
        }

        /// <summary>
        /// Wires combat to the view layer: whoever took the blow flashes and the number rises.
        /// </summary>
        private void OnAttacked(Character attacker, Character target, DamageResult result)
        {
            CharacterView view;
            if (!views.TryGetValue(target, out view))
            {
                return;
            }

            if (result.Damage > 0)
            {
                view.Flash();
            }

            Vector3 origin = target.transform.position + new Vector3(0f, gridConfig.CellSize * 0.5f, 0f);
            DamageNumber.Spawn(transform, origin, result, gridConfig.CellSize);
        }

        /// <summary>
        /// Checks whether the battle can actually happen.
        ///
        /// Without this the two most common configuration mistakes produce no message at all:
        /// a side with nobody on it leaves everyone standing still for lack of an enemy, and a
        /// character with 0 maximum health is born dead and also never acts.
        /// </summary>
        private bool IsBattleValid(List<Character> characters)
        {
            int heroCount = 0;
            int enemyCount = 0;
            bool valid = true;

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];

                if (character.Team == Team.Heroes)
                {
                    heroCount++;
                }
                else
                {
                    enemyCount++;
                }

                if (character.Stats.MaxHealth <= 0)
                {
                    Debug.LogError("BattleBootstrap: " + character.Definition.DisplayName
                        + " has 0 maximum health, so it is born dead and never acts. "
                        + "Maximum health is Power x 5 plus Constitution x 10, and both are zero on the sheet.",
                        character.Definition);
                    valid = false;
                }
            }

            if (heroCount == 0 || enemyCount == 0)
            {
                Debug.LogError("BattleBootstrap: the battle has " + heroCount + " hero(es) and " + enemyCount
                    + " enemy(ies). Both sides need someone, otherwise nobody has a target and nobody moves. "
                    + "Check the Team field on both formations: one must be Heroes and the other Enemies.", this);
                valid = false;
            }

            if (valid)
            {
                Debug.Log("BattleBootstrap: battle started with " + heroCount + " hero(es) against " + enemyCount + " enemy(ies).", this);
            }

            return valid;
        }

        private void SpawnFormation(BattleFormation formation, List<Character> output)
        {
            if (formation == null)
            {
                Debug.LogWarning("BattleBootstrap: one of the formation references is missing.", this);
                return;
            }

            for (int i = 0; i < formation.Placements.Count; i++)
            {
                BattleFormation.Placement placement = formation.Placements[i];

                if (placement.Character == null)
                {
                    Debug.LogWarning("Formation " + formation.name + ": entry " + i + " has no character.", formation);
                    continue;
                }

                GridPosition position = new GridPosition(placement.Column, placement.Row);

                if (!grid.IsInside(position))
                {
                    Debug.LogWarning("Formation " + formation.name + ": " + placement.Character.DisplayName
                        + " sits on cell " + position + ", which is outside the board.", formation);
                    continue;
                }

                if (!grid.IsFree(position))
                {
                    Debug.LogWarning("Formation " + formation.name + ": " + placement.Character.DisplayName
                        + " wants cell " + position + ", which is already taken.", formation);
                    continue;
                }

                output.Add(CreateCharacter(placement.Character, formation.Team, position));
            }
        }

        private Character CreateCharacter(CharacterDefinition definition, Team team, GridPosition position)
        {
            GameObject instance = new GameObject(definition.DisplayName);
            instance.transform.SetParent(transform, false);

            Character character = instance.AddComponent<Character>();
            character.Initialize(definition, team, position, grid);

            // The body and the health bar are children of the character, so the scaling lives
            // on them and not on the object that travels across the board.
            CharacterView view = instance.AddComponent<CharacterView>();
            view.Build(character, viewSettings, definition.Color, gridConfig.CellSize);
            views.Add(character, view);

            return character;
        }
    }
}
