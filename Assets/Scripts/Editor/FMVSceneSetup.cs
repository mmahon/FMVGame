using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using FMV.Core;
using FMV.UI;

public class FMVSceneSetup
{
    [MenuItem("FMV/Initialize Sample Scene")]
    public static void InitializeScene()
    {
        // 1. Create main manager
        GameObject fmvObj = new GameObject("FMV Manager");
        FMVManager manager = fmvObj.AddComponent<FMVManager>();
        manager.AutoStart = false;
        
        // 2. Create Canvas
        GameObject canvasObj = new GameObject("FMV Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // 3. Create UI Controller
        FMVUIController uiController = canvasObj.AddComponent<FMVUIController>();
        uiController.CanvasRoot = canvasObj;

        // 4. Choice screen (video area, scrim, text block, choice stack, button prefab)
        BuildChoiceScreen(canvasObj.transform, uiController);

        // 9. Title screen (layout and styling from Art/UI/README.md)
        BuildTitleScreen(canvasObj.transform);

        // Ensure EventSystem exists using the new Input System module
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        Debug.Log("FMV Scene successfully initialized! Press Play to start.");
    }

    // ---------------------------------------------------------------- choice screen

    const string ChoicePrefabPath = "Assets/UI/Prefabs/ChoiceButton.prefab";
    const string MediumFontPath = "Assets/Fonts/Archivo-Medium SDF.asset";
    static readonly Color DescriptionColor = Hex("#C5D1DA");

    // Choice-screen layout, 1920x1080 reference pixels (Art/UI/README.md)
    const float TextBlockX = 120f, TextBlockWidth = 860f;
    const float ChoiceStackX = 1060f, ChoiceStackWidth = 740f;
    const float BottomMargin = 96f;
    const float ChoiceMinHeight = 84f, ChoiceGap = 16f;

    // Replaces the game UI (video area, text block, choice stack, button prefab) in the open scene with a
    // freshly built one, leaving the manager and the title screen alone. Use it to bring an older scene
    // up to the current layout.
    [MenuItem("FMV/Rebuild Choice Screen")]
    public static void RebuildChoiceScreen()
    {
        var ui = Object.FindAnyObjectByType<FMVUIController>(FindObjectsInactive.Include);
        if (ui == null)
        {
            Debug.LogError("[FMV] No FMVUIController in the open scene. Run FMV > Initialize Sample Scene first.");
            return;
        }

        bool showDescription = ui.ShowDescription;
        Texture2D fallback = ui.DefaultFallbackTexture;

        string[] oldNames = { "Video Area", "Description Text", "Choices Container", "Choice Button Prefab", "Choice Scrim", "Text Block" };
        foreach (string name in oldNames)
        {
            Transform old = ui.transform.Find(name);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        }

        BuildChoiceScreen(ui.transform, ui);
        ui.ShowDescription = showDescription;
        ui.DefaultFallbackTexture = fallback;

        // The game UI sits behind the title screen.
        var title = Object.FindAnyObjectByType<FMVTitleScreen>(FindObjectsInactive.Include);
        if (title != null) title.transform.SetAsLastSibling();

        EditorUtility.SetDirty(ui);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
        Debug.Log("[FMV] Choice screen rebuilt. Save the scene to keep it.");
    }

    static void BuildChoiceScreen(Transform canvas, FMVUIController ui)
    {
        TMP_FontAsset condensed = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CondensedFontPath);
        TMP_FontAsset body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFontPath);

        // Back to front: video / freeze-frame (full screen, aspect-fill) -> scrim -> text block + choice stack
        GameObject videoObj = NewUI("Video Area", canvas);
        Stretch(videoObj.GetComponent<RectTransform>());
        RawImage video = videoObj.AddComponent<RawImage>();
        video.color = Color.black;
        videoObj.AddComponent<FMVAspectFillImage>();
        ui.VideoArea = video;

        GameObject scrimObj = NewUI("Choice Scrim", canvas);
        Stretch(scrimObj.GetComponent<RectTransform>());
        Image scrim = scrimObj.AddComponent<Image>();
        scrim.sprite = LoadSprite("choice_scrim", "Choice");
        scrim.raycastTarget = false;

