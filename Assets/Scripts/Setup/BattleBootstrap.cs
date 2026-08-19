using System;
using System.Collections.Generic;
using System.Globalization;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Persistence;
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
        [Tooltip("Which stage to play when there is no save. A save says where the player was, and overrides this.")]
        [Min(0)]
        [SerializeField] private int stageIndex;

        [Tooltip("The player's team and where it starts on the board.")]
        [SerializeField] private BattleFormation heroFormation;

        [Header("Placeholder look")]
        [SerializeField] private CharacterViewSettings viewSettings = new CharacterViewSettings();

        [Tooltip("The bolt drawn for a ranged basic attack. Decoration only: the damage has already "
            + "been applied by the time it leaves.")]
        [SerializeField] private ProjectileSettings projectileSettings = new ProjectileSettings();

        [Header("Combat")]
        [Tooltip("Random seed. The same value always produces the same battle. Leave 0 to draw a new seed on every Play.")]
        [SerializeField] private int randomSeed;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Header("Development")]
        [Tooltip("Starts from the Stage Index above instead of from the save. Never present in a released build.")]
        [SerializeField] private bool ignoreSave;
#endif

        [Header("Performance")]
        [Tooltip("The game sits open all day in a corner of the screen, so there is no point burning GPU.")]
        [Min(10)]
        [SerializeField] private int targetFrameRate = 30;

        private BattleGrid grid;
        private BattleDirector director;
        private PlayerWallet wallet;
        private ActivityLog activity;
        private DamageNumberPool damageNumbers;
        private ProjectilePool projectiles;
        private Roster roster;
        private readonly List<string> clearedStages = new List<string>();

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

            // Read before anything else is decided, because the save is what says which stage the
            // player is on. Nothing readable in the folder is somebody's first game, not a failure.
            SaveStore store = new SaveStore(SaveStore.DefaultFolder());
            SaveReadResult save = IgnoringSave() ? new SaveReadResult() : store.Load();

            int index = stageIndex;

            if (save.Found)
            {
                Debug.Log("BattleBootstrap: save '" + save.FileName + "' loaded.", this);
                index = StageFor(save.Payload, index);
            }

            StageData stage = stageDatabase.Load(index);
            if (stage == null || !IsStageValid(stage, charactersById))
            {
                return;
            }

            grid = new BattleGrid(gridConfig);
            Transform board = BoardRenderer.Build(grid, transform);
            damageNumbers = new DamageNumberPool(transform, gridConfig.CellSize);
            projectiles = new ProjectilePool(transform, gridConfig.CellSize, projectileSettings);

            // The roster says who exists and who is fielded; the stage says how many of them it
            // takes. A brand new game owns only the first hero of the formation, with one
            // position, and everything else is unlocked by clearing stages.
            roster = new Roster();
            roster.Unlock(FirstFormationHeroId());

            if (save.Found)
            {
                SaveMapper.ApplyRoster(save.Payload, roster, clearedStages);
            }

            List<Character> heroes = SpawnHeroes(roster.PartyFor(stage.heroLimit));
            if (heroes.Count == 0)
            {
                Debug.LogError("BattleBootstrap: no hero was created, so the stage cannot be played. Check the hero formation.", this);
                return;
            }

            wallet = new PlayerWallet();
            wallet.Changed += () => Debug.Log("Money: " + wallet.Money + ".", this);

            activity = new ActivityLog();

            string integrity = SavePayload.IntegrityOk;

            if (save.Found)
            {
                SaveMapper.ApplyHeroes(save.Payload, heroes);
                SaveMapper.ApplyActivity(save.Payload, activity);
                wallet.Restore(save.Payload.money);
                integrity = save.Payload.integrity;

                CreditTimeAway(save.Payload, heroes);
            }

            int seed = randomSeed != 0 ? randomSeed : Environment.TickCount;
            Debug.Log("BattleBootstrap: random seed " + seed + ". Put this value in the Random Seed field to replay this exact run.", this);

            BattleRandom random = new BattleRandom(seed);

            director = gameObject.AddComponent<BattleDirector>();
            director.Attacked += OnAttacked;
            director.BasicAttackLanded += OnBasicAttackLanded;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Never present in a released build. A speed slider would defeat a game whose
            // whole point is that time passes.
            gameObject.AddComponent<HerOClock.Dev.DevSpeedControl>();
