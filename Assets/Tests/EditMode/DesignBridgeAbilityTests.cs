using System;
using System.Collections.Generic;
using System.IO;
using HerOClock.Abilities;
using HerOClock.Characters;
using HerOClock.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// The other half of the bridge: the ability trees.
    ///
    /// `DesignBridgeTests` compares the scalar fields of a sheet and stops there. The abilities are
    /// the largest and most intricate part of a character and were duplicated between
    /// `.claude/specs/characters/` and the assets with **nothing at all** watching them — the same
    /// situation the stage files were in before 0.11.0.1.
    ///
    /// It lives in its own file rather than growing the other one, and the two die together when
    /// the sheets become a single source. Until then this is what makes that move a checked step
    /// instead of a leap: it says, field by field, that the JSON about to be promoted already
    /// agrees with the asset about to be deleted.
    ///
    /// A hero writes `abilityTrees` and everybody else writes a flat `abilities`, following
    /// characters.md. The asset holds one flat list either way, so the trees are flattened here.
    /// **The grouping itself has no counterpart in the asset** and is therefore not compared: it is
    /// information the code does not read yet, and will only start reading in 0.18.0.0.
    /// </summary>
    public class DesignBridgeAbilityTests
    {
        // --- The shape of a design sheet, as far as the abilities go ---

        [Serializable]
        private class SheetFile
        {
            public string id;
            public Tree[] abilityTrees;
            public Ability[] abilities;
        }

        [Serializable]
        private class Tree
        {
            public string id;
            public Ability[] abilities;
        }

        [Serializable]
        private class Ability
        {
            public string id;
            public int ranks;
            public int[] rankAvailability;
            public float[] cooldown;
            public float[] preparation;
            public float[] casting;
            public float[] recoil;
            public Targeting targeting;
            public Effect[] effects;
        }

        [Serializable]
        private class Targeting
        {
            public string who;
            public string shape;
            public string priority;
            public int range;
            public int areaColumns;
            public int areaRows;
            public float[] maxTargets;
            public int jumpRange;
        }

        [Serializable]
        private class Effect
        {
            public string type;
            public string target;
            public float[] duration;
            public string stat;
            public string mode;
            public float[] value;
            public string damageType;
            public float[] @base;
            public Scaling scaling;
            public float[] falloff;
            public string status;
            public string anchor;
        }

        [Serializable]
        private class Scaling
        {
            public float pow;
            public float agi;
            public float spe;
            public float con;
        }

        // --- The vocabularies the data and the code each use ---

        private static readonly Dictionary<string, EffectType> Types = new Dictionary<string, EffectType>
        {
            { "modify_stat", EffectType.ModifyStat },
            { "deal_damage", EffectType.DealDamage },
            { "apply_status", EffectType.ApplyStatus },
            { "move_to", EffectType.MoveTo }
        };

        private static readonly Dictionary<string, EffectTarget> Targets = new Dictionary<string, EffectTarget>
        {
            { "self", EffectTarget.Self },
            { "eachTarget", EffectTarget.EachTarget },
            { "firstTarget", EffectTarget.FirstTarget }
        };

        private static readonly Dictionary<string, ModifiableStat> Stats = new Dictionary<string, ModifiableStat>
        {
            { "pow", ModifiableStat.Power },
            { "agi", ModifiableStat.Agility },
            { "spe", ModifiableStat.Specialty },
            { "con", ModifiableStat.Constitution },
            { "attackSpeed", ModifiableStat.AttackSpeed },
            { "movementSpeed", ModifiableStat.MovementSpeed },
            { "cooldownReduction", ModifiableStat.CooldownReduction },
            { "physicalArmor", ModifiableStat.PhysicalArmor }
        };

        private static readonly Dictionary<string, StatModifierMode> Modes = new Dictionary<string, StatModifierMode>
        {
            { "percent", StatModifierMode.Percent },
            { "flat", StatModifierMode.Flat }
        };

        private static readonly Dictionary<string, DamageType> Damages = new Dictionary<string, DamageType>
        {
            { "physical", DamageType.Physical },
            { "fire", DamageType.Fire },
            { "water", DamageType.Water },
            { "electric", DamageType.Electric }
        };

        private static readonly Dictionary<string, StatusKind> Statuses = new Dictionary<string, StatusKind>
        {
            { "untargetable", StatusKind.Untargetable },
            { "intangible", StatusKind.Intangible },
            { "taunted", StatusKind.Taunted },
            { "silenced", StatusKind.Silenced },
            { "blinded", StatusKind.Blinded }
        };

        private static readonly Dictionary<string, MoveAnchor> Anchors = new Dictionary<string, MoveAnchor>
        {
            { "lastTargetAnySide", MoveAnchor.LastTargetAnySide }
        };

        private static readonly Dictionary<string, AbilityWho> Whos = new Dictionary<string, AbilityWho>
        {
            { "self", AbilityWho.Self },
            { "allies", AbilityWho.Allies },
            { "enemies", AbilityWho.Enemies }
        };

        private static readonly Dictionary<string, AbilityShape> Shapes = new Dictionary<string, AbilityShape>
        {
            { "self", AbilityShape.Self },
            { "single", AbilityShape.Single },
            { "area", AbilityShape.Area },
            { "chain", AbilityShape.Chain },
            { "line", AbilityShape.Line }
        };

        private static readonly Dictionary<string, TargetPriority> Priorities = new Dictionary<string, TargetPriority>
        {
            { "nearest", TargetPriority.Nearest },
            { "farthest", TargetPriority.Farthest },
            { "lowestHealthPercent", TargetPriority.LowestHealthPercent },
            { "lowestMaxHealth", TargetPriority.LowestMaxHealth },
            { "lowestPhysicalArmor", TargetPriority.LowestPhysicalArmor }
        };

        // --- Loading ---

        private static List<SheetFile> Sheets()
        {
            string folder = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", ".claude", "specs", "characters"));

            List<SheetFile> sheets = new List<SheetFile>();

            if (!Directory.Exists(folder))
            {
                return sheets;
            }

            string[] files = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                SheetFile sheet = JsonUtility.FromJson<SheetFile>(File.ReadAllText(files[i]));

                if (sheet != null && !string.IsNullOrEmpty(sheet.id))
                {
                    sheets.Add(sheet);
                }
            }

            return sheets;
        }

        private static Dictionary<string, CharacterDefinition> Assets()
        {
            Dictionary<string, CharacterDefinition> assets = new Dictionary<string, CharacterDefinition>();
            string[] found = AssetDatabase.FindAssets("t:" + typeof(CharacterDefinition).Name);

            for (int i = 0; i < found.Length; i++)
            {
                CharacterDefinition asset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                    AssetDatabase.GUIDToAssetPath(found[i]));

                if (asset != null && !string.IsNullOrEmpty(asset.Id))
                {
                    assets[asset.Id] = asset;
                }
            }

            return assets;
        }

        /// <summary>A hero's trees flattened, or the flat list everybody else writes.</summary>
        private static List<Ability> AbilitiesOf(SheetFile sheet)
        {
            List<Ability> flat = new List<Ability>();

            if (sheet.abilityTrees != null)
            {
                for (int t = 0; t < sheet.abilityTrees.Length; t++)
                {
                    Tree tree = sheet.abilityTrees[t];

                    if (tree != null && tree.abilities != null)
                    {
                        flat.AddRange(tree.abilities);
                    }
                }
            }

            if (sheet.abilities != null)
            {
                flat.AddRange(sheet.abilities);
            }

            return flat;
        }

        // --- The tests ---

        /// <summary>
        /// Every character the game loads has a design sheet, and every design sheet has a
        /// character.
        ///
        /// `gadrat-npc` used to be the exception, skipped by name with a comment saying it mirrored
        /// Gadrat. It did not: it has no abilities where Gadrat has two, so it was a fifth character
        /// whose numbers existed in exactly one place and were checked by nothing.
        /// </summary>
        [Test]
        public void EveryAssetAndEverySheetHaveAPair()
        {
            Dictionary<string, CharacterDefinition> assets = Assets();
            List<SheetFile> sheets = Sheets();

            HashSet<string> sheetIds = new HashSet<string>();

            for (int i = 0; i < sheets.Count; i++)
            {
                sheetIds.Add(sheets[i].id);
            }

            List<string> problems = new List<string>();

            foreach (KeyValuePair<string, CharacterDefinition> asset in assets)
            {
                if (!sheetIds.Contains(asset.Key))
                {
                    problems.Add(asset.Key + ": the game loads this character and no design sheet describes it.");
                }
            }

            foreach (string id in sheetIds)
            {
                if (!assets.ContainsKey(id))
                {
                    problems.Add(id + ": a design sheet describes this character and the game loads no such asset.");
                }
            }

            Assert.IsEmpty(problems, string.Join("\n", problems));
            Assert.Greater(assets.Count, 0, "No character assets were found at all.");
        }

        /// <summary>
        /// Every ability, field by field, on both sides.
        ///
        /// All the problems are collected before reporting, so one run says everything that
        /// disagrees rather than stopping at the first.
        /// </summary>
        [Test]
        public void EveryAbilityInTheAssetSaysWhatTheDesignSheetSays()
        {
            Dictionary<string, CharacterDefinition> assets = Assets();
            List<string> problems = new List<string>();

            foreach (SheetFile sheet in Sheets())
            {
                CharacterDefinition asset;

                if (!assets.TryGetValue(sheet.id, out asset))
                {
                    continue;
                }

                CompareAbilities(sheet, asset, problems);
            }

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        private static void CompareAbilities(SheetFile sheet, CharacterDefinition asset, List<string> problems)
        {
            List<Ability> written = AbilitiesOf(sheet);
            List<AbilityDefinition> built = asset.Abilities ?? new List<AbilityDefinition>();

            if (written.Count != built.Count)
            {
                problems.Add(sheet.id + ": the design sheet describes " + written.Count
                    + " abilities and the asset holds " + built.Count + ".");
                return;
            }

            for (int i = 0; i < written.Count; i++)
            {
                Ability a = written[i];
                AbilityDefinition b = built[i];

                // Compared by position, and the id is the first thing checked, so a reordering
                // reports itself here rather than as a wall of unrelated differences below.
                string where = sheet.id + "/" + a.id;

                Check(where, "id", a.id, b.Id, problems);
                Check(where, "ranks", a.ranks, b.Ranks, problems);
                CheckInts(where, "rankAvailability", a.rankAvailability, b.RankAvailability, problems);

                CheckRanked(where, "cooldown", a.cooldown, b.Cooldown, problems);
                CheckRanked(where, "preparation", a.preparation, b.Preparation, problems);
                CheckRanked(where, "casting", a.casting, b.Casting, problems);
                CheckRanked(where, "recoil", a.recoil, b.Recoil, problems);

                CompareTargeting(where, a.targeting, b.Targeting, problems);
                CompareEffects(where, a.effects, b.Effects, problems);
            }
        }

        private static void CompareTargeting(
            string where, Targeting written, AbilityTargeting built, List<string> problems)
        {
            if (written == null || built == null)
            {
                return;
            }

            CheckEnum(where, "targeting.who", Whos, written.who, built.Who, problems);
            CheckEnum(where, "targeting.shape", Shapes, written.shape, built.Shape, problems);

            // Absent in the file means the default the asset already holds, so an unwritten
            // priority is not a difference. Only a written one is compared.
            if (!string.IsNullOrEmpty(written.priority))
            {
                CheckEnum(where, "targeting.priority", Priorities, written.priority, built.Priority, problems);
            }

            // A range is written only by the shapes that choose a target, which abilities.md lists
            // as `single`, `chain` and `line`. A `self` ability and an area centred on its user have
            // nobody to reach for, so they leave the field out and the asset keeps its default of 1.
            // Zero therefore reads as "not declared" rather than as a range of zero cells, which is
            // a distance `self` already expresses.
            if (written.range > 0)
            {
                Check(where, "targeting.range", written.range, built.Range, problems);
            }

            CheckRanked(where, "targeting.maxTargets", written.maxTargets, built.MaxTargets, problems);

            // The area is written only by the shapes that have one, and the asset keeps its 1s.
            if (written.areaColumns > 0)
            {
                Check(where, "targeting.areaColumns", written.areaColumns, built.AreaColumns, problems);
                Check(where, "targeting.areaRows", written.areaRows, built.AreaRows, problems);
            }

            if (written.jumpRange > 0)
            {
                Check(where, "targeting.jumpRange", written.jumpRange, built.JumpRange, problems);
            }
        }

        private static void CompareEffects(
            string where, Effect[] written, AbilityEffect[] built, List<string> problems)
        {
            int writtenCount = written == null ? 0 : written.Length;
            int builtCount = built == null ? 0 : built.Length;

            if (writtenCount != builtCount)
            {
                problems.Add(where + ": the design sheet describes " + writtenCount
                    + " effects and the asset holds " + builtCount + ".");
                return;
            }

            for (int i = 0; i < writtenCount; i++)
            {
                Effect a = written[i];
                AbilityEffect b = built[i];
                string at = where + "/effect " + i;

                CheckEnum(at, "type", Types, a.type, b.Type, problems);
                CheckEnum(at, "target", Targets, a.target, b.Target, problems);

                CheckRanked(at, "duration", a.duration, b.Duration, problems);
                CheckRanked(at, "value", a.value, b.Value, problems);
                CheckRanked(at, "base", a.@base, b.Base, problems);
                CheckRanked(at, "falloff", a.falloff, b.Falloff, problems);

                // Each of these only means something to one kind of effect, and the asset holds the
                // enum's zero on the others. Comparing an unwritten one would report a difference
                // between "not applicable" and "the first value of the enum".
                if (!string.IsNullOrEmpty(a.stat))
                {
                    CheckEnum(at, "stat", Stats, a.stat, b.Stat, problems);
                }

                if (!string.IsNullOrEmpty(a.mode))
                {
                    CheckEnum(at, "mode", Modes, a.mode, b.Mode, problems);
                }

                if (!string.IsNullOrEmpty(a.damageType))
                {
                    CheckEnum(at, "damageType", Damages, a.damageType, b.DamageType, problems);
                }

                if (!string.IsNullOrEmpty(a.status))
                {
                    CheckEnum(at, "status", Statuses, a.status, b.Status, problems);
                }

                if (!string.IsNullOrEmpty(a.anchor))
                {
                    CheckEnum(at, "anchor", Anchors, a.anchor, b.Anchor, problems);
                }

                CompareScaling(at, a.scaling, b.Scaling, problems);
            }
        }

        private static void CompareScaling(string where, Scaling written, AbilityScaling built, List<string> problems)
        {
            float pow = written == null ? 0f : written.pow;
            float agi = written == null ? 0f : written.agi;
            float spe = written == null ? 0f : written.spe;
            float con = written == null ? 0f : written.con;

            AbilityScaling actual = built ?? new AbilityScaling();

            Check(where, "scaling.pow", pow, actual.Power, problems);
            Check(where, "scaling.agi", agi, actual.Agility, problems);
            Check(where, "scaling.spe", spe, actual.Specialty, problems);
            Check(where, "scaling.con", con, actual.Constitution, problems);
        }

        // --- Comparing ---

        /// <summary>
        /// A written list against the `PerRank` the asset holds.
        ///
        /// An absent field and an empty `PerRank` are the same thing: no value at any rank. Every
        /// design sheet writes these as arrays since 0.11.4.0, so a single value is a one entry
        /// array on both sides.
        /// </summary>
        private static void CheckRanked(
            string where, string field, float[] written, RankedValue built, List<string> problems)
        {
            float[] actual = built.PerRank ?? new float[0];
            float[] expected = written ?? new float[0];

            if (expected.Length != actual.Length)
            {
                problems.Add(where + ": the design sheet gives " + field + " " + expected.Length
                    + " entries and the asset gives it " + actual.Length + ".");
                return;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                Check(where, field + "[" + i + "]", expected[i], actual[i], problems);
            }
        }

        private static void CheckInts(
            string where, string field, int[] written, int[] built, List<string> problems)
        {
            int[] actual = built ?? new int[0];
            int[] expected = written ?? new int[0];

            if (expected.Length != actual.Length)
            {
                problems.Add(where + ": the design sheet gives " + field + " " + expected.Length
                    + " entries and the asset gives it " + actual.Length + ".");
                return;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                Check(where, field + "[" + i + "]", expected[i], actual[i], problems);
            }
        }

        /// <summary>
        /// A word from the data against a value from the code.
        ///
        /// A word the map does not know is a problem of its own, and a loud one: it means the sheet
        /// names something the game has no value for, which would otherwise be read as the enum's
        /// zero and work by accident.
        /// </summary>
        private static void CheckEnum<T>(
            string where, string field, Dictionary<string, T> vocabulary, string written, T built,
            List<string> problems) where T : struct
        {
            T expected;

            if (!vocabulary.TryGetValue(written ?? string.Empty, out expected))
            {
                problems.Add(where + ": the design sheet says " + field + " is \"" + written
                    + "\", which is not a value the game knows.");
                return;
            }

            Check(where, field, expected, built, problems);
        }

        private static void Check<T>(string where, string field, T fromSheet, T fromAsset, List<string> problems)
        {
            if (!EqualityComparer<T>.Default.Equals(fromSheet, fromAsset))
            {
                problems.Add(where + ": the design sheet says " + field + " is " + fromSheet
                    + ", and the asset says " + fromAsset + ".");
            }
        }
    }
}
