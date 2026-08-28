using System;
using System.Collections.Generic;
using HerOClock.Characters;
using Attribute = HerOClock.Characters.Attribute;

namespace HerOClock.Items
{
    /// <summary>
    /// Folds one item into a character's totals: the defence its base grants, plus every modifier
    /// it rolled.
    ///
    /// It is the only place that knows what a modifier id means. Everything else moves items
    /// around without ever asking what they do.
    /// </summary>
    public static class ItemContribution
    {
        /// <summary>
        /// Modifiers this version knows how to apply.
        ///
        /// Anything outside this list is **not** silently dropped: <see cref="Deferred"/> holds what
        /// is waiting for a seam that does not exist yet, and a test asserts that every modifier in
        /// the data is on one list or the other. A modifier added tomorrow that nobody wires up
        /// then fails a test, instead of quietly doing nothing on every item that rolls it.
        /// </summary>
        public static readonly IReadOnlyList<string> Handled = new List<string>
        {
            "pow", "agi", "spe", "con",
            "life",
            "fireResistance", "waterResistance", "electricResistance", "allResistance",
            "localArmour", "localEvasion",
            "thorns"
        };

        /// <summary>
        /// Modifiers that exist in the data and have nowhere to land yet.
        ///
        /// - The four ability damage percentages and the ignored resistance need seams that do not
        ///   exist: `AbilityResolver` runs through no modifier at all, and `DamageInput` has no
        ///   attacker side field for penetration. They arrive in 0.11.1.1.
        /// - Weapon damage belongs with the weapon replacing the character's own swing, in 0.11.2.0.
        /// </summary>
        public static readonly IReadOnlyList<string> Deferred = new List<string>
        {
            "fireAbilityDamage", "waterAbilityDamage", "electricAbilityDamage", "physicalAbilityDamage",
            "resistanceIgnored",
            "weaponDamage"
        };

        public static void AddTo(EquipmentTotals totals, Item item, ItemDefence defence)
        {
            if (totals == null)
            {
                return;
            }

            totals.AddArmour(Round(defence.Armour));
            totals.AddEvasion(Round(defence.Evasion));

            // A special class defends against all three elements at once, which is exactly why its
            // budget is lower than the armour one in slots.json.
            totals.AddAllResistances(Round(defence.ElementalResistance));

            if (item == null || item.Modifiers == null)
            {
                return;
            }

            for (int i = 0; i < item.Modifiers.Count; i++)
            {
                Apply(totals, item.Modifiers[i]);
            }
        }

        private static void Apply(EquipmentTotals totals, ItemModifierRoll roll)
        {
            switch (roll.Id)
            {
                case "pow": totals.AddAttribute(Attribute.Power, Round(roll.Value)); break;
                case "agi": totals.AddAttribute(Attribute.Agility, Round(roll.Value)); break;
                case "spe": totals.AddAttribute(Attribute.Specialty, Round(roll.Value)); break;
                case "con": totals.AddAttribute(Attribute.Constitution, Round(roll.Value)); break;

                case "life": totals.AddLife(Round(roll.Value)); break;

                case "fireResistance": totals.AddFireResistance(Round(roll.Value)); break;
                case "waterResistance": totals.AddWaterResistance(Round(roll.Value)); break;
                case "electricResistance": totals.AddElectricResistance(Round(roll.Value)); break;
                case "allResistance": totals.AddAllResistances(Round(roll.Value)); break;

                // "on this item" is a plain sum onto that item's own defence. There is no local
                // multiplier anywhere in the game, so what those words really carry is the slot
                // restriction, and that is what stops a Blade rolling armour.
                case "localArmour": totals.AddArmour(Round(roll.Value)); break;
                case "localEvasion": totals.AddEvasion(Round(roll.Value)); break;

                case "thorns": totals.AddThornsPercent(roll.Value); break;
            }
        }

        /// <summary>
        /// Rounds the way attributes.md does: nearest integer, halves to the even one.
        ///
        /// It is the language's own rounding, and using anything else here would make an item
        /// disagree with every other number in the game.
        /// </summary>
        private static int Round(float value)
        {
            return (int)Math.Round(value, MidpointRounding.ToEven);
        }
    }
}