        // Left text block: bottom-left anchored, grows upward. Title, accent rule, description.
        GameObject blockObj = NewUI("Text Block", canvas);
        RectTransform blockRt = blockObj.GetComponent<RectTransform>();
        blockRt.anchorMin = blockRt.anchorMax = Vector2.zero;
        blockRt.pivot = Vector2.zero;
        blockRt.anchoredPosition = new Vector2(TextBlockX, BottomMargin);
        blockRt.sizeDelta = new Vector2(TextBlockWidth, 0f);
        var blockLayout = blockObj.AddComponent<VerticalLayoutGroup>();
        blockLayout.childAlignment = TextAnchor.LowerLeft;
        blockLayout.spacing = 22f; // title -> rule
        blockLayout.childControlWidth = blockLayout.childControlHeight = true;
        blockLayout.childForceExpandWidth = blockLayout.childForceExpandHeight = false;
        blockObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI title = AddText(blockObj.transform, "Title", "Scene Title", condensed, 76f, TextColor,
            characterSpacing: 4f, lineSpacing: -3.8f, FontStyles.UpperCase);
        title.textWrappingMode = TextWrappingModes.Normal;
        ExpandWidth(title.gameObject);

        GameObject rule = NewUI("Accent Rule", blockObj.transform);
        Image ruleImg = rule.AddComponent<Image>();
        ruleImg.sprite = LoadSprite("accent_rule");
        ruleImg.raycastTarget = false;
        var ruleLayout = rule.AddComponent<LayoutElement>();
        ruleLayout.preferredWidth = 96f;
        ruleLayout.preferredHeight = 4f;
        ruleLayout.flexibleWidth = 0f;

        // Description sits in its own holder so it can be hidden together with its gap. The README's 26 px gap
        // (22 spacing + 4 padding) is measured to the text line box; TMP puts no half-leading above the first
        // line or below the last (the 150% line height adds ~6 px at each end), so those 6 px are added here.
        GameObject descRoot = NewUI("Description", blockObj.transform);
        var descLayout = descRoot.AddComponent<VerticalLayoutGroup>();
        descLayout.padding = new RectOffset(0, 0, 10, 6);
        descLayout.childControlWidth = descLayout.childControlHeight = true;
        descLayout.childForceExpandWidth = descLayout.childForceExpandHeight = false;
        ExpandWidth(descRoot);

        TextMeshProUGUI desc = AddText(descRoot.transform, "Description Text", "Scene description goes here...", body, 30f,
            DescriptionColor, characterSpacing: 0f, lineSpacing: 41.2f);
        desc.textWrappingMode = TextWrappingModes.Normal;
        ExpandWidth(desc.gameObject);

