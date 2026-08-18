using HerOClock.Common;
using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// A bolt drawn flying from whoever attacked to whoever was hit.
    ///
    /// It is **purely decoration**. The damage was already resolved when the attack happened, and
    /// this only draws the trip afterwards, so nothing here can change the outcome of a fight and
    /// no rule depends on where the bolt is at any moment.
    ///
    /// That is also why it flies to a **position captured at launch** rather than following the
    /// target. A target can die mid flight, and a dead minion is deactivated, so a bolt holding on
    /// to it would be chasing an object that is no longer there. Aiming at the cell the target
    /// stood in also reads better: the shot was thrown at where they were.
    ///
    /// It moves on <c>Time.deltaTime</c>, which <c>Time.timeScale</c> already scales, so it stays
    /// in step with the battle at any of the development speeds without knowing they exist.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private ProjectilePool pool;
        private SpriteRenderer body;

        private Vector3 origin;
        private Vector3 destination;
        private float speed;
        private float maxLifetime;
        private float travelled;
        private float distance;
        private float elapsed;

        /// <summary>Creates the sprite. Called once, by the pool.</summary>
        public void Build(ProjectilePool pool)
        {
            this.pool = pool;

            body = gameObject.AddComponent<SpriteRenderer>();
            body.sprite = SquareSprite.Get();
            // The project reserves a layer of its own for this, above the characters and below
            // the damage numbers, so a bolt passes in front of whoever fired it and never covers
            // the number that says what it did.
            body.sortingLayerName = "Projectiles";
            body.sortingOrder = 0;
        }

        /// <summary>Sends a bolt on its way. Everything it needs is settled here, at launch.</summary>
        public void Launch(Vector3 from, Vector3 to, Color color, ProjectileSettings settings, float cellSize)
        {
            origin = from;
            destination = to;

            distance = Vector3.Distance(from, to);
            speed = Mathf.Max(0.1f, settings.CellsPerSecond) * cellSize;
            maxLifetime = settings.MaxLifetime;

            travelled = 0f;
            elapsed = 0f;

            body.color = color;

            transform.position = from;
            transform.localScale = new Vector3(settings.Size.x * cellSize, settings.Size.y * cellSize, 1f);

            // Pointed along the trip, so a bolt going sideways does not look like it is sliding.
            Vector3 direction = to - from;
            transform.rotation = direction.sqrMagnitude > 0.0001f
                ? Quaternion.FromToRotation(Vector3.right, direction.normalized)
                : Quaternion.identity;

            gameObject.SetActive(true);

            // A shot at somebody standing on the same cell has nowhere to travel. Without this it
            // would sit at zero distance for its whole lifetime before being collected.
            if (distance <= 0.0001f)
            {
                pool.Return(this);
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            travelled += speed * Time.deltaTime;

            // The lifetime is a safety net, not the normal ending. It only matters if the speed is
            // set absurdly low, and it is what keeps a bolt from living forever in a game meant to
            // be left open all day.
            if (travelled >= distance || elapsed >= maxLifetime)
            {
                pool.Return(this);
                return;
            }

            transform.position = Vector3.Lerp(origin, destination, travelled / distance);
        }
    }
}
