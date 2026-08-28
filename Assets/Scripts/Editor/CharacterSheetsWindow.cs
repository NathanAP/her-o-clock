using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Characters;
using HerOClock.Progression;
using HerOClock.Setup;
using UnityEditor;
using UnityEngine;

namespace HerOClock.EditorTools
{
    /// <summary>
    /// Every character sheet in the project, side by side, at a level you pick.
    ///
    /// The per-asset Inspector already answers "what does this sheet produce", and
    /// `.claude/balance/snapshot.md` already answers "what did this change do to the rest of the
    /// game", but only at each sheet's own starting level and only once it has been committed.
    /// Neither answers the question this window exists for: **how do these two compare right now,
    /// at the level they actually meet at.**
    ///
    /// A minion is written at the level a stage hands it, and a hero at whatever level the player
    /// arrived with. Comparing them means putting them at the same level on purpose, and that is
    /// a slider and not a file.
    ///
    /// Everything here is read through the same properties the game reads, so it cannot drift
    /// from what actually happens. Nothing here writes.
    /// </summary>
    public class CharacterSheetsWindow : EditorWindow
    {
        private const string LevelKey = "HerOClock.Sheets.Level";
        private const string MultiplierKey = "HerOClock.Sheets.Multiplier";
        private const string AbilitiesKey = "HerOClock.Sheets.Abilities";
        private const string TabKey = "HerOClock.Sheets.Tab";

        /// <summary>The two questions this window answers, which are not the same question.</summary>
        private enum Tab
        {
            /// <summary>What a sheet produces at a level you pick. True whether or not the game is running.</summary>
            Sheets = 0,

            /// <summary>What the heroes of the session running right now actually are.</summary>
            Heroes = 1
        }

        private Tab tab = Tab.Sheets;

        private int level = 1;
        private float multiplier = 1f;
        private bool showAbilities = true;
        private Vector2 scroll;

        /// <summary>
        /// The sheets on screen, held rather than looked up again.
        ///
        /// <c>OnGUI</c> runs on every repaint, which includes the mouse simply moving across the
        /// window, so searching the project from in there means a project wide query per frame
        /// for a set that changes only when somebody adds or edits an asset. Reading it on the
        /// three moments it can actually have changed costs nothing and stays just as live.
        /// </summary>
        private List<CharacterDefinition> sheets;

        [MenuItem("Her-o-clock/Character sheets")]
        public static void Open()
        {
            CharacterSheetsWindow window = GetWindow<CharacterSheetsWindow>("Character sheets");
            window.minSize = new Vector2(760f, 320f);
        }

        private void OnEnable()
        {
            level = EditorPrefs.GetInt(LevelKey, 1);
            multiplier = EditorPrefs.GetFloat(MultiplierKey, 1f);
            showAbilities = EditorPrefs.GetBool(AbilitiesKey, true);
            tab = (Tab)EditorPrefs.GetInt(TabKey, (int)Tab.Sheets);

            Reload();
        }

        /// <summary>Coming back to the window is the cheapest place to notice a sheet was added.</summary>
        private void OnFocus()
        {
            Reload();
        }

        /// <summary>Fires when an asset is created, deleted, moved or reimported.</summary>
        private void OnProjectChange()
        {
            Reload();
        }

        private void Reload()
        {
            sheets = LoadSheets();
        }