        // Right choice stack: bottom-anchored on its own, so toggling the description never moves it.
        GameObject stackObj = NewUI("Choices Container", canvas);
        RectTransform stackRt = stackObj.GetComponent<RectTransform>();
        stackRt.anchorMin = stackRt.anchorMax = Vector2.zero;
        stackRt.pivot = Vector2.zero;
        stackRt.anchoredPosition = new Vector2(ChoiceStackX, BottomMargin);
        stackRt.sizeDelta = new Vector2(ChoiceStackWidth, 0f);
        var stack = stackObj.AddComponent<VerticalLayoutGroup>();
        stack.childAlignment = TextAnchor.LowerLeft;
        stack.spacing = ChoiceGap;
        stack.childControlWidth = stack.childControlHeight = true;
        stack.childForceExpandWidth = true;
        stack.childForceExpandHeight = false;
        stackObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ui.TitleText = title;
        ui.DescriptionText = desc;
        ui.TextBlock = blockObj;
        ui.DescriptionRoot = descRoot;
        ui.ChoicesContainer = stackObj.transform;
        ui.ChoiceButtonPrefab = BuildChoiceButtonPrefab();
    }

    // Creates (or overwrites) Assets/UI/Prefabs/ChoiceButton.prefab and returns the saved asset.
    static GameObject BuildChoiceButtonPrefab()
    {
        TMP_FontAsset medium = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MediumFontPath);

        GameObject root = NewUI("Choice Button", null);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(ChoiceStackWidth, ChoiceMinHeight);

        Image img = root.AddComponent<Image>();
        img.sprite = LoadSprite("choice_btn_normal", "Choice");
        img.type = Image.Type.Sliced;

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = img;
        SetSpriteSwap(btn, "choice_btn_hover", "choice_btn_pressed", "choice_btn_disabled", "Choice", selected: "choice_btn_selected");

        root.AddComponent<LayoutElement>().minHeight = ChoiceMinHeight;

        // Label padding: 40 left, 34 right, 14 top and bottom. The group sizes the button to the label.
        var group = root.AddComponent<VerticalLayoutGroup>();
        group.padding = new RectOffset(40, 34, 14, 14);
        group.childAlignment = TextAnchor.MiddleLeft;
        group.childControlWidth = group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;

        TextMeshProUGUI label = AddText(root.transform, "Label", "Choice text", medium, 32f, TextColor,
            characterSpacing: 0f, lineSpacing: 6.2f);
        label.textWrappingMode = TextWrappingModes.Normal;
        label.enableAutoSizing = true;
        label.fontSizeMin = 26f;
        label.fontSizeMax = 32f;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        var labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;

        // Chevron: right end, vertically centred, outside the layout. Only shown while hovered or selected.
        GameObject chevronObj = NewUI("Chevron", root.transform);
        RectTransform chevronRt = chevronObj.GetComponent<RectTransform>();
        chevronRt.anchorMin = chevronRt.anchorMax = new Vector2(1f, 0.5f);
        chevronRt.pivot = new Vector2(0.5f, 0.5f);
        chevronRt.anchoredPosition = new Vector2(-53f, 0f);
        chevronRt.sizeDelta = new Vector2(36f, 36f); // the mock shows the 48 px sprite at about 0.75 scale
        Image chevron = chevronObj.AddComponent<Image>();
        chevron.sprite = LoadSprite("icon_chevron", "Choice");
        chevron.raycastTarget = false;
        chevron.enabled = false;
        chevronObj.AddComponent<LayoutElement>().ignoreLayout = true;

        var choiceButton = root.AddComponent<FMVChoiceButton>();
        choiceButton.Label = label;
        choiceButton.Chevron = chevron;
        choiceButton.LabelLayout = labelLayout;

        System.IO.Directory.CreateDirectory("Assets/UI/Prefabs");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ChoicePrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void ExpandWidth(GameObject go)
    {
        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
    }

    // Replaces the title screen in the open scene with a freshly built one, leaving everything else
    // (manager settings, game UI) alone. Use it to bring an older scene up to the current layout.
    [MenuItem("FMV/Rebuild Title Screen")]
    public static void RebuildTitleScreen()
    {
        var existing = Object.FindAnyObjectByType<FMVTitleScreen>(FindObjectsInactive.Include);
        Transform canvas = existing != null ? existing.transform.parent : null;
        if (canvas == null)
        {
            var found = Object.FindAnyObjectByType<Canvas>();
            canvas = found != null ? found.transform : null;
        }
        if (canvas == null)
        {
            Debug.LogError("[FMV] No Canvas in the open scene. Run FMV > Initialize Sample Scene first.");
            return;
        }

        if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
        BuildTitleScreen(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("[FMV] Title screen rebuilt. Save the scene to keep it.");
    }

    // ---------------------------------------------------------------- title screen

    const string SpriteDir = "Assets/UI/Sprites/Title/";
    const string ConfigPath = "Assets/UI/TitleScreenConfig.asset";
    const string DefaultFreezeFramePath = "Assets/Resources/FMV/ClonedMahon/Images/a2_hallway_2_choice.jpeg";
    const string CondensedFontPath = "Assets/Fonts/Archivo_Condensed-Medium SDF.asset";
    const string BodyFontPath = "Assets/Fonts/Archivo-Regular SDF.asset";

    // Colour tokens (Art/UI/README.md)
    static readonly Color TextColor = Hex("#E8EEF2");
    static readonly Color SteelColor = Hex("#8EA3B3");
    static readonly Color MutedColor = Hex("#5F7384");

    const float ColumnX = 180f; // left edge of the text column, in 1920x1080 reference pixels

    static void BuildTitleScreen(Transform canvas)
    {
        TMP_FontAsset condensed = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CondensedFontPath);
        TMP_FontAsset body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFontPath);
        FMVTitleScreenConfig config = GetOrCreateConfig();

        // Root: full screen, hosts FMVTitleScreen. Drawn last so it covers the game UI.
        GameObject root = NewUI("Title Screen Panel", canvas);
        Stretch(root.GetComponent<RectTransform>());

        // Back to front: freeze-frame -> scrim -> text column -> footer -> options modal
        GameObject bgObj = NewUI("Freeze Frame", root.transform);
        Stretch(bgObj.GetComponent<RectTransform>());
        RawImage bg = bgObj.AddComponent<RawImage>(); // also blocks clicks to the game UI behind
        bg.color = Color.white;
        bgObj.AddComponent<FMVAspectFillImage>();

        GameObject scrimObj = NewUI("Scrim", root.transform);
        Stretch(scrimObj.GetComponent<RectTransform>());
        Image scrim = scrimObj.AddComponent<Image>();
        scrim.sprite = LoadSprite("title_scrim");
        scrim.raycastTarget = false;

        GameObject column = NewUI("Text Column", root.transform);
        Stretch(column.GetComponent<RectTransform>());

        AddImage(column.transform, "Logo Mark", "logo_mark", ColumnX, 140f, 128f, 128f, Image.Type.Simple);

        TextMeshProUGUI title = AddText(column.transform, "Title", "THEY\nCLONED ME", condensed, 132f,
            TextColor, characterSpacing: 5f, lineSpacing: -7.5f, FontStyles.UpperCase);
        Place(title.rectTransform, ColumnX, 299f, 1300f, 300f);

        TextMeshProUGUI subtitle = AddText(column.transform, "Subtitle", "An interactive thriller", body, 30f,
            SteelColor, characterSpacing: 4f);
        Place(subtitle.rectTransform, ColumnX, 594f, 1000f, 44f);

        AddImage(column.transform, "Accent Rule", "accent_rule", ColumnX, 676f, 96f, 4f, Image.Type.Simple);

        Button play = AddMenuButton(column.transform, "Play Button", "PLAY", "btn_primary", ColumnX, 736f, condensed, Color.white);
        Button options = AddMenuButton(column.transform, "Options Button", "OPTIONS", "btn_secondary", ColumnX, 832f, condensed, TextColor);

        TextMeshProUGUI footer = AddText(root.transform, "Footer", "v0.1  ·  Prototype build", body, 20f, MutedColor, characterSpacing: 6f);
        RectTransform footerRt = footer.rectTransform;
        footerRt.anchorMin = footerRt.anchorMax = Vector2.zero;
        footerRt.pivot = Vector2.zero;
        footerRt.anchoredPosition = new Vector2(ColumnX, 57f);
        footerRt.sizeDelta = new Vector2(1000f, 28f);

        // Options modal: panel_modal, heading, divider, close icon
        GameObject optionsObj = NewUI("Options Panel", root.transform);
        RectTransform optionsRt = optionsObj.GetComponent<RectTransform>();
        optionsRt.anchorMin = new Vector2(0.2f, 0.2f);
        optionsRt.anchorMax = new Vector2(0.8f, 0.8f);
        optionsRt.offsetMin = optionsRt.offsetMax = Vector2.zero;
        Image panelImg = optionsObj.AddComponent<Image>();
        panelImg.sprite = LoadSprite("panel_modal");
        panelImg.type = Image.Type.Sliced;

        TextMeshProUGUI heading = AddText(optionsObj.transform, "Heading", "OPTIONS", condensed, 38f,
            TextColor, characterSpacing: 8f, lineSpacing: 0f, FontStyles.UpperCase);
        RectTransform headingRt = heading.rectTransform;
        headingRt.anchorMin = new Vector2(0f, 1f);
        headingRt.anchorMax = new Vector2(1f, 1f);
        headingRt.pivot = new Vector2(0.5f, 1f);
        headingRt.offsetMin = new Vector2(56f, -100f);
        headingRt.offsetMax = new Vector2(-120f, -40f);
        heading.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject dividerObj = NewUI("Divider", optionsObj.transform);
        RectTransform dividerRt = dividerObj.GetComponent<RectTransform>();
        dividerRt.anchorMin = new Vector2(0f, 1f);
        dividerRt.anchorMax = new Vector2(1f, 1f);
        dividerRt.pivot = new Vector2(0.5f, 1f);
        dividerRt.offsetMin = new Vector2(56f, -112f);
        dividerRt.offsetMax = new Vector2(-56f, -110f);
        Image divider = dividerObj.AddComponent<Image>();
        divider.sprite = LoadSprite("divider_h");
        divider.raycastTarget = false;

        GameObject closeObj = NewUI("Close Button", optionsObj.transform);
        RectTransform closeRt = closeObj.GetComponent<RectTransform>();
        closeRt.anchorMin = closeRt.anchorMax = new Vector2(1f, 1f);
        closeRt.pivot = new Vector2(1f, 1f);
        closeRt.anchoredPosition = new Vector2(-32f, -32f);
        closeRt.sizeDelta = new Vector2(48f, 48f);
        Image closeImg = closeObj.AddComponent<Image>();
        closeImg.sprite = LoadSprite("icon_close");
        Button close = closeObj.AddComponent<Button>();
        close.targetGraphic = closeImg;
        SetSpriteSwap(close, hover: "icon_close_hover", pressed: "icon_close_hover", disabled: null);

        optionsObj.SetActive(false);

        FMVTitleScreen screen = root.AddComponent<FMVTitleScreen>();
        screen.TitleScreenPanel = root;
        screen.OptionsPanel = optionsObj;
        screen.PlayButton = play;
        screen.OptionsButton = options;
        screen.CloseOptionsButton = close;
        screen.Config = config;
        screen.TitleText = title;
        screen.SubtitleText = subtitle;
        screen.FooterText = footer;
        screen.BackgroundImage = bg;
        screen.ApplyConfig(); // show the configured text and freeze-frame in the Editor too
    }

    // Creates the config asset once. An existing asset is never overwritten, so edits to it survive re-running setup.
    static FMVTitleScreenConfig GetOrCreateConfig()
    {
        var config = AssetDatabase.LoadAssetAtPath<FMVTitleScreenConfig>(ConfigPath);
        if (config != null) return config;

        config = ScriptableObject.CreateInstance<FMVTitleScreenConfig>();
        config.Title = "They\nCloned Me";
        config.Subtitle = "An interactive thriller";
        config.Footer = "v0.1  ·  Prototype build";
        config.FreezeFrame = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultFreezeFramePath);
        if (config.FreezeFrame == null)
            Debug.LogWarning($"[FMV] Default freeze-frame not found at {DefaultFreezeFramePath}; assign one on {ConfigPath}.");

        AssetDatabase.CreateAsset(config, ConfigPath);
        AssetDatabase.SaveAssets();
        return config;
    }

    // kit = the folder under Assets/UI/Sprites ("Title" or "Choice")
    static Sprite LoadSprite(string name, string kit = "Title")
    {
        string dir = $"Assets/UI/Sprites/{kit}/";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{dir}{name}.png");
        if (sprite == null)
            Debug.LogWarning($"[FMV] Sprite '{name}' not found in {dir} (not imported as a Sprite yet?).");
        return sprite;
    }

    static Color Hex(string html)
    {
        ColorUtility.TryParseHtmlString(html, out Color c);
        return c;
    }

    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // Positions from the top-left corner of the canvas, in reference pixels.
    static void Place(RectTransform rt, float x, float yFromTop, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -yFromTop);
        rt.sizeDelta = new Vector2(width, height);
    }

    static Image AddImage(Transform parent, string name, string spriteName, float x, float y, float w, float h, Image.Type type)
    {
        GameObject go = NewUI(name, parent);
        Place(go.GetComponent<RectTransform>(), x, y, w, h);
        Image img = go.AddComponent<Image>();
        img.sprite = LoadSprite(spriteName);
        img.type = type;
        img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color,
        float characterSpacing = 0f, float lineSpacing = 0f, FontStyles style = FontStyles.Normal)
    {
        GameObject go = NewUI(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        if (font != null) tmp.font = font;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.characterSpacing = characterSpacing; // TMP units: hundredths of an em, so 5 = +5%
        tmp.lineSpacing = lineSpacing;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button AddMenuButton(Transform parent, string name, string label, string spritePrefix, float x, float y,
        TMP_FontAsset font, Color labelColor)
    {
        GameObject go = NewUI(name, parent);
        Place(go.GetComponent<RectTransform>(), x, y, 420f, 76f);

        Image img = go.AddComponent<Image>();
        img.sprite = LoadSprite($"{spritePrefix}_normal");
        img.type = Image.Type.Sliced;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        SetSpriteSwap(btn, $"{spritePrefix}_hover", $"{spritePrefix}_pressed", $"{spritePrefix}_disabled");

        TextMeshProUGUI text = AddText(go.transform, "Label", label, font, 36f, labelColor,
            characterSpacing: 12f, lineSpacing: 0f, FontStyles.UpperCase);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.Center;
        return btn;
    }

    // Sprite Swap: normal = the Image's sprite; hover/pressed/disabled from the given sprite names.
    // "Selected" uses the given sprite (keyboard / gamepad focus), or reuses the normal sprite so a
    // clicked button does not stay highlighted.
    static void SetSpriteSwap(Button btn, string hover, string pressed, string disabled, string kit = "Title", string selected = null)
    {
        btn.transition = Selectable.Transition.SpriteSwap;
        Image img = (Image)btn.targetGraphic;
        btn.spriteState = new SpriteState
        {
            highlightedSprite = LoadSprite(hover, kit),
            pressedSprite = LoadSprite(pressed, kit),
            selectedSprite = selected != null ? LoadSprite(selected, kit) : img.sprite,
            disabledSprite = disabled != null ? LoadSprite(disabled, kit) : null
        };
    }
}
