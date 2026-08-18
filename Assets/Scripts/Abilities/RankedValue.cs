using System;
using UnityEngine;

namespace HerOClock.Abilities
{
    /// <summary>
    /// A number on an ability sheet, which may or may not change with the ability's rank.
    ///
    /// abilities.md allows both shapes: a plain number is the same at every rank, and an array
    /// scales, with index 0 being rank 1. The sheet holds one array either way, and its **length**
    /// is what says which shape it is:
    ///
    /// - one entry means constant;
    /// - as many entries as the ability has ranks means it scales.
    ///
    /// Anything else is invalid, and that is the whole point of modelling it this way. The spec
    /// names the failure it is guarding against: an array one entry short would make the last rank
    /// read a value that does not exist, with no error anywhere. Here it is a length that
    /// <see cref="AbilityValidator"/> can simply compare.
    /// </summary>
    [Serializable]
    public struct RankedValue
    {
        [Tooltip("One entry for a value that never changes, or one per rank to make it scale.")]
        public float[] PerRank;

        public RankedValue(params float[] perRank)
        {
            PerRank = perRank;
        }

        public static RankedValue Constant(float value)
        {
            return new RankedValue(value);
        }

        public bool IsEmpty
        {
            get { return PerRank == null || PerRank.Length == 0; }
        }

        public int Length
        {
            get { return PerRank == null ? 0 : PerRank.Length; }
        }

        /// <summary>
        /// The value at a rank, counting from 1.
        ///
        /// A rank outside the array is clamped rather than allowed to throw. The validator is what
        /// reports a badly sized array; by the time a battle is running, refusing to produce a
        /// number would only turn an authoring mistake into a crash in front of a player.
        /// </summary>
        public float At(int rank)
        {
            if (IsEmpty)
            {
                return 0f;
            }

            if (PerRank.Length == 1)
            {
                return PerRank[0];
            }

            int index = Mathf.Clamp(rank - 1, 0, PerRank.Length - 1);
            return PerRank[index];
        }

        public int IntAt(int rank)
        {
            return Mathf.RoundToInt(At(rank));
        }
    }
}
