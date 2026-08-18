using System;
using UnityEngine;

namespace HerOClock.Abilities
{
    /// <summary>
    /// One ability on a character sheet, mirroring the shape described in abilities.md.
    ///
    /// An ability is never a behaviour of its own. It is a combination of reusable pieces, and
    /// what belongs to the character is the numbers and which pieces it chose. That is what keeps
    /// a striking ability from needing a rule only it uses.
    ///
    /// Every time here is in seconds, and every number that scales with the rank is a
    /// <see cref="RankedValue"/>.
    /// </summary>
    [Serializable]
    public class AbilityDefinition
    {
        [Tooltip("Stable text id, used by the strings file and by anything that refers to this ability.")]
        public string Id = "";

        [Tooltip("How many ranks the ability has. Every scaling array has to be exactly this long.")]
        [Min(1)] public int Ranks = 1;

        [Header("Timing, in seconds")]
        [Tooltip("Time the character spends winding up. It stands still and cannot act.")]
        public RankedValue Preparation;

        [Tooltip("Time the ability takes to happen. The character stands still and cannot act.")]
        public RankedValue Casting;

        [Tooltip("Time the character spends recovering afterwards. It stands still and cannot act.")]
        public RankedValue Recoil;

        [Tooltip("Time before the ability can be used again, counted from the moment the casting ended.")]
        public RankedValue Cooldown;

        [Header("What it targets")]
        public AbilityTargeting Targeting = new AbilityTargeting();

        [Header("What it does, resolved in this order")]
        public AbilityEffect[] Effects = new AbilityEffect[0];

        /// <summary>
        /// How long the character is occupied by this ability, from the first frame of the wind up
        /// to the last of the recovery.
        ///
        /// The three phases are one block: whatever comes next, ability or basic attack, waits for
        /// all of it.
        /// </summary>
        public float BusySeconds(int rank)
        {
            return Preparation.At(rank) + Casting.At(rank) + Recoil.At(rank);
        }

        /// <summary>
        /// How long until the cooldown starts running, which is the moment the casting ends.
        ///
        /// The recovery is deliberately outside this sum, and it is the only difference from
        /// <see cref="BusySeconds"/>. An ability with a long recovery frees the next ability late
        /// but has already begun recharging.
        /// </summary>
        public float SecondsUntilCooldownStarts(int rank)
        {
            return Preparation.At(rank) + Casting.At(rank);
        }
    }
}
