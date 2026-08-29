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
        /// Modifiers that land in the shared bag of numbers a character carries.
        ///
        /// Anything outside the three lists is **not** silently dropped: <see cref="HandledByWeapon"/>
        /// holds what belongs to one weapon rather than to the character, <see cref="Deferred"/>
        /// holds what is waiting for a seam that does not exist yet, and a test asserts that every
        /// modifier in the data is on one of them. A modifier added tomorrow that nobody wires up
        /// then fails a test, instead of quietly doing nothing on every item that rolls it.
        /// </summary>
        public static readonly IReadOnlyList<string> Handled = new List<string>
        {
            "pow", "agi", "spe", "con",
            "life",
            "fireResistance", "waterResistance", "electricResistance", "allResistance",
            "localArmour", "localEvasion",
            "thorns",
            "resistanceIgnored",
            "fireAbilityDamage", "waterAbilityDamage", "electricAbilityDamage", "physicalAbilityDamage"
        };

        /// <summary>
        /// Modifiers that belong to the weapon carrying them, and never to the character.
        ///
        /// `weaponDamage` is local in the same sense `localArmour` and `localEvasion` are: it raises
        /// the base damage of **that** weapon and nothing else. With two weapons held, that is the
        /// difference between an off hand being a real choice and it being strictly better — a
        /// character wide modifier on the off hand would raise the main hand's swing too.
        ///
        /// It cannot go through <see cref="AddTo"/> at all, because that fills one bag shared by
        /// everything worn, and this number has to stay attached to one hand.
        /// </summary>
        public static readonly IReadOnlyList<string> HandledByWeapon = new List<string>
        {
            "weaponDamage"
        };

        /// <summary>
        /// Modifiers that exist in the data and have nowhere to land yet.
        ///
        /// **Empty, and that is the point.** Every modifier in `modifiers.json` now reaches the
        /// game. The list stays because the test that keeps it honest reads all three, and because
        /// the next modifier that needs a seam has somewhere to wait where nobody can forget it.
        /// </summary>
        public static readonly IReadOnlyList<string> Deferred = new List<string>();

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

        /// <summary>
        /// Folds an item's own local modifiers into what that item swings for.
        ///
        /// Only the weapon holding the modifier is touched, which is why this takes one weapon and
        /// not the character. `weaponDamage` adds to the **base**, before POW multiplies, exactly
        /// where the subtype's own range sits — a modifier that landed after POW would be worth
        /// more on a hero who never invested in it.
        /// </summary>
        public static void AddToWeapon(ref ItemWeapon weapon, Item item)
        {
            if (!weapon.Exists || item == null || item.Modifiers == null)
            {
                return;
            }

            for (int i = 0; i < item.Modifiers.Count; i++)
            {
                ItemModifierRoll roll = item.Modifiers[i];

                if (roll.Id == "weaponDamage")
                {
                    weapon.MinDamage += roll.Value;
                    weapon.MaxDamage += roll.ValueMax;
                }
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

                // Percentages are not rounded on the way in. They multiply a base later, and
                // rounding a 4.5 to a 4 here would quietly throw away an eighth of the modifier.
                case "resistanceIgnored": totals.AddResistanceIgnoredPercent(roll.Value); break;

                case "fireAbilityDamage": totals.AddFireAbilityDamagePercent(roll.Value); break;
                case "waterAbilityDamage": totals.AddWaterAbilityDamagePercent(roll.Value); break;
                case "electricAbilityDamage": totals.AddElectricAbilityDamagePercent(roll.Value); break;
                case "physicalAbilityDamage": totals.AddPhysicalAbilityDamagePercent(roll.Value); break;
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
