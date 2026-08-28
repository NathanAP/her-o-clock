using System.Collections.Generic;

namespace HerOClock.Items
{
    /// <summary>
    /// Checks the item data files against each other before the game tries to use them.
    ///
    /// It is the same price the stages pay for living in JSON: every reference between the files
    /// is a piece of text, so a typo would otherwise only show up much later as a modifier that
    /// never rolls or an item with no name. Every problem in a set of files is reported at once
    /// rather than stopping at the first.
    ///
    /// It knows nothing about Unity, so it can be tested outside the editor.
    /// </summary>
    public static class ItemValidator
    {
        public static List<string> Validate(
            ItemSlots slots,
            ItemSubtypes subtypes,
            ItemModifiers modifiers,
            ItemTiers tiers,
            ItemUniqueRules uniques)
        {
            List<string> problems = new List<string>();

            HashSet<string> slotIds = CheckSlots(slots, problems);
            HashSet<string> classIds = CheckClasses(slots, problems);

            CheckSubtypes(subtypes, classIds, problems);
            CheckModifiers(modifiers, slotIds, problems);
            CheckTiers(tiers, problems);
            CheckUniques(uniques, problems);

            return problems;
        }

        /// <summary>Slot ids, plus the roles the off hand splits into, since modifiers name those.</summary>
        private static HashSet<string> CheckSlots(ItemSlots slots, List<string> problems)
        {
            HashSet<string> ids = new HashSet<string>();

            if (slots == null || slots.slots == null || slots.slots.Length == 0)
            {
                problems.Add("slots.json could not be read, or declares no slot.");
                return ids;
            }

            for (int i = 0; i < slots.slots.Length; i++)
            {
                ItemSlot slot = slots.slots[i];

                if (string.IsNullOrEmpty(slot.id))
                {
                    problems.Add("A slot at position " + i + " has no id.");
                    continue;
                }

                if (!ids.Add(slot.id))
                {
                    problems.Add("The slot id " + slot.id + " appears more than once.");
                }

                if (slot.defenceWeight < 0f)
                {
                    problems.Add(slot.id + " has a negative defence weight.");
                }

                if (slot.roles == null)
                {
                    continue;
                }

                for (int r = 0; r < slot.roles.Length; r++)
                {
                    if (!ids.Add(slot.roles[r]))
                    {
                        problems.Add("The role " + slot.roles[r] + " collides with another slot id.");
                    }
                }
            }

            if (slots.defenceBudget == null)
            {
                problems.Add("slots.json has no defence budget.");
            }

            return ids;
        }

        /// <summary>
        /// The equipment classes, taken from the defence table, and the check that the other two
        /// tables cover exactly the same set.
        ///
        /// It is generic rather than a list of six names on purpose: adding a class has to be a
        /// change to the data and never to this file.
        /// </summary>
        private static HashSet<string> CheckClasses(ItemSlots slots, List<string> problems)
        {
            HashSet<string> ids = new HashSet<string>();

            if (slots == null || slots.classDefence == null || slots.classDefence.Length == 0)
            {
                problems.Add("slots.json declares no equipment class.");
                return ids;
            }

            for (int i = 0; i < slots.classDefence.Length; i++)
            {
                string id = slots.classDefence[i].id;

                if (string.IsNullOrEmpty(id))
                {
                    problems.Add("A class in classDefence has no id.");
                }
                else if (!ids.Add(id))
                {
                    problems.Add("The class " + id + " appears more than once in classDefence.");
                }
            }

            CheckClassCoverage(ids, Ids(slots.requirement == null ? null : slots.requirement.attributeByClass),
                "requirement.attributeByClass", problems);

            CheckClassCoverage(ids, Ids(slots.armourNaming == null ? null : slots.armourNaming.classNouns),
                "armourNaming.classNouns", problems);

            if (slots.armourNaming != null && slots.armourNaming.classNouns != null)
            {
                for (int i = 0; i < slots.armourNaming.classNouns.Length; i++)
                {
                    ClassNouns entry = slots.armourNaming.classNouns[i];

                    if (entry.nouns == null || entry.nouns.Length == 0)
                    {
                        problems.Add("The class " + entry.id + " has no naming noun, so its armour cannot be named.");
                        continue;
                    }

                    if (entry.nouns[0].minItemLevel != 1)
                    {
                        problems.Add("The naming nouns of " + entry.id + " start at item level "
                            + entry.nouns[0].minItemLevel + ", so an item below that has no name.");
                    }
                }
            }

            return ids;
        }

        private static void CheckClassCoverage(
            HashSet<string> expected, HashSet<string> found, string where, List<string> problems)
        {
            foreach (string id in expected)
            {
                if (!found.Contains(id))
                {
                    problems.Add("The class " + id + " is missing from " + where + ".");
                }
            }

            foreach (string id in found)
            {
                if (!expected.Contains(id))
                {
                    problems.Add(where + " declares " + id + ", which is not an equipment class.");
                }
            }
        }

