using System;
using System.Collections.Generic;
using System.IO;
using HerOClock.Characters;
using NUnit.Framework;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// The bridge: every asset the game reads has to agree with the design document it mirrors.
    ///
    /// The same numbers live in two places — `.claude/specs/characters/` says what a character is
    /// meant to be, and `Assets/ScriptableObjects/` is what the game actually loads. That is a
    /// duplication the project chose on purpose, for the reason "## Onde cada número mora" in
    /// CLAUDE.md gives: two independent statements of the same value, so a disagreement between
    /// them shows up instead of quietly winning.
    ///
    /// Without this test the two simply drift. Somebody edits a design sheet, nothing complains,
    /// and months later nobody can say which side was right.
    ///
    /// It only compares **inputs**. What those numbers produce — health, mitigation, how long a
    /// stage takes — belongs to the balance snapshot and is measured, not asserted here.
    /// </summary>
    public class DesignBridgeTests
    {
        // --- The shape of a design sheet, as far as this test needs it ---

        [Serializable]
        private class Sheet
        {
            public string id;
            public string kind;
            public int maxLevel;
            public string equipment;
            public int minRange;
            public int maxRange;
            public int thornsPercent;
            public int lifeStealPercent;
            public int healthRegen;
            public Attributes baseAttributes;
            public Attributes attributeGrowth;
            public Defence defence;
            public Scaled baseDamage;
            public AutoAttacks autoAttacks;
        }

        [Serializable]
        private class Attributes
        {
            public int pow;
            public int agi;
            public int spe;
            public int con;
        }

        [Serializable]
        private class Defence
        {
            public Scaled physicalArmor;
            public Scaled fireResistance;
            public Scaled waterResistance;
            public Scaled electricResistance;
        }

        [Serializable]
        private class Scaled
        {
            public int @base;
            public int perLevel;
        }

        [Serializable]
        private class AutoAttacks
        {
            public string type;
        }

        private static readonly Dictionary<string, CharacterKind> Kinds = new Dictionary<string, CharacterKind>
        {
            { "hero", CharacterKind.Hero },
            { "minion", CharacterKind.Minion },
            { "villain", CharacterKind.Villain },
            { "npc", CharacterKind.Npc }
        };

        private static readonly Dictionary<string, EquipmentClass> Equipment = new Dictionary<string, EquipmentClass>
        {
            { "light", EquipmentClass.Light },
            { "magic", EquipmentClass.Magic },
            { "heavy", EquipmentClass.Heavy }
        };

        private static readonly Dictionary<string, AutoAttackType> Attacks = new Dictionary<string, AutoAttackType>
        {
            { "melee", AutoAttackType.Melee },
            { "ranged", AutoAttackType.Ranged }
        };

        private static List<Sheet> Sheets()
        {
            string folder = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", ".claude", "specs", "characters"));

            List<Sheet> sheets = new List<Sheet>();

            if (!Directory.Exists(folder))
            {
                return sheets;
            }

            string[] files = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                sheets.Add(JsonUtility.FromJson<Sheet>(File.ReadAllText(files[i])));
            }

            return sheets;
        }

        private static Dictionary<string, CharacterDefinition> Assets()
        {
            Dictionary<string, CharacterDefinition> byId = new Dictionary<string, CharacterDefinition>();

            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterDefinition");

            for (int i = 0; i < guids.Length; i++)
            {
                CharacterDefinition asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                    UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]));

                if (asset != null && !string.IsNullOrWhiteSpace(asset.Id))
                {
                    byId[asset.Id] = asset;
                }
            }

            return byId;
        }

        /// <summary>
        /// The guard that keeps every other test here from passing by saying nothing.
        ///
        /// A bridge with no pair to compare reports success and means it has checked nothing, which
        /// is the exact failure this whole class was built to prevent somewhere else.
        /// </summary>
        [Test]
        public void ThereArePairsToCompare()
        {
            Dictionary<string, CharacterDefinition> assets = Assets();
            int pairs = 0;

            foreach (Sheet sheet in Sheets())
            {
                if (sheet != null && assets.ContainsKey(sheet.id))
                {
                    pairs++;
                }
            }

            Assert.Greater(pairs, 0,
                "No design sheet matched any asset by id, so the bridge is checking nothing at all.");
        }

        /// <summary>
        /// Field by field, for every design sheet that has an asset.
        ///
        /// An asset with no sheet is skipped rather than failed: `gadrat-npc` mirrors Gadrat's sheet
        /// deliberately and has none of its own, and the four test sheets used to live here too.
        /// Every problem is collected before reporting, so one run says everything that is wrong.
        /// </summary>
        [Test]
        public void EveryAssetSaysWhatItsDesignSheetSays()
        {
            Dictionary<string, CharacterDefinition> assets = Assets();
            List<string> problems = new List<string>();

            foreach (Sheet sheet in Sheets())
            {
                CharacterDefinition asset;

                if (sheet == null || !assets.TryGetValue(sheet.id, out asset))
                {
                    continue;
                }

                Compare(sheet, asset, problems);
            }

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        private static void Compare(Sheet sheet, CharacterDefinition asset, List<string> problems)
        {
            Check(sheet.id, "kind", Kinds[sheet.kind], asset.Kind, problems);
            Check(sheet.id, "maxLevel", sheet.maxLevel, asset.MaxLevel, problems);
            Check(sheet.id, "minRange", sheet.minRange, asset.MinRange, problems);
            Check(sheet.id, "maxRange", sheet.maxRange, asset.MaxRange, problems);
            Check(sheet.id, "equipment", Equipment[sheet.equipment], asset.Stats.Equipment, problems);

            if (sheet.autoAttacks != null)
            {
                Check(sheet.id, "autoAttacks.type", Attacks[sheet.autoAttacks.type], asset.AutoAttack, problems);
            }

            Check(sheet.id, "baseAttributes.pow", sheet.baseAttributes.pow, asset.Stats.BasePower, problems);
            Check(sheet.id, "baseAttributes.agi", sheet.baseAttributes.agi, asset.Stats.BaseAgility, problems);
            Check(sheet.id, "baseAttributes.spe", sheet.baseAttributes.spe, asset.Stats.BaseSpecialty, problems);
            Check(sheet.id, "baseAttributes.con", sheet.baseAttributes.con, asset.Stats.BaseConstitution, problems);

            Check(sheet.id, "attributeGrowth.pow", sheet.attributeGrowth.pow, asset.Growth.Power, problems);
            Check(sheet.id, "attributeGrowth.agi", sheet.attributeGrowth.agi, asset.Growth.Agility, problems);
            Check(sheet.id, "attributeGrowth.spe", sheet.attributeGrowth.spe, asset.Growth.Specialty, problems);
            Check(sheet.id, "attributeGrowth.con", sheet.attributeGrowth.con, asset.Growth.Constitution, problems);

            if (sheet.baseDamage != null)
            {
                Check(sheet.id, "baseDamage.base", sheet.baseDamage.@base, asset.Stats.BaseDamage, problems);
                Check(sheet.id, "baseDamage.perLevel", sheet.baseDamage.perLevel,
                    asset.Stats.BaseDamagePerLevel, problems);
            }

            Check(sheet.id, "defence.physicalArmor.base", sheet.defence.physicalArmor.@base,
                asset.Stats.BasePhysicalArmor, problems);
            Check(sheet.id, "defence.physicalArmor.perLevel", sheet.defence.physicalArmor.perLevel,
                asset.Stats.PhysicalArmorPerLevel, problems);
            Check(sheet.id, "defence.fireResistance.base", sheet.defence.fireResistance.@base,
                asset.Stats.BaseFireResistance, problems);
            Check(sheet.id, "defence.fireResistance.perLevel", sheet.defence.fireResistance.perLevel,
                asset.Stats.FireResistancePerLevel, problems);
            Check(sheet.id, "defence.waterResistance.base", sheet.defence.waterResistance.@base,
                asset.Stats.BaseWaterResistance, problems);
            Check(sheet.id, "defence.waterResistance.perLevel", sheet.defence.waterResistance.perLevel,
                asset.Stats.WaterResistancePerLevel, problems);
            Check(sheet.id, "defence.electricResistance.base", sheet.defence.electricResistance.@base,
                asset.Stats.BaseElectricResistance, problems);
            Check(sheet.id, "defence.electricResistance.perLevel", sheet.defence.electricResistance.perLevel,
                asset.Stats.ElectricResistancePerLevel, problems);

            Check(sheet.id, "thornsPercent", sheet.thornsPercent, (int)asset.Stats.ThornsPercent, problems);
            Check(sheet.id, "lifeStealPercent", sheet.lifeStealPercent, (int)asset.Stats.LifeStealPercent, problems);
            Check(sheet.id, "healthRegen", sheet.healthRegen, (int)asset.Stats.BaseHealthRegen, problems);
        }

        private static void Check<T>(string id, string field, T fromSheet, T fromAsset, List<string> problems)
        {
            if (!EqualityComparer<T>.Default.Equals(fromSheet, fromAsset))
            {
                problems.Add(id + ": the design sheet says " + field + " is " + fromSheet
                    + ", and the asset says " + fromAsset + ".");
            }
        }
    }
}
