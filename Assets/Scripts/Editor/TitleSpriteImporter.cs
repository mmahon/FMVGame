using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Applies the import settings from Art/UI/README.md to the UI kit sprites (title screen and choice
/// screen), keyed on file name. Runs whenever a texture under Assets/UI/Sprites is imported, so new or
/// re-exported PNGs come in correct without touching the Inspector.
/// </summary>
public class TitleSpriteImporter : AssetPostprocessor
{
    const string Folder = "Assets/UI/Sprites/";
    const float ButtonBorder = 24f; // btn_* and choice_btn_*
    const float PanelBorder = 40f;  // panel_modal, text_panel

    // Bump when the rules below change so Unity reimports the sprites.
    public override uint GetVersion() => 2;

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Folder)) return;

        var importer = (TextureImporter)assetImporter;
        string name = Path.GetFileNameWithoutExtension(assetPath);
        float border = BorderFor(name);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.spriteBorder = new Vector4(border, border, border, border); // L, B, R, T

        // 9-sliced sprites need Full Rect meshes so the slices are not tightly cropped.
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = border > 0f ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
        importer.SetTextureSettings(settings);
    }

    static float BorderFor(string name)
    {
        if (name.StartsWith("btn_") || name.StartsWith("choice_btn_")) return ButtonBorder;
        if (name == "panel_modal" || name == "text_panel") return PanelBorder;
        return 0f;
    }

    [MenuItem("FMV/Reimport UI Sprites")]
    static void Reimport()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Folder.TrimEnd('/') }))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
    }
}
