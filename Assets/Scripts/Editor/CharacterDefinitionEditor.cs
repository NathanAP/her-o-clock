using HerOClock.Characters;
using UnityEditor;
using UnityEngine;

namespace HerOClock.EditorTools
{
    /// <summary>
    /// Shows what a sheet actually produces, at a level you pick.
    ///
    /// A sheet is written in primary attributes, but nobody thinks in primary attributes. What a
    /// designer wants to say is "this minion has 370 health and hits for 24", and working out that
    /// POW 24 with CON 25 gets there is arithmetic done by hand, once per character. With twelve
    /// minions and five villains to write for act 1, that stops being a small cost.
    ///
    /// So the sheet keeps the primaries and the Inspector answers the question instead. Minions
    /// and villains keep their primary attributes for a reason worth remembering: abilities scale
    /// off attributes, and the buffs and debuffs of `modify_stat` need an attribute to bite on.
    /// Take the primaries away from enemies and both need a second path built just for them.
    ///
    /// This is a preview and nothing else. Every number here is read from the same properties the
    /// game reads, so it cannot drift from what actually happens.
    /// </summary>
    [CustomEditor(typeof(CharacterDefinition))]
    public class CharacterDefinitionEditor : Editor
    {
        private const string LevelKey = "HerOClock.PreviewLevel";
        private const string MultiplierKey = "HerOClock.PreviewMultiplier";

        private static int previewLevel = 1;
        private static float previewMultiplier = 1f;
        private static bool loaded;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CharacterDefinition definition = (CharacterDefinition)target;

            if (!loaded)
            {
                previewLevel = EditorPrefs.GetInt(LevelKey, 1);
                previewMultiplier = EditorPrefs.GetFloat(MultiplierKey, 1f);
                loaded = true;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("What this sheet produces", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            previewLevel = EditorGUILayout.IntSlider("At level", previewLevel, 1, Mathf.Max(1, definition.MaxLevel));
            previewMultiplier = EditorGUILayout.Slider("Stage multiplier", previewMultiplier, 0f, 3f);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(LevelKey, previewLevel);
                EditorPrefs.SetFloat(MultiplierKey, previewMultiplier);
            }

            CharacterStats stats = definition.Stats.Clone();
            stats.ApplyInstance(previewLevel, definition.Growth, previewMultiplier);

            EditorGUILayout.Space();

            Row("POW / AGI / SPE / CON",
                stats.Power + " / " + stats.Agility + " / " + stats.Specialty + " / " + stats.Constitution);

            EditorGUILayout.Space();

            Row("Maximum health", stats.MaxHealth.ToString());
            Row("Physical damage", Range(stats.PhysicalDamageMin, stats.PhysicalDamageMax));
            Row("Attacks per second", stats.AttacksPerSecond.ToString("F2"));
            Row("Cells per second", stats.CellsPerSecond.ToString("F2"));
            Row("Health per second", stats.HealthPerSecond.ToString("F2"));

            EditorGUILayout.Space();

            Row("Cooldown reduction", stats.CooldownReduction.ToString("F1") + "%");
            Row("Physical armour", stats.PhysicalArmor.ToString());
            Row("Evasion points", stats.EvasionPoints.ToString());

            // Both against an attacker of this character's own level, because that is the number
            // the defence growth exists to hold still. Read against a fixed attacker instead, they
            // would look like they are collapsing when they are not.
            //
            // Evasion has no reading without an attacker at all, since its constant grows with the
            // attacker's level exactly like the mitigation one.
            Row("Mitigation vs same level", stats.PhysicalMitigationAgainst(previewLevel).ToString("F1") + "%");
            Row("Evasion vs same level", stats.EvasionChanceAgainst(previewLevel).ToString("F1") + "%");

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Damage per second below assumes every blow lands, with no evasion and no mitigation "
                + "on the other side. It is for comparing sheets against each other, not for predicting a fight.",
                MessageType.None);

            Row("Physical damage per second", (stats.AveragePhysicalDamage * stats.AttacksPerSecond).ToString("F1"));
        }

        /// <summary>A damage range as the player would read it, collapsed when both ends match.</summary>
        private static string Range(float min, float max)
        {
            int low = Mathf.RoundToInt(min);
            int high = Mathf.RoundToInt(max);

            return low == high ? low.ToString() : low + "-" + high;
        }

        private static void Row(string label, string value)
        {
            EditorGUILayout.LabelField(label, value);
        }
    }
}
