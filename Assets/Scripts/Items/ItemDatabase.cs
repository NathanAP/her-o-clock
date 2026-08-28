using UnityEngine;

namespace HerOClock.Items
{
    /// <summary>
    /// Every item data file the game knows about.
    ///
    /// The files are referenced as TextAssets rather than loaded by name from a Resources folder,
    /// so renaming or moving one never breaks the reference. It is the same arrangement
    /// <see cref="Stages.StageDatabase"/> uses, and for the same reason.
    ///
    /// **Nothing is cached here.** This project runs with Domain Reload disabled, so a field on a
    /// ScriptableObject keeps its value across Play sessions forever; a lazily built cache would
    /// be permanent, and one built during a bad session would stay bad. Treat the asset as read
    /// only data and hold what you load somewhere that dies with the session.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Her-o-clock/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Tooltip("slots.json — the eight slots, the defence budget and the attribute requirement.")]
        public TextAsset Slots;

        [Tooltip("subtypes.json — weapon subtypes and the three defensive off hands.")]
        public TextAsset Subtypes;

        [Tooltip("modifiers.json — everything a non unique item can roll.")]
        public TextAsset Modifiers;

        [Tooltip("tiers.json — the tier ladder, its distribution, and the technology table.")]
        public TextAsset Tiers;

        [Tooltip("uniques.json — the rules every unique obeys, including the power tier table.")]
        public TextAsset Uniques;

        [Tooltip("itemStrings.json — every word of item text the player reads.")]
        public TextAsset Strings;

        public ItemSlots LoadSlots()
        {
            return Read<ItemSlots>(Slots, "slots");
        }

        public ItemSubtypes LoadSubtypes()
        {
            return Read<ItemSubtypes>(Subtypes, "subtypes");
        }

        public ItemModifiers LoadModifiers()
        {
            return Read<ItemModifiers>(Modifiers, "modifiers");
        }

        public ItemTiers LoadTiers()
        {
            return Read<ItemTiers>(Tiers, "tiers");
        }

        public ItemUniqueRules LoadUniqueRules()
        {
            return Read<ItemUniqueRules>(Uniques, "uniques");
        }

        /// <summary>
        /// Reads one file into memory. Returns null and says why when it cannot, rather than
        /// handing back an empty object that would look like content with nothing in it.
        /// </summary>
        private T Read<T>(TextAsset file, string name) where T : class
        {
            if (file == null)
            {
                Debug.LogError("ItemDatabase: the entry for " + name + " is empty.", this);
                return null;
            }

            T data = JsonUtility.FromJson<T>(file.text);

            if (data == null)
            {
                Debug.LogError("ItemDatabase: " + name + " could not be read as " + typeof(T).Name + ".", this);
            }

            return data;
        }
    }
}
