using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Stages;
using HerOClock.View;
using UnityEngine;

namespace HerOClock.Setup
{
    /// <summary>
    /// Builds everything at runtime from the configuration assets and starts a stage.
    ///
    /// The scene holds only this component. Everything else is created in code on purpose, so
    /// the .unity file stays small and almost never causes a git conflict.
    /// </summary>
    public class BattleBootstrap : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BattleGridConfig gridConfig;
        [SerializeField] private CharacterDatabase characterDatabase;
        [SerializeField] private StageDatabase stageDatabase;

        [Header("Stage")]
        [Tooltip("Which stage of the database to play. Stage selection does not exist yet.")]
        [Min(0)]
        [SerializeField] private int stageIndex;

        [Tooltip("The player's team and where it starts on the board.")]
        [SerializeField] private BattleFormation heroFormation;

        [Header("Placeholder look")]
        [SerializeField] private CharacterViewSettings viewSettings = new CharacterViewSettings();

        [Header("Combat")]
        [Tooltip("Random seed. The same value always produces the same battle. Leave 0 to draw a new seed on every Play.")]
        [SerializeField] private int randomSeed;

        [Header("Performance")]
        [Tooltip("The game sits open all day in a corner of the screen, so there is no point burning GPU.")]
        [Min(10)]
        [SerializeField] private int targetFrameRate = 30;

        private BattleGrid grid;
        private BattleDirector director;

        private void Start()
        {
            Application.targetFrameRate = targetFrameRate;

            if (!HasRequiredAssets())
            {
                return;
            }

            // Built once, here, and passed around. The database asset keeps no runtime state,
            // because a ScriptableObject survives between Play sessions and a stale cache would
            // survive with it.
            Dictionary<string, CharacterDefinition> charactersById = characterDatabase.BuildIndex();
            Debug.Log("BattleBootstrap: " + charactersById.Count + " character(s) loaded: "
                + string.Join(", ", charactersById.Keys) + ".", this);

            StageData stage = stageDatabase.Load(stageIndex);
            if (stage == null || !IsStageValid(stage, charactersById))
            {
                return;
            }

            grid = new BattleGrid(gridConfig);
            Transform board = BoardRenderer.Build(grid, transform);

            List<Character> heroes = SpawnHeroes();
            if (heroes.Count == 0)
            {
                Debug.LogError("BattleBootstrap: no hero was created, so the stage cannot be played. Check the hero formation.", this);
                return;
            }

            int seed = randomSeed != 0 ? randomSeed : System.Environment.TickCount;
            Debug.Log("BattleBootstrap: random seed " + seed + ". Put this value in the Random Seed field to replay this exact run.", this);

            BattleRandom random = new BattleRandom(seed);

            director = gameObject.AddComponent<BattleDirector>();
            director.Attacked += OnAttacked;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Never present in a released build. A speed slider would defeat a game whose
            // whole point is that time passes.
            gameObject.AddComponent<HerOClock.Dev.DevSpeedControl>();
#endif

            StageRunner runner = gameObject.AddComponent<StageRunner>();
            runner.Configure(grid, director, charactersById, random, new BoardScroller(board, gridConfig.CellSize), heroes, CreateCharacter);
            runner.StartStage(stage);
        }

        private bool HasRequiredAssets()
        {
            bool ok = true;

            if (gridConfig == null)
            {
                Debug.LogError("BattleBootstrap: the Grid Config reference is missing.", this);
                ok = false;
            }

            if (characterDatabase == null)
            {
                Debug.LogError("BattleBootstrap: the Character Database reference is missing.", this);
                ok = false;
            }

            if (stageDatabase == null)
            {
                Debug.LogError("BattleBootstrap: the Stage Database reference is missing.", this);
                ok = false;
            }

            if (heroFormation == null)
            {
                Debug.LogError("BattleBootstrap: the Hero Formation reference is missing.", this);
                ok = false;
            }

            return ok;
        }

        /// <summary>
        /// Reports every problem in the stage file at once, so a typo shows up as a readable
        /// message instead of an empty battlefield.
        /// </summary>
        private bool IsStageValid(StageData stage, IReadOnlyDictionary<string, CharacterDefinition> charactersById)
        {
            List<string> problems = StageValidator.Validate(
                stage, gridConfig.Columns, gridConfig.Rows, CharacterDatabase.KindsOf(charactersById));

            if (problems.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < problems.Count; i++)
            {
                Debug.LogError("Stage '" + stage.id + "': " + problems[i], this);
            }

            return false;
        }

        /// <summary>
        /// Wires combat to the view layer: whoever took the blow flashes and the number rises.
        /// </summary>
        private void OnAttacked(Character attacker, Character target, DamageResult result)
        {
            CharacterView view = target.GetComponent<CharacterView>();

            if (view != null && result.Damage > 0)
            {
                view.Flash();
            }

            Vector3 origin = target.transform.position + new Vector3(0f, gridConfig.CellSize * 0.5f, 0f);
            DamageNumber.Spawn(transform, origin, result, gridConfig.CellSize);
        }

        private List<Character> SpawnHeroes()
        {
            List<Character> heroes = new List<Character>();

            for (int i = 0; i < heroFormation.Placements.Count; i++)
            {
                BattleFormation.Placement placement = heroFormation.Placements[i];

                if (placement.Character == null)
                {
                    Debug.LogWarning("Formation " + heroFormation.name + ": entry " + i + " has no character.", heroFormation);
                    continue;
                }

                GridPosition position = new GridPosition(placement.Column, placement.Row);

                if (!grid.IsInside(position))
                {
                    Debug.LogWarning("Formation " + heroFormation.name + ": " + placement.Character.DisplayName
                        + " sits on cell " + position + ", which is outside the board.", heroFormation);
                    continue;
                }

                if (!grid.IsFree(position))
                {
                    Debug.LogWarning("Formation " + heroFormation.name + ": " + placement.Character.DisplayName
                        + " wants cell " + position + ", which is already taken.", heroFormation);
                    continue;
                }

                if (placement.Character.Stats.MaxHealth <= 0)
                {
                    Debug.LogError("BattleBootstrap: " + placement.Character.DisplayName
                        + " has 0 maximum health, so it is born dead and never acts. "
                        + "Maximum health is Power x 5 plus Constitution x 10, and both are zero on the sheet.",
                        placement.Character);
                    continue;
                }

                heroes.Add(CreateCharacter(placement.Character, Team.Heroes, position, placement.Character.Level));
            }

            return heroes;
        }

        private Character CreateCharacter(CharacterDefinition definition, Team team, GridPosition position, int level)
        {
            GameObject instance = new GameObject(definition.DisplayName);
            instance.transform.SetParent(transform, false);

            Character character = instance.AddComponent<Character>();
            character.Initialize(definition, team, position, grid, level);

            // The body and the health bar are children of the character, so the scaling lives
            // on them and not on the object that travels across the board.
            CharacterView view = instance.AddComponent<CharacterView>();
            view.Build(character, viewSettings, definition.Color, gridConfig.CellSize);

            return character;
        }
    }
}
