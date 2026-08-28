using System;

namespace HerOClock.Items
{
    /// <summary>
    /// `tiers.json`: two independent things that are kept apart on purpose.
    ///
    /// The **ladder** says what a tier is worth. The **distribution** says how often it shows up.
    /// Separating them allows balancing generosity without touching rarity, and the other way
    /// round.
    ///
    /// The **technology** block is a third thing again: it decides only how *many* modifiers an
    /// item carries, never how good they are. That is why a Conventional item with two tier 5
    /// modifiers can beat a Quantum one with six tier 1.
    /// </summary>
    [Serializable]
    public class ItemTiers
    {
        public TierLadder ladder;
        public TierDistribution distribution;
        public TechnologyTable technology;
    }

    [Serializable]
    public class TierLadder
    {
        public LadderStep[] steps;
    }

    [Serializable]
    public class LadderStep
    {
        public int tier;
        public float multiplier;
    }

    [Serializable]
    public class TierDistribution
    {
        /// <summary>How to read between two rows. Only `linear` exists.</summary>
        public string interpolation;

        public TierChances[] byItemLevel;

        /// <summary>
        /// The rules this table has to satisfy, written in the file so a test can execute them
        /// rather than a reader having to trust them.
        /// </summary>
        public string[] invariants;
    }

    /// <summary>
    /// The chance of each tier at one item level, in percent.
    ///
    /// **No tier is locked behind a level.** An item of level 1 can roll tier 5, and that small
    /// lottery is deliberate: it is the only thing that lets a drop in act 1 surprise anybody.
    /// </summary>
    [Serializable]
    public class TierChances
    {
        public int itemLevel;
        public float tier1;
        public float tier2;
        public float tier3;
        public float tier4;
        public float tier5;

        public float Total
        {
            get { return tier1 + tier2 + tier3 + tier4 + tier5; }
        }

        public float Of(int tier)
        {
            switch (tier)
            {
                case 1: return tier1;
                case 2: return tier2;
                case 3: return tier3;
                case 4: return tier4;
                default: return tier5;
            }
        }
    }

    [Serializable]
    public class TechnologyTable
    {
        /// <summary>
        /// Chance of rolling one fewer modifier than the maximum, so two items of the same
        /// technology are not always the same size.
        /// </summary>
        public float lowerCountChance;

        public TechnologyStep[] steps;

        /// <summary>Most modifiers of one family an item may carry. Three, from items.md.</summary>
        public int familyCap;
    }

    [Serializable]
    public class TechnologyStep
    {
        public string id;

        /// <summary>
        /// How many modifiers this technology rolls at most.
        ///
        /// **-1 means the technology does not go through the generator at all**, and never that it
        /// carries none. That is the unique, which brings its list written by hand. Without the
        /// distinction it would read as `noTechnology`, which carries zero on purpose.
        /// </summary>
        public int maxModifiers;
    }
}
