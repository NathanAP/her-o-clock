using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Characters;
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

        private int level = 1;
        private float multiplier = 1f;
        private bool showAbilities = true;
        private Vector2 scroll;

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
        }

        private void OnGUI()
        {
            DrawControls();

            List<CharacterDefinition> sheets = LoadSheets();

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
            Cell(stats.PhysicalDamage.ToString(), 3);
            Cell(stats.AttacksPerSecond.ToString("F2"), 4);
            Cell((stats.PhysicalDamage * stats.AttacksPerSecond).ToString("F1"), 5);
            Cell(stats.CellsPerSecond.ToString("F2"), 6);
            Cell(stats.EvasionChance.ToString("F1") + "%", 7);
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
