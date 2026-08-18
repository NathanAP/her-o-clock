using System;
using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// What a basic attack's projectile looks like while the game runs on flat coloured shapes.
    ///
    /// Sizes are fractions of one cell, so 1 means the whole cell, the same reading as
    /// <see cref="CharacterViewSettings"/>. Exposed in the Inspector so it can be tuned with the
    /// game running, which is the only sane way to judge something whose whole job is to look
    /// right.
    /// </summary>
    [Serializable]
    public class ProjectileSettings
    {
        [Tooltip("Size of the bolt, as a fraction of one cell. Longer than it is tall, so it reads "
            + "as travelling rather than as a dot.")]
        public Vector2 Size = new Vector2(0.36f, 0.08f);

        [Tooltip("Cells per second. This is only how fast the bolt is drawn: the damage has already "
            + "been applied when it leaves, so this number changes nothing about the fight.")]
        [Min(0.1f)] public float CellsPerSecond = 14f;

        [Tooltip("Longest a bolt may stay on screen, in seconds. It is a safety net for a shot "
            + "crossing the whole board, never the normal way one ends.")]
        [Min(0.05f)] public float MaxLifetime = 1.5f;

        [Tooltip("How much of the shooter's colour the bolt keeps. 0 is white, 1 is the exact "
            + "colour of the body that fired it.")]
        [Range(0f, 1f)] public float ColorFromShooter = 0.65f;
    }
}
