using System;

namespace HerOClock.Items
{
    /// <summary>
    /// `uniques.json`: the rules every unique item obeys. Each unique itself lives in its own file
    /// under `Assets/Items/Uniques/`.
    ///
    /// The table below exists so that rarity and minimum level are never chosen one by one. A
    /// unique declares a **power tier** and that single number decides both. Choosing them apart
    /// would allow writing, by accident, an item that settles a whole build and drops at level 1.
    /// </summary>
    [Serializable]
    public class ItemUniqueRules
    {
        public PowerTier[] powerTiers;

        /// <summary>The rules in prose, kept beside the numbers they govern.</summary>
        public string[] rules;

        public UniqueDiscovery discovery;

        /// <summary>The row for a tier, or null when the tier does not exist.</summary>
        public PowerTier TierOf(int tier)
        {
            if (powerTiers == null)
            {
                return null;
            }

            for (int i = 0; i < powerTiers.Length; i++)
            {
                if (powerTiers[i].tier == tier)
                {
                    return powerTiers[i];
                }
            }

            return null;
        }
    }

    [Serializable]
    public class PowerTier
    {
        public int tier;

        /// <summary>
        /// The floor an item of this tier may drop at. A unique may declare a level **above** its
        /// tier's floor and never below it: raising is a content decision, lowering would break
        /// the promise that the strongest arrive later.
        /// </summary>
        public int minItemLevel;

        /// <summary>Weight relative to the other uniques. Not the chance of a drop being unique.</summary>
        public float dropWeight;

        /// <summary>What an item of this tier is meant to feel like.</summary>
        public string intent;
    }

    [Serializable]
    public class UniqueDiscovery
    {
        /// <summary>Every `.json` in this folder is a unique, and every unique is in it.</summary>
        public string folder;

        public string[] rules;
    }
}