        /// <summary>
        /// Repaints while the game runs, because a record moves without the editor being told.
        ///
        /// Only while playing and only on the live tab: an editor window that repaints forever
        /// costs frames that the person using the editor paid for.
        /// </summary>
        private void Update()
        {
            if (tab == Tab.Heroes && EditorApplication.isPlaying)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawTabs();

            if (tab == Tab.Heroes)
            {
                DrawLiveHeroes();
                return;
            }

            DrawControls();

            // A domain reload can leave the window alive with the list gone.
            if (sheets == null)
            {
                Reload();
            }

            if (sheets.Count == 0)
            {
                EditorGUILayout.HelpBox("No character sheets found in the project.", MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            // Grouped by kind rather than listed flat, because a hero is never compared against a
            // minion by accident: they are read against others of their own sort first.
            DrawGroup("Heroes", sheets, CharacterKind.Hero);
            DrawGroup("Minions", sheets, CharacterKind.Minion);
            DrawGroup("Villains", sheets, CharacterKind.Villain);
            DrawGroup("NPCs", sheets, CharacterKind.Npc);

            EditorGUILayout.EndScrollView();
        }

        private void DrawTabs()
        {
            EditorGUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();

            tab = (Tab)GUILayout.Toolbar((int)tab, new[] { "Sheets", "Heroes in play" });

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(TabKey, (int)tab);
            }
        }

        // --- The heroes of the session running right now ---

        /// <summary>
        /// What each hero **is** at this moment, which no sheet can answer.
        ///
        /// A sheet is content and a record is a save. Since a hero and the thing that fights are
        /// two objects, the record is the only place the real level lives — and it stopped being
        /// visible anywhere once the level label came off the character.
        ///
        /// It reads and never writes, like the rest of this window.
        /// </summary>
        private void DrawLiveHeroes()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "A hero only exists while the game is running. Press Play and come back.",
                    MessageType.Info);
                return;
            }

            // Any and not First: there is exactly one bootstrap in the scene, so asking for "the
            // first" would be asking for an ordering that does not matter — which is the reason
            // FindFirstObjectByType is deprecated.
            BattleBootstrap bootstrap = FindAnyObjectByType<BattleBootstrap>();

            if (bootstrap == null)
            {
                EditorGUILayout.HelpBox("No BattleBootstrap in the scene.", MessageType.Warning);
                return;
            }

            List<HeroRecord> records = new List<HeroRecord>(bootstrap.LiveRecords);

