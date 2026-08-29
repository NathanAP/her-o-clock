using System;

namespace HerOClock.Persistence
{
    /// <summary>
    /// One item as it is written to disk.
    ///
    /// ## Why the item and its whereabouts are separate things
    ///
    /// Every item the player owns lives **once**, in a flat list, and whatever is holding it refers
    /// to it by id. A hero's slot names an id; the chest of 0.14.0.0 will name an id with a tab and
    /// a position beside it.
    ///
    /// Storing the item inside whatever holds it would mean moving it rewrites it, and the same
    /// item could end up existing twice with different values. Referring to it means moving is
    /// changing one line and nothing else.
    ///
    /// ## What is written, and what deliberately is not
    ///
    /// Only the **choices**: the base it came from and the values it rolled. Everything derived —
    /// the base defence, the attribute requirement, the name the player reads — is worked out again
    /// from the tables each time. That is why a change to `slots.json` reaches items that already
    /// exist instead of leaving them frozen with yesterday's arithmetic.
    ///
    /// The name in particular is never stored. It is built from the modifiers, so writing it down
    /// would be creating a second copy of something the item already says.
    /// </summary>
    [Serializable]
    public class ItemSave
    {
        /// <summary>Identity of this exact item, which is what everything else refers to.</summary>
        public string id;

        public string slot;

        /// <summary>Named `itemClass` because `class` is a keyword, and the key has to be a field.</summary>
        public string itemClass;

        public string subtype;

        public string technology;

        public int level = 1;

        public ItemModifierSave[] modifiers = new ItemModifierSave[0];
    }

    /// <summary>
    /// One modifier as it landed: which one, at which tier, and the value that came out.
    ///
    /// The tier travels with the value even though the value already contains it, because the name
    /// of an item is built from the highest tier of each family it carries. Dropping the tier would
    /// mean the name could not be worked out again.
    ///
    /// Two values, because `weaponDamage` rolls a range rather than an amount. On every other
    /// modifier they are the same number, and a file written before this field existed reads
    /// <see cref="valueMax"/> as zero — which is why loading treats anything below
    /// <see cref="value"/> as "no second number" instead of as a range running backwards.
    /// </summary>
    [Serializable]
    public class ItemModifierSave
    {
        public string id;
        public string family;
        public int tier = 1;
        public float value;
        public float valueMax;
    }

    /// <summary>Which item is in which of a hero's slots.</summary>
    [Serializable]
    public class EquippedSave
    {
        public string slot;
        public string itemId;
    }
}
