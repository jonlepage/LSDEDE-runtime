using UnityEditor;
using UnityEngine;

namespace LSDE.Demo.Editor
{
    /// <summary>
    /// Configures the hand-drawn bubble sprite (bubble.png) for 9-slice usage in Unity.
    /// 9-slice means Unity splits the sprite into 9 zones: 4 corners (never stretched),
    /// 4 edges (stretched in one direction), and 1 center (stretched in both directions).
    /// This preserves the hand-drawn outline quality while allowing the bubble to resize.
    ///
    /// Run via LSDE > Configure Bubble Sprite.
    /// </summary>
    public static class BubbleSpriteGenerator
    {
        private const string BubbleSpritePath = "Assets/LSDE/Demo/Sprites/bubble.png";

        // 9-slice borders (in pixels from each edge)
        // These define where the "protected" corner/edge zones end.
        // The tail is at the bottom-right, so bottom and right borders are larger.
        private const float BorderLeft = 150f;
        private const float BorderRight = 300f;
        private const float BorderTop = 150f;
        private const float BorderBottom = 280f;

        [MenuItem("LSDE/Configure Bubble Sprite")]
        public static void ConfigureBubbleSprite()
        {
            var textureImporter = AssetImporter.GetAtPath(BubbleSpritePath) as TextureImporter;

            if (textureImporter == null)
            {
                EditorUtility.DisplayDialog(
                    "LSDE Bubble Config",
                    $"Sprite not found at:\n{BubbleSpritePath}\n\n"
                        + "Place your bubble.png in Assets/LSDE/Demo/Sprites/",
                    "OK"
                );
                return;
            }

            // Configure as Sprite with 9-slice borders
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.mipmapEnabled = false;

            // 9-slice border: Vector4(left, bottom, right, top)
            textureImporter.spriteBorder = new Vector4(
                BorderLeft,
                BorderBottom,
                BorderRight,
                BorderTop
            );

            // Set pivot to bottom-center (so the tail points down from the pivot)
            var textureSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(textureSettings);
            textureSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            textureSettings.spritePivot = new Vector2(0.55f, 0.1f);
            textureImporter.SetTextureSettings(textureSettings);

            textureImporter.SaveAndReimport();

            Debug.Log(
                "[LSDE] Bubble sprite configured with 9-slice borders "
                    + $"(L:{BorderLeft}, R:{BorderRight}, T:{BorderTop}, B:{BorderBottom})"
            );

            EditorUtility.DisplayDialog(
                "LSDE Bubble Config",
                "bubble.png configured for 9-slice!\n\n"
                    + "Now run LSDE > Rebuild All Speech Bubbles to apply.",
                "OK"
            );
        }
    }
}
