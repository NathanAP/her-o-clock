using System.Collections.Generic;
using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// Keeps the bolts alive between shots instead of building one per attack.
    ///
    /// Same reasoning as <see cref="DamageNumberPool"/>: a ranged character fires a few times a
    /// second, the game is meant to sit open all day, and every bolt is identical apart from its
    /// colour and where it is going. Creating and collecting one per shot would be a steady drip
    /// of garbage for nothing.
    ///
    /// It is an instance owned by the bootstrap and not a static, so that leaving Play Mode with
    /// Domain Reload disabled cannot leave it holding objects the editor already destroyed.
    /// </summary>
    public class ProjectilePool
    {
        private readonly Transform parent;
        private readonly float cellSize;
        private readonly ProjectileSettings settings;
        private readonly Stack<Projectile> resting = new Stack<Projectile>();

        public ProjectilePool(Transform parent, float cellSize, ProjectileSettings settings)
        {
            this.parent = parent;
            this.cellSize = cellSize;
            this.settings = settings;
        }

        public void Fire(Vector3 from, Vector3 to, Color shooterColor)
        {
            Projectile projectile;

            if (resting.Count > 0)
            {
                projectile = resting.Pop();
            }
            else
            {
                GameObject instance = new GameObject("Projectile");
                instance.transform.SetParent(parent, false);

                projectile = instance.AddComponent<Projectile>();
                projectile.Build(this);
            }

            // Tinted towards the shooter so the player can read who is firing at whom without a
            // legend, which is the same job the body colours already do.
            Color tint = Color.Lerp(Color.white, shooterColor, settings.ColorFromShooter);

            projectile.Launch(from, to, tint, settings, cellSize);
        }

        /// <summary>Called by a bolt once it has arrived, or once its lifetime ran out.</summary>
        public void Return(Projectile projectile)
        {
            if (!projectile.gameObject.activeSelf)
            {
                return;
            }

            projectile.gameObject.SetActive(false);
            resting.Push(projectile);
        }
    }
}
