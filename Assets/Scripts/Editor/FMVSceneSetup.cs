using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
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
        
        // 2. Create Canvas
        GameObject canvasObj = new GameObject("FMV Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
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
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Scene Title";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.LowerLeft;
        titleText.color = Color.white;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.05f);
        titleRt.anchorMax = new Vector2(0.95f, 0.2f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        uiController.TitleText = titleText;

        // 6. Description Text
        GameObject descObj = new GameObject("Description Text");
        descObj.transform.SetParent(canvasObj.transform, false);
        Text descText = descObj.AddComponent<Text>();
        descText.text = "Scene Description goes here...";
        descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descText.fontSize = 24;
        descText.alignment = TextAnchor.UpperLeft;
        descText.color = Color.white;
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.05f, 0.25f);
        descRt.anchorMax = new Vector2(0.95f, 0.35f);
        descRt.offsetMin = Vector2.zero;
        descRt.offsetMax = Vector2.zero;
        uiController.DescriptionText = descText;

        // 7. Choices Container
        GameObject choicesContainer = new GameObject("Choices Container");
        choicesContainer.transform.SetParent(canvasObj.transform, false);
        VerticalLayoutGroup vlg = choicesContainer.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.spacing = 10;
        RectTransform choicesRt = choicesContainer.GetComponent<RectTransform>();
        choicesRt.anchorMin = new Vector2(0.1f, 0.05f);
        choicesRt.anchorMax = new Vector2(0.9f, 0.2f);
        choicesRt.offsetMin = Vector2.zero;
        choicesRt.offsetMax = Vector2.zero;
        uiController.ChoicesContainer = choicesContainer.transform;

        // 8. Prefab for Choice Button
        GameObject buttonPrefab = new GameObject("Choice Button Prefab");
        Image btnImage = buttonPrefab.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.2f);
        buttonPrefab.AddComponent<Button>();
        RectTransform btnRt = buttonPrefab.GetComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(0, 40);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonPrefab.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.text = "Choice Text";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 20;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;

        buttonPrefab.transform.SetParent(canvasObj.transform, false);
        buttonPrefab.SetActive(false);
        uiController.ChoiceButtonPrefab = buttonPrefab;

        // Ensure EventSystem exists using the new Input System module
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        Debug.Log("FMV Scene successfully initialized! Press Play to start.");
    }
}
