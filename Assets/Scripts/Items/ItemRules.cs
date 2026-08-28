using System;
using System.Collections.Generic;
using HerOClock.Characters;
using Attribute = HerOClock.Characters.Attribute;

namespace HerOClock.Items
{
    /// <summary>
    /// The item tables, indexed and asked questions instead of read.
    ///
    /// The wire types loaded from `Assets/Items/` are arrays, because that is what a file is. This
    /// turns them into the lookups the game actually needs, once, and answers the three questions
    /// that everything else asks: what does this item defend, what does it demand, and which of
    /// the three character classes does it count towards.
    ///
    /// It knows nothing about Unity, so all of it can be tested outside the editor.
    /// </summary>
    public class ItemRules
    {
        private readonly Dictionary<string, ItemSlot> slots = new Dictionary<string, ItemSlot>();
        private readonly Dictionary<string, ClassDefence> classDefence = new Dictionary<string, ClassDefence>();
        private readonly Dictionary<string, string[]> classAttributes = new Dictionary<string, string[]>();

        private readonly DefenceBudget budget;
        private readonly SlotRequirement requirement;

        public ItemRules(ItemSlots table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            budget = table.defenceBudget;
            requirement = table.requirement;

            for (int i = 0; i < table.slots.Length; i++)
            {
                slots[table.slots[i].id] = table.slots[i];
            }

            for (int i = 0; i < table.classDefence.Length; i++)
            {
                classDefence[table.classDefence[i].id] = table.classDefence[i];
            }

            if (table.requirement != null && table.requirement.attributeByClass != null)
            {
                for (int i = 0; i < table.requirement.attributeByClass.Length; i++)
                {
                    ClassAttributes entry = table.requirement.attributeByClass[i];
                    classAttributes[entry.id] = entry.attributes;
                }
            }
        }

        /// <summary>The defence an item grants before any modifier, from its slot, class and level.</summary>
        public ItemDefence DefenceOf(Item item)
        {
            ItemDefence defence = new ItemDefence();

            if (item == null)
            {
                return defence;
            }

            ItemSlot slot;
            ClassDefence share;

            if (!slots.TryGetValue(item.SlotId, out slot) || !classDefence.TryGetValue(item.ClassId, out share))
            {
                return defence;
            }

            float weight = slot.defenceWeight;

            if (weight <= 0f)
            {
                // A weapon and the two utility slots carry no base defence at all. They exist to
                // carry modifiers, which is what makes a Controller worth wearing.
                return defence;
            }

            defence.Armour = budget.armour.At(item.Level) * weight * share.armour;
            defence.Evasion = budget.evasion.At(item.Level) * weight * share.evasion;
            defence.ElementalResistance =
                budget.elementalResistance.At(item.Level) * weight * share.elementalResistance;

            return defence;
        }

        /// <summary>
        /// What an item demands to stay active, as an amount per attribute.
        ///
        /// A pure class demands the whole value of one attribute. A hybrid demands a share of each
        /// of two, so wearing hybrid costs more in total and less on each side — which is what
        /// makes the choice exist rather than being strictly worse.
        ///
        /// Rounded up, because a requirement of 1.5 that a character with 1 point could meet would
        /// not be a requirement.
        /// </summary>
        public Dictionary<Attribute, int> RequirementOf(Item item)
        {
            Dictionary<Attribute, int> demanded = new Dictionary<Attribute, int>();

            if (item == null || requirement == null)
            {
                return demanded;
            }

            string[] attributes;

            if (!classAttributes.TryGetValue(item.ClassId, out attributes) || attributes.Length == 0)
            {
                return demanded;
            }

            float full = requirement.perItemLevel * item.Level;
            float each = attributes.Length > 1 ? full * requirement.hybridShare : full;

            for (int i = 0; i < attributes.Length; i++)
            {
                Attribute attribute;

                if (!TryReadAttribute(attributes[i], out attribute))
                {
                    continue;
                }

                demanded[attribute] = (int)Math.Ceiling(each);
            }

            return demanded;
        }

        /// <summary>
        /// The slices an item hands to the three character classes, following "A classe do
        /// personagem" in attributes.md.
        ///
        /// A hybrid hands half to each of its two sides, which is read straight off the defence
        /// table rather than written down a second time: a class that splits its defence between
        /// two is a class that is half of each.
        /// </summary>
        public void AddClassSlicesOf(Item item, List<ItemClass> into)
        {
            if (item == null || into == null)
            {
                return;
            }

            ItemClass parsed;

            if (TryReadClass(item.ClassId, out parsed))
            {
                into.Add(parsed);
            }
        }

        public bool KnowsSlot(string slotId)
        {
            return slots.ContainsKey(slotId);
        }

        private static bool TryReadAttribute(string id, out Attribute attribute)
        {
            switch (id)
            {
                case "pow": attribute = Attribute.Power; return true;
                case "agi": attribute = Attribute.Agility; return true;
                case "spe": attribute = Attribute.Specialty; return true;
                case "con": attribute = Attribute.Constitution; return true;
                default: attribute = Attribute.Power; return false;
            }
        }

        /// <summary>
        /// The six class ids as the enum the mixing rule works in.
        ///
        /// The ids come from the data and the enum comes from the code, so this is the one place
        /// the two vocabularies meet. A class added to the data without a value here is refused
        /// rather than silently treated as light.
        /// </summary>
        public static bool TryReadClass(string id, out ItemClass parsed)
        {
            switch (id)
            {
                case "light": parsed = ItemClass.Light; return true;
                case "special": parsed = ItemClass.Special; return true;
                case "heavy": parsed = ItemClass.Heavy; return true;
                case "medium": parsed = ItemClass.Medium; return true;
                case "lightSpecial": parsed = ItemClass.LightSpecial; return true;
                case "heavySpecial": parsed = ItemClass.HeavySpecial; return true;
                default: parsed = ItemClass.Light; return false;
            }
        }
    }

    /// <summary>The three defences an item's base grants, before any modifier.</summary>
    public struct ItemDefence
    {
        public float Armour;
        public float Evasion;
        public float ElementalResistance;
    }
}
