using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Progression;
using HerOClock.Stages;
using HerOClock.Text;
using HerOClock.View;
using HerOClock.Window;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

        [Tooltip("The .json file holding every piece of text the player reads.")]
        [SerializeField] private TextAsset stringsFile;

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
        private PlayerWallet wallet;
        private DamageNumberPool damageNumbers;

        private void Start()
        {
            Application.targetFrameRate = targetFrameRate;
            SetUpWindow();

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

            StringTable strings = BuildStrings(charactersById);
            if (strings == null)
            {
                return;
            }

            StageData stage = stageDatabase.Load(stageIndex);
            if (stage == null || !IsStageValid(stage, charactersById))
            {
                return;
            }

            grid = new BattleGrid(gridConfig);
            Transform board = BoardRenderer.Build(grid, transform);
            damageNumbers = new DamageNumberPool(transform, gridConfig.CellSize);

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

            wallet = new PlayerWallet();
            wallet.Changed += () => Debug.Log("Money: " + wallet.Money + ".", this);

            StageRunner runner = gameObject.AddComponent<StageRunner>();
            runner.Configure(grid, director, charactersById, wallet, strings, random, new BoardScroller(board, gridConfig.CellSize), heroes, CreateCharacter);
            runner.StartStage(stage);
        }

        /// <summary>
        /// Puts the window on one of the allowed sizes.
        ///
        /// The reference resolution is read from the Pixel Perfect Camera rather than written
        /// here, because those numbers are already tied to the pixels per unit and the board
        /// width. Repeating them would give the pair a second chance to disagree.
        ///
        /// It lives in the bootstrap because the bootstrap is the only entry point the game has.
        /// When there is a real application start, ahead of any battle, this belongs there.
        /// </summary>
        private void SetUpWindow()
        {
            Camera camera = Camera.main;
            PixelPerfectCamera pixelPerfect = camera != null ? camera.GetComponent<PixelPerfectCamera>() : null;

            if (pixelPerfect == null)
            {
                Debug.LogWarning("BattleBootstrap: no Pixel Perfect Camera found, so the window size is left alone.", this);
                return;
            }

            WindowScale scale = gameObject.AddComponent<WindowScale>();
            scale.Configure(pixelPerfect.refResolutionX, pixelPerfect.refResolutionY);

            // Takes the frame off, keeps the window above everything, and lets it be dragged by
            // the game itself. It disables itself outside a Windows build, so the editor is never
            // touched: the handle it would find there is the editor's own window.
            gameObject.AddComponent<WindowDrag>();
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

            if (stringsFile == null)
            {
                Debug.LogError("BattleBootstrap: the Strings File reference is missing.", this);
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
        /// Reads the strings file and checks that everything the game will ask for is in it.
        ///
        /// A missing key is the same class of problem as a typo in a stage file: nothing crashes,
        /// the game simply shows the wrong thing. Checking at startup turns it into a readable
        /// message, and returns null so the run stops rather than carrying on with broken text.
        /// </summary>
        private StringTable BuildStrings(IReadOnlyDictionary<string, CharacterDefinition> charactersById)
        {
            List<string> problems = new List<string>();

            StringTableData data;

            try
            {
                data = JsonUtility.FromJson<StringTableData>(stringsFile.text);
            }
            catch (System.Exception exception)
            {
                Debug.LogError("BattleBootstrap: " + stringsFile.name + " is not valid JSON. " + exception.Message, this);
                return null;
            }

            StringTable strings = StringTable.From(data, problems);

            StringTableValidator.Validate(strings, charactersById.Keys, StageIds(), problems);

            if (problems.Count == 0)
            {
                Debug.Log("BattleBootstrap: " + strings.Count + " string(s) loaded for '" + strings.Language + "'.", this);
                return strings;
            }

            for (int i = 0; i < problems.Count; i++)
            {
                Debug.LogError("Strings: " + problems[i], this);
            }

            return null;
        }

        /// <summary>Ids of every stage in the database, for checking their text exists.</summary>
        private List<string> StageIds()
        {
            List<string> ids = new List<string>();

            for (int i = 0; i < stageDatabase.Stages.Count; i++)
            {
                StageData stage = stageDatabase.Load(i);

                if (stage != null && !string.IsNullOrWhiteSpace(stage.id))
                {
                    ids.Add(stage.id);
                }
            }

            return ids;
        }

        /// <summary>
        /// Reports every problem in the stage file at once, so a typo shows up as a readable
        /// message instead of an empty battlefield.
        /// </summary>
        private bool IsStageValid(StageData stage, IReadOnlyDictionary<string, CharacterDefinition> charactersById)
        {
            List<string> problems = StageValidator.Validate(
                stage, gridConfig.Columns, gridConfig.Rows, gridConfig.HeroRows,
                CharacterDatabase.KindsOf(charactersById));

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
            damageNumbers.Show(origin, result);
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
                    Debug.LogWarning("Formation " + heroFormation.name + ": " + placement.Character.Id
                        + " sits on cell " + position + ", which is outside the board.", heroFormation);
                    continue;
                }

                if (!grid.IsFree(position))
                {
                    Debug.LogWarning("Formation " + heroFormation.name + ": " + placement.Character.Id
                        + " wants cell " + position + ", which is already taken.", heroFormation);
                    continue;
                }

                Character hero = CreateCharacter(placement.Character, Team.Heroes, position, placement.Character.Level, 1f);

                // Checked after the fact, on the instance, rather than by reading the sheet.
                // Deriving anything from the shared definition is what architecture.md forbids,
                // and the old check got it wrong anyway: it assumed level 1 and so misjudged any
                // hero whose sheet starts higher. A character born with no health never acts,
                // and Unity reports nothing about it.
                if (!hero.IsAlive)
                {
                    Debug.LogError("BattleBootstrap: " + placement.Character.Id
                        + " has 0 maximum health at level " + hero.Level
                        + ", so it is born dead and never acts. "
                        + "Maximum health is Power x 5 plus Constitution x 10.",
                        placement.Character);

                    hero.ClearFromGrid();
                    Destroy(hero.gameObject);
                    continue;
                }

                heroes.Add(hero);
            }

            return heroes;
        }

        private Character CreateCharacter(CharacterDefinition definition, Team team, GridPosition position, int level, float multiplier)
        {
            // Named by id: the Hierarchy is a developer tool, and the id is stable and not translated.
            GameObject instance = new GameObject(definition.Id);
            instance.transform.SetParent(transform, false);

            Character character = instance.AddComponent<Character>();
            character.Initialize(definition, team, position, grid, level, multiplier);

            // The body and the health bar are children of the character, so the scaling lives
            // on them and not on the object that travels across the board.
            CharacterView view = instance.AddComponent<CharacterView>();
            view.Build(character, viewSettings, definition.Color, gridConfig.CellSize);

            return character;
        }
    }
}