        private static void CheckSubtypes(
            ItemSubtypes subtypes, HashSet<string> classIds, List<string> problems)
        {
            HashSet<string> families = new HashSet<string>();

            if (subtypes == null || subtypes.weapons == null || subtypes.weapons.Length == 0)
            {
                problems.Add("subtypes.json could not be read, or declares no weapon.");
                return;
            }

            if (subtypes.damageBudget != null && subtypes.damageBudget.families != null)
            {
                for (int i = 0; i < subtypes.damageBudget.families.Length; i++)
                {
                    families.Add(subtypes.damageBudget.families[i].id);
                }
            }

            if (families.Count == 0)
            {
                problems.Add("damageBudget declares no family, so no weapon has a budget to meet.");
            }

            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < subtypes.weapons.Length; i++)
            {
                WeaponSubtype weapon = subtypes.weapons[i];

                if (!ids.Add(weapon.id))
                {
                    problems.Add("The subtype id " + weapon.id + " appears more than once.");
                }

                if (!families.Contains(weapon.family))
                {
                    problems.Add(weapon.id + " belongs to the family " + weapon.family + ", which has no budget.");
                }

                if (classIds.Count > 0 && !classIds.Contains(weapon.naturalClass))
                {
                    problems.Add(weapon.id + " has the natural class " + weapon.naturalClass + ", which does not exist.");
                }

                if (weapon.hands != 1 && weapon.hands != 2)
                {
                    problems.Add(weapon.id + " occupies " + weapon.hands + " hands, and only 1 or 2 exist.");
                }

                if (weapon.minRange < 1 || weapon.maxRange < weapon.minRange)
                {
                    problems.Add(weapon.id + " has a reach of " + weapon.minRange + " to " + weapon.maxRange + ".");
                }

                if (weapon.attackSpeed <= 0f)
                {
                    problems.Add(weapon.id + " attacks " + weapon.attackSpeed + " times a second.");
                }

                CheckRange(weapon.id, weapon.damage, problems);
                CheckBases(weapon.id, weapon.bases, problems);
            }

            if (subtypes.offHandDefensive == null || subtypes.offHandDefensive.Length == 0)
            {
                problems.Add("subtypes.json declares no defensive off hand.");
                return;
            }

            for (int i = 0; i < subtypes.offHandDefensive.Length; i++)
            {
                OffHandDefensive off = subtypes.offHandDefensive[i];

                if (!ids.Add(off.id))
                {
                    problems.Add("The subtype id " + off.id + " appears more than once.");
                }

                if (off.grants != "armour" && off.grants != "evasion" && off.grants != "elementalResistance")
                {
                    problems.Add(off.id + " grants " + off.grants + ", which is not one of the three defences.");
                }

                CheckBases(off.id, off.bases, problems);
            }
        }

        private static void CheckRange(string owner, ScaledRange range, List<string> problems)
        {
            if (range == null || range.min == null || range.max == null)
            {
                problems.Add(owner + " has no damage range.");
                return;
            }

            if (range.max.@base < range.min.@base || range.max.perLevel < range.min.perLevel)
            {
                problems.Add(owner + " has a damage range whose maximum is below its minimum.");
            }
        }

        private static void CheckBases(string owner, LevelledBase[] bases, List<string> problems)
        {
            if (bases == null || bases.Length == 0)
            {
                problems.Add(owner + " has no base name, so an item of it cannot be named.");
                return;
            }

            if (bases[0].minItemLevel != 1)
            {
                problems.Add(owner + " has no base name below item level " + bases[0].minItemLevel + ".");
            }

            for (int i = 1; i < bases.Length; i++)
            {
                if (bases[i].minItemLevel <= bases[i - 1].minItemLevel)
                {
                    problems.Add(owner + " has base names that do not rise in item level.");
                }
            }
        }

        private static void CheckModifiers(ItemModifiers modifiers, HashSet<string> slotIds, List<string> problems)
        {
            if (modifiers == null || modifiers.slotGroups == null)
            {
                problems.Add("modifiers.json could not be read.");
                return;
            }

            HashSet<string> groups = new HashSet<string>();

            for (int i = 0; i < modifiers.slotGroups.Length; i++)
            {
                SlotGroup group = modifiers.slotGroups[i];

                if (!groups.Add(group.id))
                {
                    problems.Add("The slot group " + group.id + " appears more than once.");
                }

                if (group.slots == null || group.slots.Length == 0)
                {
                    problems.Add("The slot group " + group.id + " is empty.");
                    continue;
                }

                for (int s = 0; s < group.slots.Length; s++)
                {
                    if (slotIds.Count > 0 && !slotIds.Contains(group.slots[s]))
                    {
                        problems.Add("The slot group " + group.id + " names " + group.slots[s]
                            + ", which is not a slot or a role.");
                    }
                }
            }

            CheckModifierList(modifiers.hardware, "hardware", groups, problems);
            CheckModifierList(modifiers.software, "software", groups, problems);
        }

