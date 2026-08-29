using System.Collections.Generic;
using HerOClock.Characters;

namespace HerOClock.Items
{
    /// <summary>
    /// Turns what a hero is holding into the <see cref="WeaponLoadout"/> a fight reads.
    ///
    /// It is the offensive twin of <see cref="ItemContribution"/>: `Items` knows what the tables
    /// mean and fills a bag that lives in `Characters`, and `Characters` never learns what an item
    /// is. Same arrow, same direction.
    ///
    /// ## The main hand decides, and an empty main hand means no weapon at all
    ///
    /// Speed and reach come from the main hand and from nowhere else, which `items.md` states.
    /// A weapon alone in the off hand therefore has nothing to take them from, so the hero punches
    /// with the sheet — the off hand only ever adds a second swing to a weapon that is already
    /// there.
    /// </summary>
    public static class WeaponBuilder
    {
        /// <summary>
        /// Builds the loadout from the **active** pieces, or null when the hero is fighting bare.
        ///
        /// Null and not an empty loadout: "wearing no weapon" is the sheet's own punch, and that
        /// answer lives on the sheet. An empty loadout would be a second place claiming to know
        /// what an unarmed character swings for.
        /// </summary>
        public static WeaponLoadout Build(IReadOnlyList<Item> active, ItemRules rules)
        {
            if (active == null || rules == null)
            {
                return null;
            }

            ItemWeapon main = rules.WeaponOf(In(active, ItemRules.MainHandSlot));

            if (!main.Exists)
            {
                return null;
            }

            WeaponLoadout loadout = new WeaponLoadout();

            loadout.UseMainHand(
                main.AttackSpeed, main.MinRange, main.MaxRange,
                main.Ranged ? AutoAttackType.Ranged : AutoAttackType.Melee);

            loadout.AddHand(main.MinDamage, main.MaxDamage);

            // A two handed weapon never shares. The off hand is empty by then anyway, but asking
            // here as well means a set that arrived from a save cannot produce three swings.
            if (main.Hands < 2)
            {
                ItemWeapon off = rules.WeaponOf(In(active, ItemRules.OffHandSlot));

                if (off.Exists)
                {
                    loadout.AddHand(off.MinDamage, off.MaxDamage);
                }
            }

            return loadout;
        }

        private static Item In(IReadOnlyList<Item> items, string slotId)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].SlotId == slotId)
                {
                    return items[i];
                }
            }

            return null;
        }
    }
}
