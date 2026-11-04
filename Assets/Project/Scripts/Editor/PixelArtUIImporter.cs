using UnityEditor;
using UnityEngine;

public class PixelArtUIImporter : AssetPostprocessor
{
    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // Only images dropped into this folder are set up automatically
    private const string UIFolder = "Assets/Project/Art Pack/UI/";

    /* Void OnPreprocessTexture Method:
     * ~ Unity calls this before importing any image (editor only, never in the build)
     * ~ Gives every NEW image in the UI folder crisp pixel art settings: Sprite, Single, 100 Pixels Per Unit,
     *   Point filtering, no compression, no mipmaps and a Full Rect mesh
     * ~ Only runs the first time a file is imported, so anything changed by hand afterwards
     *   (9 slice borders, Multiple sprite mode for button sheets) is left alone
     */
    private void OnPreprocessTexture()
    {
        // Not a UI image, so it's left as it is
        if (!assetPath.StartsWith(UIFolder))
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;

        // Already set up once, so manual changes are respected
        if (!importer.importSettingsMissing)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        // Matches the Canvas, so 1 art pixel = 1 UI unit
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        // Full Rect is needed for clean 9 slicing
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(settings);
    }
}
