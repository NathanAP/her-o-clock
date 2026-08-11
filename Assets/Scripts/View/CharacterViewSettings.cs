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
        public Vector2 BodySize = new Vector2(0.5f, 0.7f);
        public float BodyOffsetY = -0.1f;

        [Header("Health bar")]
        public Vector2 HealthBarSize = new Vector2(0.8f, 0.1f);
        public float HealthBarOffsetY = 0.4f;
        public Color HealthBarBackground = new Color(0.08f, 0.08f, 0.1f, 0.9f);
        public Color HealthBarFull = new Color(0.35f, 0.85f, 0.4f);
        public Color HealthBarEmpty = new Color(0.85f, 0.25f, 0.25f);

        [Header("Damage flash")]
        public Color FlashColor = Color.white;
        [Min(0.01f)] public float FlashDuration = 0.12f;
    }
}