            if (records.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No hero has a record yet. They are created as the party is first built.",
                    MessageType.Info);
                return;
            }

            records.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

            EditorGUILayout.Space(2f);
            EditorGUILayout.HelpBox(
                "What each hero is right now. The combatant on the board was built from this when "
                + "the stage began, so during a stage the two can differ — that is the rule, not a "
                + "fault.",
                MessageType.None);

            EditorGUILayout.Space(4f);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawLiveHeader();

            for (int i = 0; i < records.Count; i++)
            {
                DrawLiveRow(records[i]);
            }

            EditorGUILayout.EndScrollView();
        }

        private static readonly float[] LiveWidths = { 150f, 52f, 150f, 62f, 120f, 90f };

        private static readonly string[] LiveHeaders =
        {
            "Hero", "Level", "To next level", "Skill pts", "POW/AGI/SPE/CON", "Unspent"
        };

        private static void DrawLiveHeader()
        {
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < LiveHeaders.Length; i++)
            {
                EditorGUILayout.LabelField(LiveHeaders[i], EditorStyles.miniBoldLabel,
                    GUILayout.Width(LiveWidths[i]));
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawLiveRow(HeroRecord record)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(record.Id, EditorStyles.linkLabel, GUILayout.Width(LiveWidths[0])))
            {
                Selection.activeObject = record.Definition;
                EditorGUIUtility.PingObject(record.Definition);
            }

            LevelProgress progress = record.Progress;

            EditorGUILayout.LabelField(progress.Level.ToString(), GUILayout.Width(LiveWidths[1]));

            // The maximum level has nothing to count towards, and a bar filling to nowhere reads
            // as a hero stuck one level short.
            string towards = progress.IsMaxLevel
                ? "max level"
                : progress.CurrentXp + " / " + ExperienceTable.XpToNextLevel(progress.Level);

            EditorGUILayout.LabelField(towards, GUILayout.Width(LiveWidths[2]));
            EditorGUILayout.LabelField(progress.SkillPoints.ToString(), GUILayout.Width(LiveWidths[3]));

            AttributeAllocation attributes = record.Attributes;

            string split = attributes.SpentOn(Attribute.Power)
                + "/" + attributes.SpentOn(Attribute.Agility)
                + "/" + attributes.SpentOn(Attribute.Specialty)
                + "/" + attributes.SpentOn(Attribute.Constitution);

            EditorGUILayout.LabelField(split, GUILayout.Width(LiveWidths[4]));

            string unspent = attributes.IsAutomatic
                ? "automatic"
                : attributes.Unspent.ToString();

            EditorGUILayout.LabelField(unspent, GUILayout.Width(LiveWidths[5]));

            EditorGUILayout.EndHorizontal();
        }

        // --- The sheets ---

        private void DrawControls()
        {
            EditorGUILayout.Space(4f);
            EditorGUI.BeginChangeCheck();

            level = EditorGUILayout.IntSlider("At level", level, 1, 100);
            multiplier = EditorGUILayout.Slider("Stage multiplier", multiplier, 0f, 3f);
            showAbilities = EditorGUILayout.Toggle("Show abilities", showAbilities);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(LevelKey, level);
                EditorPrefs.SetFloat(MultiplierKey, multiplier);
                EditorPrefs.SetBool(AbilitiesKey, showAbilities);
            }

            EditorGUILayout.HelpBox(
                "Every sheet at the same level, with no items. Mitigation is read against an "
                + "attacker of that same level, which is the number the defence growth exists to "
                + "hold still. Damage per second assumes every blow lands.",
                MessageType.None);

            EditorGUILayout.Space(4f);
        }

        private static List<CharacterDefinition> LoadSheets()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(CharacterDefinition));
            List<CharacterDefinition> found = new List<CharacterDefinition>();

            for (int i = 0; i < guids.Length; i++)
            {
                CharacterDefinition sheet = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                    AssetDatabase.GUIDToAssetPath(guids[i]));

                if (sheet != null)
                {
                    found.Add(sheet);
                }
            }

            found.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            return found;
        }

        private void DrawGroup(string title, List<CharacterDefinition> sheets, CharacterKind kind)
        {
            List<CharacterDefinition> ofKind = new List<CharacterDefinition>();

            for (int i = 0; i < sheets.Count; i++)
            {
                // A held sheet can be destroyed under the window between one repaint and the
                // next, by a delete or by undoing a creation. Comparing against null uses
                // Unity's own operator, which is what reports a destroyed object as gone.
                if (sheets[i] == null)
                {
                    continue;
                }

                if (sheets[i].Kind == kind)
                {
                    ofKind.Add(sheets[i]);
                }
            }

            if (ofKind.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            DrawHeaderRow();

            for (int i = 0; i < ofKind.Count; i++)
            {
                DrawSheetRow(ofKind[i]);
            }
        }

        // --- The table ---

        private static readonly float[] Widths = { 150f, 96f, 58f, 58f, 52f, 62f, 60f, 66f, 70f, 62f };

        private static readonly string[] Headers =
        {
            "Sheet", "POW/AGI/SPE/CON", "Health", "Damage", "Atk/s", "DPS", "Cells/s", "Evasion", "Armour", "Mitig."
        };

        private static void DrawHeaderRow()
        {
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < Headers.Length; i++)
            {
                EditorGUILayout.LabelField(Headers[i], EditorStyles.miniBoldLabel, GUILayout.Width(Widths[i]));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSheetRow(CharacterDefinition sheet)
        {
            CharacterStats stats = sheet.Stats.Clone();
            stats.ApplyInstance(level, sheet.Growth, multiplier);

            EditorGUILayout.BeginHorizontal();

            // Clicking the name selects the asset, so the window is a way into the sheets and not
            // only a way of looking at them.
            if (GUILayout.Button(sheet.Id, EditorStyles.linkLabel, GUILayout.Width(Widths[0])))
            {
                Selection.activeObject = sheet;
                EditorGUIUtility.PingObject(sheet);
            }

            Cell(stats.Power + "/" + stats.Agility + "/" + stats.Specialty + "/" + stats.Constitution, 1);
            Cell(stats.MaxHealth.ToString(), 2);
            Cell(Mathf.RoundToInt(stats.PhysicalDamageMin) + "-" + Mathf.RoundToInt(stats.PhysicalDamageMax), 3);
            Cell(stats.AttacksPerSecond.ToString("F2"), 4);
            Cell((stats.AveragePhysicalDamage * stats.AttacksPerSecond).ToString("F1"), 5);
            Cell(stats.CellsPerSecond.ToString("F2"), 6);
            Cell(stats.EvasionChanceAgainst(level).ToString("F1") + "%", 7);
            Cell(stats.PhysicalArmor.ToString(), 8);
            Cell(stats.PhysicalMitigationAgainst(level).ToString("F1") + "%", 9);

            EditorGUILayout.EndHorizontal();

            if (showAbilities)
            {
                DrawAbilities(sheet, stats);
            }
        }

        private static void Cell(string value, int column)
        {
            EditorGUILayout.LabelField(value, GUILayout.Width(Widths[column]));
        }

        // --- Abilities ---

        /// <summary>
        /// What each ability does at this level, and **who it can catch**.
        ///
        /// The reach is spelled out rather than left to the reader, because an area is centred on
        /// whoever uses it and catches everyone inside, ally included, exactly as `abilities.md`
        /// says. That rule is easy to agree with in the abstract and easy to be surprised by on
        /// the board, and there was nowhere in the editor that showed it.
        /// </summary>
        private void DrawAbilities(CharacterDefinition sheet, CharacterStats stats)
        {
            if (sheet.Abilities == null || sheet.Abilities.Count == 0)
            {
                return;
            }

            EditorGUI.indentLevel += 2;

            for (int i = 0; i < sheet.Abilities.Count; i++)
            {
                AbilityDefinition ability = sheet.Abilities[i];

                if (ability == null)
                {
                    continue;
                }

                int rank = ability.RankAt(level);

                if (rank <= 0)
                {
                    EditorGUILayout.LabelField(ability.Id, "not available yet at this level");
                    continue;
                }

                EditorGUILayout.LabelField(
                    ability.Id + "  (rank " + rank + " of " + ability.Ranks + ")",
                    Reach(ability) + Damage(ability, rank, stats));
            }

            EditorGUI.indentLevel -= 2;
        }

        private static string Reach(AbilityDefinition ability)
        {
            AbilityTargeting targeting = ability.Targeting;

            if (targeting == null)
            {
                return "";
            }

            switch (targeting.Shape)
            {
                case AbilityShape.Area:
                    return "area " + targeting.AreaColumns + "x" + targeting.AreaRows
                        + " on self, CATCHES ALLIES";

                case AbilityShape.Line:
                    return "line, range " + targeting.Range + ", catches everyone in the way";

                case AbilityShape.Chain:
                    return "chain, range " + targeting.Range;

                case AbilityShape.Self:
                    return "self";

                default:
                    return "single, range " + targeting.Range;
            }
        }

        private static string Damage(AbilityDefinition ability, int rank, CharacterStats stats)
        {
            if (ability.Effects == null)
            {
                return "";
            }

            for (int i = 0; i < ability.Effects.Length; i++)
            {
                AbilityEffect effect = ability.Effects[i];

                if (effect == null || effect.Type != EffectType.DealDamage)
                {
                    continue;
                }

                float raw = effect.Base.At(rank) * effect.Scaling.MultiplierFor(stats);
                string who = effect.Target == EffectTarget.Self ? " to itself" : "";

                return "  |  " + Mathf.RoundToInt(raw) + " " + effect.DamageType + who;
            }

            return "";
        }
    }
}
