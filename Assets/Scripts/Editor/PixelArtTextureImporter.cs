using UnityEditor;
using UnityEngine;

namespace HerOClock.EditorTools
{
    /// <summary>
    /// Import settings for every drawing under Assets/Art, applied in code instead of by hand.
    ///
    /// Pixel art needs six settings changed away from Unity's defaults, and every one of them
    /// fails silently when forgotten: bilinear filtering blurs the art, compression invents
    /// colours that were never in the palette, and the default 100 pixels per unit makes a
    /// 24x32 sprite land at a third of the size of its cell. None of those raise a warning.
    ///
    /// Doing it here rather than writing the .meta files by hand means the settings survive a
    /// Unity upgrade, and that re-importing or deleting a .meta cannot quietly undo them.
    /// </summary>
    public class PixelArtTextureImporter : AssetPostprocessor
    {
        private const string ArtFolder = "Assets/Art/";

        /// <summary>
        /// Matches the Assets Pixels Per Unit of the Pixel Perfect Camera, which is what makes
        /// one board cell measure 30x30 pixels. The two numbers are the same number and have to
        /// move together: see main-camera.md.
        /// </summary>
        private const float PixelsPerUnit = 30f;

        /// <summary>
        /// The feet sit two pixels above the bottom edge of a 32 pixel tall frame, so the pivot
        /// goes there instead of on the very bottom. Otherwise the character floats two pixels
        /// above the floor of its cell, which is invisible in the Inspector and obvious on screen.
        /// </summary>
        private static readonly Vector2 BottomPivot = new Vector2(0.5f, 2f / 32f);

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            // Custom alignment is the only one that reads spritePivot; every other value
            // ignores it and silently re-centres the sprite.
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = BottomPivot;
            importer.SetTextureSettings(settings);
        }
    }
}