        private static void CheckModifierList(
            ItemModifier[] list, string family, HashSet<string> groups, List<string> problems)
        {
            if (list == null || list.Length == 0)
            {
                problems.Add("There is no " + family + " modifier at all.");
                return;
            }

            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < list.Length; i++)
            {
                ItemModifier modifier = list[i];

                if (!ids.Add(modifier.id))
                {
                    problems.Add("The " + family + " modifier " + modifier.id + " appears more than once.");
                }

                if (!groups.Contains(modifier.slots))
                {
                    problems.Add(modifier.id + " falls on " + modifier.slots + ", which is not a slot group.");
                }

                if (modifier.weight <= 0f)
                {
                    problems.Add(modifier.id + " has a weight of " + modifier.weight + ", so it never rolls.");
                }

                if (string.IsNullOrEmpty(modifier.affix))
                {
                    problems.Add(modifier.id + " has no affix word, so it cannot name an item.");
                }

                if (modifier.min == null || modifier.max == null)
                {
                    problems.Add(modifier.id + " has no value range.");
                    continue;
                }

                if (modifier.max.@base < modifier.min.@base || modifier.max.perLevel < modifier.min.perLevel)
                {
                    problems.Add(modifier.id + " has a maximum below its minimum.");
                }
            }
        }

        private static void CheckTiers(ItemTiers tiers, List<string> problems)
        {
            if (tiers == null || tiers.ladder == null || tiers.distribution == null || tiers.technology == null)
            {
                problems.Add("tiers.json could not be read.");
                return;
            }

            LadderStep[] steps = tiers.ladder.steps;

            if (steps == null || steps.Length == 0)
            {
                problems.Add("The tier ladder is empty.");
            }
            else
            {
                for (int i = 0; i < steps.Length; i++)
                {
                    if (steps[i].tier != i + 1)
                    {
                        problems.Add("The tier ladder is out of order at position " + i + ".");
                    }

                    if (i > 0 && steps[i].multiplier <= steps[i - 1].multiplier)
                    {
                        problems.Add("Tier " + steps[i].tier + " is not worth more than the tier below it.");
                    }
                }

                if (steps[0].multiplier != 1f)
                {
                    problems.Add("Tier 1 multiplies by " + steps[0].multiplier
                        + ", but a modifier declares its own tier 1 range, so it has to be 1.");
                }
            }

            TierChances[] rows = tiers.distribution.byItemLevel;

            if (rows == null || rows.Length < 2)
            {
                problems.Add("The tier distribution needs at least two rows to interpolate between.");
            }
            else
            {
                for (int i = 1; i < rows.Length; i++)
                {
                    if (rows[i].itemLevel <= rows[i - 1].itemLevel)
                    {
                        problems.Add("The tier distribution rows are not in rising item level order.");
                    }
                }
            }

            if (tiers.technology.steps == null || tiers.technology.steps.Length == 0)
            {
                problems.Add("There is no technology step.");
                return;
            }

            HashSet<string> technologies = new HashSet<string>();

            for (int i = 0; i < tiers.technology.steps.Length; i++)
            {
                TechnologyStep step = tiers.technology.steps[i];

                if (!technologies.Add(step.id))
                {
                    problems.Add("The technology " + step.id + " appears more than once.");
                }

                if (step.maxModifiers < -1)
                {
                    problems.Add(step.id + " carries " + step.maxModifiers + " modifiers.");
                }
            }

            if (tiers.technology.familyCap < 1)
            {
                problems.Add("The family cap is " + tiers.technology.familyCap + ", so no item may carry a modifier.");
            }
        }

        private static void CheckUniques(ItemUniqueRules uniques, List<string> problems)
        {
            if (uniques == null || uniques.powerTiers == null || uniques.powerTiers.Length == 0)
            {
                problems.Add("uniques.json could not be read, or declares no power tier.");
                return;
            }

            for (int i = 0; i < uniques.powerTiers.Length; i++)
            {
                PowerTier tier = uniques.powerTiers[i];

                if (tier.tier != i + 1)
                {
                    problems.Add("The power tiers are out of order at position " + i + ".");
                }

                if (tier.dropWeight <= 0f)
                {
                    problems.Add("Power tier " + tier.tier + " has a weight of " + tier.dropWeight + ", so it never drops.");
                }

                if (i == 0)
                {
                    continue;
                }

                PowerTier below = uniques.powerTiers[i - 1];

                // The whole point of the table: a stronger unique has to be both rarer and later.
                if (tier.minItemLevel <= below.minItemLevel)
                {
                    problems.Add("Power tier " + tier.tier + " does not start later than the tier below it.");
                }

                if (tier.dropWeight >= below.dropWeight)
                {
                    problems.Add("Power tier " + tier.tier + " is not rarer than the tier below it.");
                }
            }
        }

        private static HashSet<string> Ids(ClassAttributes[] list)
        {
            HashSet<string> ids = new HashSet<string>();

            if (list != null)
            {
                for (int i = 0; i < list.Length; i++)
                {
                    ids.Add(list[i].id);
                }
            }

            return ids;
        }

        private static HashSet<string> Ids(ClassNouns[] list)
        {
            HashSet<string> ids = new HashSet<string>();

            if (list != null)
            {
                for (int i = 0; i < list.Length; i++)
                {
                    ids.Add(list[i].id);
                }
            }

            return ids;
        }
    }
}
