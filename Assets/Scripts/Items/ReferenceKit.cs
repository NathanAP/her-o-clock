using System.Collections.Generic;

namespace HerOClock.Items
{
    /// <summary>
    /// A full set of plain gear at a given level, built the same way every time.
    ///
    /// ## Why it exists and why it is deliberately boring
    ///
    /// Nothing drops yet, so there is no way to ask "what does wearing a level 20 set actually do
    /// to a hero" without inventing the set. Invent it at random and the answer changes on every
    /// run, which is no answer at all — a balance number has to come out the same twice.
    ///
    /// So the kit carries **no modifiers**. Only the base defence its slots and class give it.
    /// Picking modifiers here would mean writing a generator, and the generator is a version of its
    /// own with its own weights and its own tiers. Guessing at it now would produce a yardstick
    /// that measures something the game does not do.
    ///
    /// What it answers is the one question that is fully decided today: **how much defence a full
    /// set of level N gear is worth.** The balance snapshot publishes that, and the editor tool
    /// hands it out so a fight can be watched with gear on.
    /// </summary>
    public static class ReferenceKit
    {
        /// <summary>
        /// The slots a plain kit fills.
        ///
        /// The four casings and the off hand carry defence weight; the main hand carries the weapon,
        /// which is what the kit measures on the offensive side. The controller and the firmware are
        /// left out because they carry no base defence at all — they exist for modifiers, and this
        /// kit has none.
        /// </summary>
        public static readonly IReadOnlyList<string> Slots = new List<string>
        {
            "cranialCasing", "chassis", "armServos", "tractionUnits", "mainHand", "offHand"
        };

        /// <summary>
        /// The weapon each class carries, picked as the plainest one handed subtype of its natural
        /// class in `subtypes.json`.
        ///
        /// One handed on purpose. A two hander would empty the off hand, and the kit would stop
        /// measuring the defence it was built to measure — the answer would move for a reason that
        /// has nothing to do with the change being looked at.
        ///
        /// A subtype missing here means that class simply has no weapon in the kit, which is the
        /// honest answer rather than a guess.
        /// </summary>
        private static readonly Dictionary<string, string> WeaponByClass = new Dictionary<string, string>
        {
            { "light", "blade" },
            { "heavy", "ram" },
            { "special", "catalyst" }
        };

        /// <summary>
        /// A kit of one class, so the character's class mix comes out pure and the numbers stay
        /// readable. A mixed kit is a valid thing to want and a bad thing to measure against.
        /// </summary>
        public static List<Item> Of(string classId, int level)
        {
            List<Item> kit = new List<Item>();

            for (int i = 0; i < Slots.Count; i++)
            {
                string subtype = SubtypeFor(Slots[i], classId);

                if (Slots[i] == ItemRules.MainHandSlot && string.IsNullOrEmpty(subtype))
                {
                    continue;
                }

                kit.Add(new Item(
                    "reference-" + classId + "-" + level + "-" + Slots[i],
                    Slots[i],
                    classId,
                    subtype,
                    "noTechnology",
                    level,
                    new List<ItemModifierRoll>()));
            }

            return kit;
        }

        /// <summary>
        /// The subtype a slot needs, which is only the main hand: a casing's subtype is nominal and
        /// a defensive off hand takes its defence from its slot and class rather than from a table.
        /// </summary>
        private static string SubtypeFor(string slotId, string classId)
        {
            string weapon;

            return slotId == ItemRules.MainHandSlot && WeaponByClass.TryGetValue(classId, out weapon)
                ? weapon
                : string.Empty;
        }
    }
}
