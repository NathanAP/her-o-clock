using System;

namespace HerOClock.Persistence
{
    /// <summary>
    /// One hero's instance state.
    ///
    /// The attribute points are stored the way <see cref="Characters.AttributeAllocation"/> holds
    /// them, and that detail is not cosmetic: the automatic share is a **count of points**, never
    /// the split it produces. Writing the split down would make a restored hero stop matching one
    /// that climbed to the same level while playing, because the percentages are resolved by
    /// largest remainder. Battles are replayable from a seed only while that holds.
    /// </summary>
    [Serializable]
    public class HeroSave
    {
        /// <summary>Id of the sheet this hero came from. A save cannot hold an asset reference.</summary>
        public string id;

        public int level = 1;

        /// <summary>Experience accumulated towards the next level, not the total ever earned.</summary>
        public long currentXp;

        public int skillPoints;

        // There is no health here on purpose. A stage always starts from its beginning with
        // everybody whole, so there is no half-finished stage for a hero to be hurt in.

        public bool automaticAttributes = true;

        /// <summary>How many points the sheet's distribution has spent.</summary>
        public int automaticPoints;

        /// <summary>Points earned and not yet placed.</summary>
        public int unspentPoints;

        /// <summary>Points placed by hand, in attribute order: power, agility, specialty, constitution.</summary>
        public int[] manualPoints = new int[4];

        /// <summary>
        /// Which item is in which slot, by id.
        ///
        /// Only the reference lives here. The items themselves are in the payload's own list, so a
        /// piece moving from a hero to the chest changes this line and nothing else.
        ///
        /// **Whether a piece is active is not saved**, because it is not a fact about the item — it
        /// is worked out from the hero's attributes every time a stage builds its combatant.
        /// Writing it down would be storing a derived value, and it would go stale the moment a
        /// point was moved.
        /// </summary>
        public EquippedSave[] equipment = new EquippedSave[0];
    }
}
