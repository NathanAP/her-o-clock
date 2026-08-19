using System;

namespace HerOClock.Persistence
{
    /// <summary>
    /// The team, the bench and how many positions exist, as they go to disk.
    ///
    /// Ids rather than asset references, like every other part of a save.
    ///
    /// An old file has none of this, and it arrives with zero slots and empty arrays. That is the
    /// signal the mapper uses to rebuild it from the heroes the file does carry, so no conversion
    /// step was needed when this was added.
    /// </summary>
    [Serializable]
    public class RosterSave
    {
        /// <summary>Positions the team has. Zero means the file predates the roster.</summary>
        public int slots;

        /// <summary>Every hero the player owns, in the order they arrived.</summary>
        public string[] owned = new string[0];

        /// <summary>Who is fielded, in the order the player chose.</summary>
        public string[] team = new string[0];

        /// <summary>Ids of the stages already cleared once, which is what makes a reward first time only.</summary>
        public string[] clearedStages = new string[0];
    }
}
