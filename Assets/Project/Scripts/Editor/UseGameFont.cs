using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UseGameFont
{
    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // The pixel font file (rainyhearts by camellina)
    private const string FontPath = "Assets/Project/Art Pack/Fonts/rainyhearts.ttf";
    // The TextMeshPro font asset made from it (TextMeshPro names it "<font> SDF")
    private const string FontAssetPath = "Assets/Project/Art Pack/Fonts/rainyhearts SDF.asset";
    // Every prefab in here gets the new font
    private const string PrefabFolder = "Assets/Project/Prefabs";
    // Every scene the game actually uses gets the new font
    private static readonly string[] ScenePaths =
    {
        "Assets/Project/Scenes/Title.unity",
        "Assets/Project/Scenes/Village.unity"
    };

    /* Void UseRainyhearts Method:
     * ~ Editor only menu item: Tools > Carrot's Mail Trail > Use Rainyhearts Font
     * ~ Makes the TextMeshPro font asset (the first time only), makes it the default for new text
     *   (like the post office prompt, which is built in code), then swaps every text in the prefabs and scenes over to it
     * ~ Saves as it goes, then reopens the scene that was open before
     */
    [MenuItem("Tools/Carrot's Mail Trail/Use Rainyhearts Font")]
    private static void UseRainyhearts()
    {
        // Lets you save anything unsaved first, since this opens and saves scenes
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        TMP_FontAsset fontAsset = GetOrCreateFontAsset();
        if (fontAsset == null)
        {
            Debug.LogError("Couldn't make the rainyhearts font asset. Check the font is at " + FontPath);
            return;
        }

        // New text made in code (prompts) uses the default font
        TMP_Settings.defaultFontAsset = fontAsset;
        EditorUtility.SetDirty(TMP_Settings.instance);

        int changed = 0;

        // Prefabs first, so scene copies of them already match and don't become overrides
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            int count = SwapFonts(root.GetComponentsInChildren<TMP_Text>(true), fontAsset);
            if (count > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                changed += count;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        // Then each scene
        string openScene = SceneManager.GetActiveScene().path;
        foreach (string scenePath in ScenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = SwapFonts(texts, fontAsset);
            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                changed += count;
            }
        }

        AssetDatabase.SaveAssets();

        // Puts you back where you were
        if (!string.IsNullOrEmpty(openScene))
        {
            EditorSceneManager.OpenScene(openScene, OpenSceneMode.Single);
        }

        Debug.Log("Rainyhearts font now used on " + changed + " texts.");
    }

    /* TMP_FontAsset GetOrCreateFontAsset Method:
     * ~ Returns the rainyhearts font asset, making it first if it doesn't exist yet
     * ~ Uses TextMeshPro's own "Create > TextMeshPro > Font Asset > SDF" menu, so it's set up exactly like one made by hand
     */
    private static TMP_FontAsset GetOrCreateFontAsset()
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset != null)
        {
            return fontAsset;
        }

        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null)
        {
            return null;
        }

        // TextMeshPro's menu makes a font asset from whatever is selected
        Selection.activeObject = font;
        EditorApplication.ExecuteMenuItem("Assets/Create/TextMeshPro/Font Asset/SDF");
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
    }

    /* Int SwapFonts Method:
     * ~ Puts the new font (and its matching material) on each text
     * ~ Returns how many texts actually changed
     */
    private static int SwapFonts(TMP_Text[] texts, TMP_FontAsset fontAsset)
    {
        int count = 0;

        foreach (TMP_Text text in texts)
        {
            if (text.font == fontAsset)
            {
                continue;
            }

            Undo.RecordObject(text, "Use Rainyhearts Font");
            text.font = fontAsset;
            text.fontSharedMaterial = fontAsset.material;
            EditorUtility.SetDirty(text);
            count++;
        }

        return count;
    }
}
