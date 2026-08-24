using System;
using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// The drawings of one character, one per direction.
    ///
    /// Every field may be left empty, and a character with nothing filled in falls back to the
    /// flat coloured rectangle the game used before any art existed. That is what lets a single
    /// hero get a sprite without the rest of the cast having to get one on the same day.
    ///
    /// There is no sprite for <see cref="Facing.Left"/>: it is <see cref="Side"/> mirrored.
    /// A robot with a painted shoulder marking is not truly symmetric, so mirroring is a small
    /// lie — but it is a lie that costs one field instead of one more drawing per character.
    /// </summary>
    [Serializable]
    public class CharacterSprites
    {
        [Tooltip("Turned towards the bottom of the board. The player sees its front.")]
        public Sprite Down;

        [Tooltip("Turned towards the top of the board. The player sees its back.")]
        public Sprite Up;

        [Tooltip("Seen in profile, facing right. Mirrored for the left.")]
        public Sprite Side;

        [Tooltip("Fallen. Optional: without it a dead character just dims.")]
        public Sprite Dead;

        /// <summary>Whether this character has any art at all.</summary>
        public bool HasAny
        {
            get { return Down != null || Up != null || Side != null; }
        }

        /// <summary>
        /// The drawing for a direction, or null when this character has no art.
        ///
        /// A missing direction falls back to <see cref="Down"/> rather than to nothing, because
        /// a character that vanishes when it turns is worse than one facing the wrong way.
        /// </summary>
        public Sprite For(Facing facing)
        {
            switch (facing)
            {
                case Facing.Up:
                    return Up != null ? Up : Down;

                case Facing.Left:
                case Facing.Right:
                    return Side != null ? Side : Down;

                default:
                    return Down;
            }
        }

        /// <summary>Whether the drawing has to be mirrored to face this way.</summary>
        public static bool IsMirrored(Facing facing)
        {
            return facing == Facing.Left;
        }
    }
}
