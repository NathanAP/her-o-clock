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
        /// The four casings and the off hand, which are the ones carrying defence weight. The main
        /// hand is left out because a weapon is 0.11.2.0 and a weapon with no rules yet would be an
        /// empty slot with a name. The controller and the firmware are left out because they carry
        /// no base defence at all — they exist for modifiers, and this kit has none.
        /// </summary>
        public static readonly IReadOnlyList<string> Slots = new List<string>
        {
            "cranialCasing", "chassis", "armServos", "tractionUnits", "offHand"
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
                kit.Add(new Item(
                    "reference-" + classId + "-" + level + "-" + Slots[i],
                    Slots[i],
                    classId,
                    string.Empty,
                    "noTechnology",
                    level,
                    new List<ItemModifierRoll>()));
            }

            return kit;
        }
    }
}
