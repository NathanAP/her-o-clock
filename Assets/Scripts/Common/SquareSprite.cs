using UnityEngine;

namespace HerOClock.Common
{
    /// <summary>
    /// A white sprite one world unit across, created at runtime.
    ///
    /// While the game runs on flat, untextured game objects, everything on screen is this
    /// square tinted through SpriteRenderer.color. That way the project needs no image files yet.
    /// </summary>
    public static class SquareSprite
    {
        private static Sprite cached;

        public static Sprite Get()
        {
            if (cached != null)
            {
                return cached;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "SquareSpriteTexture";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            // With pixelsPerUnit set to 1 on a 1x1 texture, the sprite measures exactly one
            // world unit. Its final size comes from the object's scale.
            cached = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            cached.name = "SquareSprite";

            return cached;
        }
    }
}
