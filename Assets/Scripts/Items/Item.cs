using System;
using System.Collections.Generic;

namespace HerOClock.Items
{
    /// <summary>
    /// One item that exists in the world: a base from the tables plus the values it rolled.
    ///
    /// ## What is stored and what is not
    ///
    /// **A roll is stored. A derivation is not.** The modifier values were decided once, when the
    /// item came into being, and re-rolling them on load would make the item a different item
    /// every time the game opens. Everything computed *from* those choices — the base defence, the
    /// attribute requirement, the name — is worked out again from the tables each time, so a
    /// change to `slots.json` reaches items that already exist instead of leaving them frozen with
    /// yesterday's arithmetic.
    ///
    /// It is the same split the rest of the project uses: the input is written down, the output
    /// never is.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Identity of this exact item, not of its kind.
        ///
        /// Two Blades rolled from the same base are two different items, and the inventory has to
        /// be able to move one without touching the other. It is also what lets the save store an
        /// item once and refer to it from wherever it happens to be.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>Which of the eight slots it goes in.</summary>
        public string SlotId { get; private set; }

        /// <summary>One of the six equipment classes. Decides defence, requirement and the mix.</summary>
        public string ClassId { get; private set; }

        /// <summary>Weapon or off hand subtype, or empty for a casing, whose subtype is nominal.</summary>
        public string SubtypeId { get; private set; }

        public string TechnologyId { get; private set; }

        /// <summary>Between 1 and 100. Everything the item is worth scales with it.</summary>
        public int Level { get; private set; }

        public IReadOnlyList<ItemModifierRoll> Modifiers { get; private set; }

        public Item(
            string id,
            string slotId,
            string classId,
            string subtypeId,
            string technologyId,
            int level,
            IReadOnlyList<ItemModifierRoll> modifiers)
        {
            Id = id;
            SlotId = slotId;
            ClassId = classId;
            SubtypeId = subtypeId;
            TechnologyId = technologyId;
            Level = level < 1 ? 1 : level;
            Modifiers = modifiers ?? new List<ItemModifierRoll>();
        }
    }

    /// <summary>
    /// One modifier as it landed on one item: which modifier, at which tier, and the value that
    /// came out.
    ///
    /// The tier is kept alongside the value even though the value already contains it. The name of
    /// an item is built from the **highest tier** hardware and software it carries, so throwing the
    /// tier away would mean the name could not be worked out again.
    /// </summary>
    [Serializable]
    public class ItemModifierRoll
    {
        public string Id { get; private set; }

        /// <summary>`hardware` or `software`, which is also which half of the name it feeds.</summary>
        public string Family { get; private set; }

        public int Tier { get; private set; }

        public float Value { get; private set; }

        public ItemModifierRoll(string id, string family, int tier, float value)
        {
            Id = id;
            Family = family;
            Tier = tier;
            Value = value;
        }
    }
}
