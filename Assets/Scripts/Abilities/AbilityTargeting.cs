using System;
using UnityEngine;

namespace HerOClock.Abilities
{
    /// <summary>
    /// The `targeting` block of an ability: who it wants, in what form, and by which rule.
    ///
    /// Kept apart from the effects on purpose. Targeting decides **where** the ability happens;
    /// the effects decide **what** happens there. Splitting them is what lets one ability damage
    /// the enemies it found and buff the character that used it.
    /// </summary>
    [Serializable]
    public class AbilityTargeting
    {
        public AbilityWho Who = AbilityWho.Enemies;

        public AbilityShape Shape = AbilityShape.Single;

        [Tooltip("Which rule heads the choice. The rest of the standard chain breaks ties underneath it.")]
        public TargetPriority Priority = TargetPriority.Nearest;

        [Tooltip("Longest distance, in cells, between the user and the target. No relation to the basic attack's range.")]
        [Min(0)] public int Range = 1;

        [Header("Area")]
        [Min(1)] public int AreaColumns = 1;
        [Min(1)] public int AreaRows = 1;

        [Header("Chain")]
        [Tooltip("How many targets the sequence reaches at most. It hits fewer when there are not enough enemies.")]
        public RankedValue MaxTargets;

        [Tooltip("Longest distance between one target and the next.")]
        [Min(0)] public int JumpRange = 1;

        /// <summary>
        /// True for the forms that actually pick somebody, which are the only ones a priority means
        /// anything for. `self` has nobody to choose, and an area is centred on the user.
        /// </summary>
        public bool ChoosesATarget
        {
            get { return Shape == AbilityShape.Single || Shape == AbilityShape.Chain || Shape == AbilityShape.Line; }
        }
    }
}
