using System;
using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// Proportions of a character inside its cell.
    ///
    /// Values are fractions of one cell, so 1 means the whole cell. They are exposed in the
    /// Inspector so they can be tuned while the game runs.
    ///
    /// The body is an upright rectangle rather than a square because a robot sprite will
    /// hardly ever be square, so the space is born with the right proportion.
    /// </summary>
    [Serializable]
    public class CharacterViewSettings
    {
        [Header("Body")]
        [Tooltip("Rectangle size, in cells, for characters that have no sprite yet.")]
        public Vector2 BodySize = new Vector2(0.5f, 0.7f);
        public float BodyOffsetY = -0.1f;

        [Header("Sprite")]
        [Tooltip("Where the feet of a sprite sit inside the cell, in cells. Minus half a cell "
            + "puts them on the floor line. A sprite is never scaled: its size already comes "
            + "from the pixels per unit of the texture.")]
        public float SpriteOffsetY = -0.5f;

        [Header("Health bar")]
        public Vector2 HealthBarSize = new Vector2(0.8f, 0.1f);
        [Tooltip("A sprite is taller than the rectangle it replaced, so the bar sits higher "
            + "than it used to. Anything below 0.55 overlaps the head of a 32 pixel sprite.")]
        public float HealthBarOffsetY = 0.55f;
        public Color HealthBarBackground = new Color(0.08f, 0.08f, 0.1f, 0.9f);
        public Color HealthBarFull = new Color(0.35f, 0.85f, 0.4f);
        public Color HealthBarEmpty = new Color(0.85f, 0.25f, 0.25f);

        [Header("Damage flash")]
        [Tooltip("Used by the placeholder rectangle, which is a flat colour and can be washed white.")]
        public Color FlashColor = Color.white;

        [Tooltip("Used by a character with art. It cannot be white: a sprite is tinted by "
            + "multiplication, so white is exactly the colour that changes nothing.")]
        public Color SpriteFlashColor = new Color(1f, 0.42f, 0.42f);
        [Min(0.01f)] public float FlashDuration = 0.12f;
    }
}
