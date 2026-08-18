using System;
using HerOClock.Characters;
using UnityEngine;

namespace HerOClock.Abilities
{
    /// <summary>
    /// How much of each of the user's attributes goes into an ability's damage, from the `scaling`
    /// block in abilities.md.
    ///
    /// It is what ties an ability to the build behind it. A damage number that ignored attributes
    /// would make an ability worth the same on a character who invested in it and on one who did
    /// not, and there would be no reason to build towards anything.
    /// </summary>
    [Serializable]
    public class AbilityScaling
    {
        [Min(0f)] public float Power;
        [Min(0f)] public float Agility;
        [Min(0f)] public float Specialty;
        [Min(0f)] public float Constitution;

        /// <summary>What the attributes of the given character add to an ability's base damage.</summary>
        public float AppliedTo(CharacterStats stats)
        {
            if (stats == null)
            {
                return 0f;
            }

            return stats.Power * Power
                + stats.Agility * Agility
                + stats.Specialty * Specialty
                + stats.Constitution * Constitution;
        }
    }
}