#endif

            StageRunner runner = gameObject.AddComponent<StageRunner>();

            runner.Configure(new StageContext
            {
                Grid = grid,
                Director = director,
                CharactersById = charactersById,
                Wallet = wallet,
                Strings = strings,
                Random = random,
                Scroller = new BoardScroller(board, gridConfig.CellSize),
                Spawn = CreateCharacter
            }, heroes);

            // The buckets measure simulation time, not the wall clock. A game the operating system
            // stopped drawing has earned nothing during that stretch, and it is the fighting the
            // rate is meant to describe.
            runner.Stepped += activity.Advance;
            runner.EnemyDefeated += OnEnemyDefeated;
            runner.StageEnded += cleared => GrantFirstClear(stage, cleared);

            // Always from the top. A save says which stage the player is on and never where inside
            // it, so being loaded is the same entry a defeat uses.
            runner.StartStage(stage);

            // Subscribed only now, so the write below is the first one and already carries the
            // loaded state and whatever the absence was worth.
            SaveService saves = gameObject.AddComponent<SaveService>();
            saves.Configure(store, runner, heroes, wallet, activity, integrity, roster, clearedStages);

            // **This write is what consumes the absence.** The time away is measured from the
            // instant in the file that was loaded, so crediting it and then falling over before
            // writing would let the next launch read the same instant and pay for it again.
            saves.Save();
        }

        /// <summary>
        /// Whether the existing progress should be left unread.
        ///
        /// Without this, the Stage Index above stops doing anything the moment a save exists, which
        /// is correct for a player and infuriating for whoever is building a stage. The game still
        /// writes saves while it is on, so the run is a real one and only the reading is skipped.
        /// </summary>
        private bool IgnoringSave()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (ignoreSave)
            {
                Debug.LogWarning("BattleBootstrap: Ignore Save is on, so the existing progress was not"
                    + " read. It is still on disk, and this run will save alongside it.", this);
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Which stage the save says the player is on.
        ///
        /// Found by id, so a stage inserted into the middle of an act does not move anyone. A stage
        /// that no longer exists sends the player back to the first one and keeps everything else,
        /// which happens while the game is being built and is not their fault.
        /// </summary>
        private int StageFor(SavePayload payload, int fallback)
        {
            if (payload.stage == null || string.IsNullOrEmpty(payload.stage.id))
            {
                return fallback;
            }

            int found = stageDatabase.IndexOf(payload.stage.id);

            if (found < 0)
            {
                Debug.LogWarning("Save: the stage '" + payload.stage.id + "' no longer exists, so the"
                    + " game starts from the first one. Nothing else was lost.", this);
                return 0;
            }

            return found;
        }

        /// <summary>
        /// Pays the player for the time the game was closed.
        ///
        /// It is arithmetic and never a simulation: the rate of the last hour multiplied by the time
        /// away, then the ceilings. See "A progressão offline é um cálculo, nunca uma simulação" in
        /// progress.md, and note that the credit deliberately never goes back into the buckets.
        /// </summary>
        private void CreditTimeAway(SavePayload payload, IReadOnlyList<Character> heroes)
        {
            DateTime savedAt;

            if (!DateTime.TryParse(payload.savedAtUtc, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out savedAt))
            {
                Debug.LogWarning("Save: '" + payload.savedAtUtc + "' is not a readable instant, so no"
                    + " time away was credited.", this);
                return;
            }

            double hours = OfflineProgress.ElapsedHours(savedAt.ToUniversalTime(), DateTime.UtcNow);
            OfflineCredit credit = OfflineProgress.Credit(activity, hours, OfflineProgress.DefaultRateShare);

            if (credit.EffectiveHours <= 0.0)
            {
                return;
            }

            wallet.Add(credit.Money);

            for (int i = 0; i < heroes.Count; i++)
            {
                Character hero = heroes[i];

                hero.AwardExperience(OfflineProgress.ExperienceFor(
                    credit.ExperienceOffered,
                    hero.Progress.Level,
                    hero.Progress.CurrentXp,
                    hero.Definition.MaxLevel));
            }

            Debug.Log("BattleBootstrap: away for " + hours.ToString("F1", CultureInfo.InvariantCulture)
                + " h, paid as " + credit.EffectiveHours.ToString("F2", CultureInfo.InvariantCulture)
                + " h of open game: " + credit.Money + " money, " + credit.ExperienceOffered
                + " experience offered per hero, " + credit.EnemiesDefeated + " enemies.", this);
        }

        /// <summary>
        /// Records a defeated enemy in the buckets of the last hour.
        ///
        /// The experience recorded is **one hero's**, not the party's total. The offline credit is
        /// applied per hero, so the rate has to describe what a single hero earns, or a bigger party
        /// would be paid several times over for being away.
        /// </summary>
        private void OnEnemyDefeated(Character enemy, long experience, long money)
        {
            activity.RecordEnemyDefeated();
            activity.RecordExperience(experience);
            activity.RecordMoney(money);
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

            RecordBlow(attacker, target, result);
        }

        /// <summary>
        /// Draws the bolt of a ranged basic attack, from whoever swung to where the target stood.
        ///
        /// Only the basic attack reaches here, which is the whole reason `BasicAttackLanded` exists
        /// apart from `Attacked`: that one also carries the thorns coming back and every blow an
        /// ability lands, and neither of those is a shot anybody fired.
        ///
        /// The destination is read now and handed over as a plain position. The bolt must not hold
        /// on to the target, which can die and be deactivated while the bolt is still travelling.
        /// </summary>
        private void OnBasicAttackLanded(Character attacker, Character target)
        {
            if (attacker.AutoAttack != AutoAttackType.Ranged)
            {
                return;
            }

            Color color = attacker.Definition != null ? attacker.Definition.Color : Color.white;

            projectiles.Fire(attacker.transform.position, target.transform.position, color);
        }

        /// <summary>
        /// Feeds one blow into the buckets of the last hour, from the party's point of view.
        ///
        /// Damage and healing are recorded here rather than counted somewhere else because these are
        /// the same numbers the player is shown, and progress.md forbids the statistics of an absence
        /// from having a second source. Thorns count as damage the party dealt, since the party is
        /// what caused them.
        /// </summary>
        private void RecordBlow(Character attacker, Character target, DamageResult result)
        {
            if (attacker.Team == Team.Heroes)
            {
                activity.RecordDamageDealt(result.Damage);
                activity.RecordHealing(result.LifeStolen);
            }

            if (target.Team == Team.Heroes)
            {
                activity.RecordDamageTaken(result.Damage);
                activity.RecordDamageDealt(result.Thorns);
                activity.RecordHealing(result.Healing);
            }
        }

        /// <summary>
        /// Puts on the board the heroes the roster says are going, at the cells the formation
        /// gives them.
        ///
        /// The formation is the layout, and no longer the team: who walks in is decided by what
        /// the player owns, what they fielded, and how many heroes the stage accepts.
        /// </summary>
        /// <summary>The first hero the formation lists, which is who a brand new game starts with.</summary>
        private string FirstFormationHeroId()
        {
            for (int i = 0; heroFormation != null && i < heroFormation.Placements.Count; i++)
            {
                if (heroFormation.Placements[i].Character != null)
                {
                    return heroFormation.Placements[i].Character.Id;
                }
            }

            return null;
        }

        /// <summary>
        /// Hands out what a stage gives the first time it is cleared, and never again.
        ///
        /// The rewards live in the stage file rather than here, so a rule of the story is content.
        /// A hero arriving and a position opening are separate fields: they usually land together
        /// and they are not the same thing.
        /// </summary>
        private void GrantFirstClear(StageData stage, bool cleared)
        {
            if (!cleared || stage.firstClear == null || clearedStages.Contains(stage.id))
            {
                return;
            }

            clearedStages.Add(stage.id);

            if (!string.IsNullOrWhiteSpace(stage.firstClear.unlocksCharacter)
                && roster.Unlock(stage.firstClear.unlocksCharacter))
            {
                Debug.Log("BattleBootstrap: " + stage.firstClear.unlocksCharacter
                    + " joined the roster after clearing '" + stage.id + "'.", this);
            }

            for (int i = 0; i < stage.firstClear.grantsTeamSlots; i++)
            {
                if (roster.GrantSlot())
                {
                    Debug.Log("BattleBootstrap: the team has " + roster.Slots
                        + " position(s) after clearing '" + stage.id + "'.", this);
                }
            }
        }

        private List<Character> SpawnHeroes(List<string> party)
        {
            List<Character> heroes = new List<Character>();

            for (int i = 0; i < heroFormation.Placements.Count; i++)
            {
                BattleFormation.Placement placement = heroFormation.Placements[i];

                if (placement.Character != null && !party.Contains(placement.Character.Id))
                {
                    continue;
                }

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
