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

        /// <summary>
        /// How much this ability's base damage is raised by the user's attributes, as a multiplier.
        ///
        /// The numbers here are **weights on the standard rate**, and not flat damage per point. A
        /// weight of 1 means the attribute pays the full ten points for one percent that
        /// attributes.md gives; 0.5 means half of it. An ability that scales with nothing returns 1
        /// and is worth exactly what its rank says.
        ///
        /// It reads the same way as a weapon and POW, on purpose: the content gives the base — here
        /// the rank — and the attribute multiplies it. Adding flat damage per point instead would
        /// make the rank stop mattering once a character has enough points, which is the same
        /// failure a flat weapon bonus causes.
        /// </summary>
        public float MultiplierFor(CharacterStats stats)
        {
            if (stats == null)
            {
                return 1f;
            }

            float share = stats.Power * Power
                + stats.Agility * Agility
                + stats.Specialty * Specialty
                + stats.Constitution * Constitution;

            return 1f + share * CharacterStats.DamageSharePerPoint;
        }
    }
}
