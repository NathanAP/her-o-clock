using System;

namespace HerOClock.Items
{
    /// <summary>
    /// `modifiers.json`: everything a non unique item can roll, split into hardware and software
    /// the way Path of Exile splits prefixes from suffixes.
    ///
    /// Each modifier declares only its **tier 1** range. The ladder in `tiers.json` multiplies it
    /// for the tiers above, so moving one number moves the game coherently instead of asking for
    /// five ranges per modifier and no guarantee that one modifier's tier 4 is worth another's.
    /// </summary>
    [Serializable]
    public class ItemModifiers
    {
        public SlotGroup[] slotGroups;
        public ItemModifier[] hardware;
        public ItemModifier[] software;
    }

    /// <summary>
    /// A named set of slots a modifier is allowed to fall on.
    ///
    /// The entries are slot ids, with one exception: `offHandWeapon` and `offHandDefensive` are
    /// the two roles of the `offHand` slot, declared in its `roles` field. The off hand is the
    /// only slot that changes nature depending on what is worn in it, and armour must not land on
    /// a weapon just because the weapon is in the left hand.
    /// </summary>
    [Serializable]
    public class SlotGroup
    {
        public string id;
        public string[] slots;
    }

    [Serializable]
    public class ItemModifier
    {
        public string id;

        /// <summary>Key into the strings file is built from the id; this is the English template.</summary>
        public string text;

        /// <summary>The word this modifier puts into an item's name when it is the highest tier.</summary>
        public string affix;

        /// <summary>Id of a <see cref="SlotGroup"/>.</summary>
        public string slots;

        /// <summary>
        /// Relative chance of being drawn. Very strong modifiers are rare by weight and never by
        /// tier, because no tier is locked behind a level.
        /// </summary>
        public float weight;

        public ScaledValue min;
        public ScaledValue max;
    }
}
