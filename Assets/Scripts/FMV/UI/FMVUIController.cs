using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMV.Core;
using FMV.Data;

namespace FMV.UI
{
    public class FMVUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject CanvasRoot;
        public RawImage VideoArea;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI DescriptionText;
        public Transform ChoicesContainer;
        public GameObject ChoiceButtonPrefab;

        [Header("State")]
        public Texture2D DefaultFallbackTexture;

        private void Start()
        {
            if (FMVManager.Instance != null)
            {
                FMVManager.Instance.OnSceneLoaded += HandleSceneLoaded;
                FMVManager.Instance.OnChoicePhaseStarted += HandleChoicePhase;
            }
            
            if (ChoicesContainer != null) ChoicesContainer.gameObject.SetActive(false);
            if (TitleText != null) 
            {
                TitleText.text = "";
                TitleText.enableWordWrapping = true;
            }
            if (DescriptionText != null) 
            {
                DescriptionText.text = "";
                DescriptionText.enableWordWrapping = true;
                DescriptionText.overflowMode = TextOverflowModes.Overflow;
            }

            if (VideoArea != null) 
            {
                RectTransform rt = VideoArea.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        private void HandleSceneLoaded(FMVScene scene)
        {
            if (ChoicesContainer != null) ChoicesContainer.gameObject.SetActive(false);
            if (DescriptionText != null) DescriptionText.text = "";
            if (TitleText != null) TitleText.text = ""; 

            // Load static image immediately so it acts as the "video" during the playback phase
            Texture2D staticImage = Resources.Load<Texture2D>($"Generated2D/FMV/Scenes/{scene.Id}");
            if (staticImage != null)
            {
                if (VideoArea != null) VideoArea.texture = staticImage;
            }
            else if (DefaultFallbackTexture != null)
            {
                if (VideoArea != null) VideoArea.texture = DefaultFallbackTexture;
            }

            if (ChoicesContainer != null)
            {
                foreach (Transform child in ChoicesContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void HandleChoicePhase(FMVScene scene)
        {
            if (TitleText != null) TitleText.text = scene.Title;
            if (DescriptionText != null) DescriptionText.text = scene.Description;
            if (ChoicesContainer != null) ChoicesContainer.gameObject.SetActive(true);

            if (ChoicesContainer != null && ChoiceButtonPrefab != null)
            {
                foreach (var choice in scene.Choices)
                {
                    if (choice.IsAvailable())
                    {
                        GameObject btnObj = Instantiate(ChoiceButtonPrefab, ChoicesContainer);
                        btnObj.SetActive(true);
                        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                        if (btnText != null) btnText.text = choice.DisplayText;
                        else
                        {
                            Text legacyText = btnObj.GetComponentInChildren<Text>();
                            if (legacyText != null) legacyText.text = choice.DisplayText;
                        }

                        Button btn = btnObj.GetComponent<Button>();
                        if (btn != null)
                        {
                            var capturedChoice = choice;
                            btn.onClick.AddListener(() => OnChoiceClicked(capturedChoice));
                        }
                    }
                }

                if (scene.Choices.Count == 0 && scene.IsEnding)
                {
                    GameObject btnObj = Instantiate(ChoiceButtonPrefab, ChoicesContainer);
                    TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = "Restart";
                    else
                    {
                        Text legacyText = btnObj.GetComponentInChildren<Text>();
                        if (legacyText != null) legacyText.text = "Restart";
                    }
                    Button btn = btnObj.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.AddListener(() => FMVManager.Instance.LoadScene(FMVManager.Instance.InitialSceneId));
                    }
                }
            }
        }

        private void OnChoiceClicked(FMVChoice choice)
        {
            FMVManager.Instance.MakeChoice(choice);
        }

        private void OnDestroy()
        {
            if (FMVManager.Instance != null)
            {
                FMVManager.Instance.OnSceneLoaded -= HandleSceneLoaded;
                FMVManager.Instance.OnChoicePhaseStarted -= HandleChoicePhase;
            }
        }
    }
}
