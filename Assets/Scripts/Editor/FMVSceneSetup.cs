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
        // 0. Load Font Asset
        TMP_FontAsset customFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Archivo_Condensed-Medium SDF.asset");

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

        // 4. Video Area
        GameObject videoAreaObj = new GameObject("Video Area");
        videoAreaObj.transform.SetParent(canvasObj.transform, false);
        RawImage rawImage = videoAreaObj.AddComponent<RawImage>();
        rawImage.color = Color.black; 
        RectTransform rtManager = videoAreaObj.GetComponent<RectTransform>();
        rtManager.anchorMin = new Vector2(0, 0.4f);
        rtManager.anchorMax = new Vector2(1, 1);
        rtManager.offsetMin = Vector2.zero;
        rtManager.offsetMax = Vector2.zero;
        uiController.VideoArea = rawImage;

        // 5. Title Text
        GameObject titleObj = new GameObject("Title Text");
        titleObj.transform.SetParent(videoAreaObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Scene Title";
        if (customFont != null) titleText.font = customFont;
        titleText.fontSize = 80;
        titleText.alignment = TextAlignmentOptions.BottomLeft;
        titleText.color = Color.white;
        titleText.textWrappingMode = TextWrappingModes.Normal;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.8f);
        titleRt.anchorMax = new Vector2(0.95f, 0.95f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        uiController.TitleText = titleText;

        // 6. Description Text
        GameObject descObj = new GameObject("Description Text");
        descObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Scene Description goes here...";
        if (customFont != null) descText.font = customFont;
        descText.fontSize = 42;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.color = Color.white;
        descText.textWrappingMode = TextWrappingModes.Normal;
        descText.overflowMode = TextOverflowModes.Overflow;
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.05f, 0.5f);
        descRt.anchorMax = new Vector2(0.95f, 0.75f);
        descRt.offsetMin = Vector2.zero;
        descRt.offsetMax = Vector2.zero;
        uiController.DescriptionText = descText;

        // 7. Choices Container
        GameObject choicesContainer = new GameObject("Choices Container");
        choicesContainer.transform.SetParent(canvasObj.transform, false);
        VerticalLayoutGroup vlg = choicesContainer.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.spacing = 20;
        RectTransform choicesRt = choicesContainer.GetComponent<RectTransform>();
        choicesRt.anchorMin = new Vector2(0.1f, 0.05f);
        choicesRt.anchorMax = new Vector2(0.9f, 0.45f);
        choicesRt.offsetMin = Vector2.zero;
        choicesRt.offsetMax = Vector2.zero;
        uiController.ChoicesContainer = choicesContainer.transform;

        // 8. Prefab for Choice Button
        GameObject buttonPrefab = new GameObject("Choice Button Prefab");
        Image btnImage = buttonPrefab.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.2f);
        buttonPrefab.AddComponent<Button>();
        RectTransform btnRt = buttonPrefab.GetComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(0, 80);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonPrefab.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Choice Text";
        if (customFont != null) btnText.font = customFont;
        btnText.fontSize = 36;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        btnText.textWrappingMode = TextWrappingModes.Normal;
        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;

        buttonPrefab.transform.SetParent(canvasObj.transform, false);
        buttonPrefab.SetActive(false);
        uiController.ChoiceButtonPrefab = buttonPrefab;

        // 9. Title Screen Panel
        GameObject titleScreenObj = new GameObject("Title Screen Panel");
        titleScreenObj.transform.SetParent(canvasObj.transform, false);
        Image titleBg = titleScreenObj.AddComponent<Image>();
        titleBg.color = new Color(0.1f, 0.1f, 0.1f); // Dark plain background
        RectTransform titleScreenRt = titleScreenObj.GetComponent<RectTransform>();
        titleScreenRt.anchorMin = Vector2.zero;
        titleScreenRt.anchorMax = Vector2.one;
        titleScreenRt.offsetMin = Vector2.zero;
        titleScreenRt.offsetMax = Vector2.zero;

        // Title Screen Text
        GameObject mainTitleObj = new GameObject("Main Title");
        mainTitleObj.transform.SetParent(titleScreenObj.transform, false);
        TextMeshProUGUI mainTitleText = mainTitleObj.AddComponent<TextMeshProUGUI>();
        mainTitleText.text = "MY FMV GAME";
        if (customFont != null) mainTitleText.font = customFont;
        mainTitleText.fontSize = 120;
        mainTitleText.alignment = TextAlignmentOptions.Center;
        mainTitleText.color = Color.white;
        RectTransform mainTitleRt = mainTitleObj.GetComponent<RectTransform>();
        mainTitleRt.anchorMin = new Vector2(0, 0.6f);
        mainTitleRt.anchorMax = new Vector2(1, 0.9f);
        mainTitleRt.offsetMin = Vector2.zero;
        mainTitleRt.offsetMax = Vector2.zero;

        // Play Button
        GameObject playBtnObj = new GameObject("Play Button");
        playBtnObj.transform.SetParent(titleScreenObj.transform, false);
        Image playBtnImg = playBtnObj.AddComponent<Image>();
        playBtnImg.color = new Color(0.2f, 0.6f, 0.2f);
        Button playBtn = playBtnObj.AddComponent<Button>();
        RectTransform playBtnRt = playBtnObj.GetComponent<RectTransform>();
        playBtnRt.anchorMin = new Vector2(0.4f, 0.4f);
        playBtnRt.anchorMax = new Vector2(0.6f, 0.5f);
        playBtnRt.offsetMin = Vector2.zero;
        playBtnRt.offsetMax = Vector2.zero;

        GameObject playTextObj = new GameObject("Text");
        playTextObj.transform.SetParent(playBtnObj.transform, false);
        TextMeshProUGUI playText = playTextObj.AddComponent<TextMeshProUGUI>();
        playText.text = "PLAY";
        if (customFont != null) playText.font = customFont;
        playText.fontSize = 50;
        playText.alignment = TextAlignmentOptions.Center;
        playText.color = Color.white;
        RectTransform playTextRt = playTextObj.GetComponent<RectTransform>();
        playTextRt.anchorMin = Vector2.zero;
        playTextRt.anchorMax = Vector2.one;
        playTextRt.offsetMin = Vector2.zero;
        playTextRt.offsetMax = Vector2.zero;

        // Options Button
        GameObject optBtnObj = new GameObject("Options Button");
        optBtnObj.transform.SetParent(titleScreenObj.transform, false);
        Image optBtnImg = optBtnObj.AddComponent<Image>();
        optBtnImg.color = new Color(0.4f, 0.4f, 0.4f);
        Button optBtn = optBtnObj.AddComponent<Button>();
        RectTransform optBtnRt = optBtnObj.GetComponent<RectTransform>();
        optBtnRt.anchorMin = new Vector2(0.4f, 0.25f);
        optBtnRt.anchorMax = new Vector2(0.6f, 0.35f);
        optBtnRt.offsetMin = Vector2.zero;
        optBtnRt.offsetMax = Vector2.zero;

        GameObject optTextObj = new GameObject("Text");
        optTextObj.transform.SetParent(optBtnObj.transform, false);
        TextMeshProUGUI optText = optTextObj.AddComponent<TextMeshProUGUI>();
        optText.text = "OPTIONS";
        if (customFont != null) optText.font = customFont;
        optText.fontSize = 50;
        optText.alignment = TextAlignmentOptions.Center;
        optText.color = Color.white;
        RectTransform optTextRt = optTextObj.GetComponent<RectTransform>();
        optTextRt.anchorMin = Vector2.zero;
        optTextRt.anchorMax = Vector2.one;
        optTextRt.offsetMin = Vector2.zero;
        optTextRt.offsetMax = Vector2.zero;

        // 10. Options Panel
        GameObject optionsPanelObj = new GameObject("Options Panel");
        optionsPanelObj.transform.SetParent(titleScreenObj.transform, false);
        Image optionsBg = optionsPanelObj.AddComponent<Image>();
        optionsBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        RectTransform optionsPanelRt = optionsPanelObj.GetComponent<RectTransform>();
        optionsPanelRt.anchorMin = new Vector2(0.2f, 0.2f);
        optionsPanelRt.anchorMax = new Vector2(0.8f, 0.8f);
        optionsPanelRt.offsetMin = Vector2.zero;
        optionsPanelRt.offsetMax = Vector2.zero;
        optionsPanelObj.SetActive(false);

        // Options Title
        GameObject optionsTitleObj = new GameObject("Options Title");
        optionsTitleObj.transform.SetParent(optionsPanelObj.transform, false);
        TextMeshProUGUI optionsTitleText = optionsTitleObj.AddComponent<TextMeshProUGUI>();
        optionsTitleText.text = "OPTIONS";
        if (customFont != null) optionsTitleText.font = customFont;
        optionsTitleText.fontSize = 80;
        optionsTitleText.alignment = TextAlignmentOptions.Center;
        optionsTitleText.color = Color.white;
        RectTransform optionsTitleRt = optionsTitleObj.GetComponent<RectTransform>();
        optionsTitleRt.anchorMin = new Vector2(0, 0.8f);
        optionsTitleRt.anchorMax = new Vector2(1, 1);
        optionsTitleRt.offsetMin = Vector2.zero;
        optionsTitleRt.offsetMax = Vector2.zero;

        // Close Options Button
        GameObject closeOptBtnObj = new GameObject("Close Button");
        closeOptBtnObj.transform.SetParent(optionsPanelObj.transform, false);
        Image closeOptBtnImg = closeOptBtnObj.AddComponent<Image>();
        closeOptBtnImg.color = new Color(0.8f, 0.2f, 0.2f);
        Button closeOptBtn = closeOptBtnObj.AddComponent<Button>();
        RectTransform closeOptBtnRt = closeOptBtnObj.GetComponent<RectTransform>();
        closeOptBtnRt.anchorMin = new Vector2(0.4f, 0.1f);
        closeOptBtnRt.anchorMax = new Vector2(0.6f, 0.2f);
        closeOptBtnRt.offsetMin = Vector2.zero;
        closeOptBtnRt.offsetMax = Vector2.zero;

        GameObject closeOptTextObj = new GameObject("Text");
        closeOptTextObj.transform.SetParent(closeOptBtnObj.transform, false);
        TextMeshProUGUI closeOptText = closeOptTextObj.AddComponent<TextMeshProUGUI>();
        closeOptText.text = "CLOSE";
        if (customFont != null) closeOptText.font = customFont;
        closeOptText.fontSize = 40;
        closeOptText.alignment = TextAlignmentOptions.Center;
        closeOptText.color = Color.white;
        RectTransform closeOptTextRt = closeOptTextObj.GetComponent<RectTransform>();
        closeOptTextRt.anchorMin = Vector2.zero;
        closeOptTextRt.anchorMax = Vector2.one;
        closeOptTextRt.offsetMin = Vector2.zero;
        closeOptTextRt.offsetMax = Vector2.zero;

        // 11. Attach Title Screen Script
        FMVTitleScreen titleScreenScript = titleScreenObj.AddComponent<FMVTitleScreen>();
        titleScreenScript.TitleScreenPanel = titleScreenObj;
        titleScreenScript.OptionsPanel = optionsPanelObj;
        titleScreenScript.PlayButton = playBtn;
        titleScreenScript.OptionsButton = optBtn;
        titleScreenScript.CloseOptionsButton = closeOptBtn;

        // Ensure EventSystem exists using the new Input System module
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        Debug.Log("FMV Scene successfully initialized! Press Play to start.");
    }
}
