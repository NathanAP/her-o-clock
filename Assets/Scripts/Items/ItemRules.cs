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
        /// <summary>
        /// The two slots the hand rules talk about.
        ///
        /// They are consts because three different rules name them — a two handed weapon emptying
        /// the off hand, the resolution refusing an off hand it cannot use, and the loadout reading
        /// speed and reach off the main hand. A typo in one of the three would be a rule that
        /// silently stops applying.
        /// </summary>
        public const string MainHandSlot = "mainHand";

        /// <inheritdoc cref="MainHandSlot"/>
        public const string OffHandSlot = "offHand";

        private readonly Dictionary<string, ItemSlot> slots = new Dictionary<string, ItemSlot>();
        private readonly Dictionary<string, ClassDefence> classDefence = new Dictionary<string, ClassDefence>();
        private readonly Dictionary<string, string[]> classAttributes = new Dictionary<string, string[]>();
        private readonly Dictionary<string, WeaponSubtype> weapons = new Dictionary<string, WeaponSubtype>();

        private readonly DefenceBudget budget;
        private readonly SlotRequirement requirement;

        /// <summary>
        /// The subtype table is optional, and its absence means one thing only: nobody is holding a
        /// weapon. A test about the requirement of a casing has no business loading `subtypes.json`,
        /// and a character built without it simply punches with the range on its own sheet.
        /// </summary>
        public ItemRules(ItemSlots table, ItemSubtypes subtypes = null)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            budget = table.defenceBudget;
            requirement = table.requirement;

            if (subtypes != null && subtypes.weapons != null)
            {
                for (int i = 0; i < subtypes.weapons.Length; i++)
                {
                    weapons[subtypes.weapons[i].id] = subtypes.weapons[i];
                }
            }

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

        /// <summary>
        /// What an item swings for, or an <see cref="ItemWeapon"/> that does not exist when the item
        /// is not a weapon at all.
        ///
        /// The three numbers a weapon replaces are read straight off the subtype and scaled by the
        /// **item's** level, never by the character's. That is the whole difference between a weapon
        /// and a sheet: a level 1 hero holding a level 60 Cannon hits like a level 60 Cannon.
        ///
        /// The local `weaponDamage` modifier is folded in here, through
        /// <see cref="ItemContribution"/>, because that stays the only place that knows what a
        /// modifier id means.
        /// </summary>
        public ItemWeapon WeaponOf(Item item)
        {
            ItemWeapon weapon = new ItemWeapon();

            if (item == null)
            {
                return weapon;
            }

            WeaponSubtype subtype;

            if (!weapons.TryGetValue(item.SubtypeId ?? string.Empty, out subtype))
            {
                return weapon;
            }

            weapon.Exists = true;
            weapon.Hands = subtype.hands;
            weapon.MinRange = subtype.minRange;
            weapon.MaxRange = subtype.maxRange;
            weapon.AttackSpeed = subtype.attackSpeed;

            if (subtype.damage != null)
            {
                weapon.MinDamage = subtype.damage.min != null ? subtype.damage.min.At(item.Level) : 0f;
                weapon.MaxDamage = subtype.damage.max != null ? subtype.damage.max.At(item.Level) : 0f;
            }

            // A bolt is drawn for the two ranged families and for nobody else. A Lance reaching two
            // cells is still somebody swinging a lance from further away, which is exactly what
            // `AutoAttackType` has always meant: reach is decided by the ranges, not by the drawing.
            weapon.Ranged = subtype.family != null && subtype.family.Contains("Ranged");

            ItemContribution.AddToWeapon(ref weapon, item);

            return weapon;
        }

        /// <summary>
        /// Whether this item takes both hands, which is what empties the off hand.
        ///
        /// False for everything that is not a weapon, including the defensive off hands: nothing
        /// but a weapon can ever occupy two slots at once.
        /// </summary>
        public bool IsTwoHanded(Item item)
        {
            ItemWeapon weapon = WeaponOf(item);
            return weapon.Exists && weapon.Hands >= 2;
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

    /// <summary>
    /// What a weapon replaces on whoever holds it: the damage range, the speed and the reach.
    ///
    /// <see cref="Exists"/> is the whole answer to "is this a weapon". A casing, a controller and a
    /// defensive off hand all come back with it false, and the caller falls through to the sheet.
    /// </summary>
    public struct ItemWeapon
    {
        public bool Exists;

        /// <summary>1 or 2. A two handed weapon empties the off hand.</summary>
        public int Hands;

        public float MinDamage;

        public float MaxDamage;

        /// <summary>Attacks per second before AGI multiplies it.</summary>
        public float AttackSpeed;

        public int MinRange;

        public int MaxRange;

        /// <summary>Whether the swing draws a bolt. Visual only.</summary>
        public bool Ranged;
    }
}
