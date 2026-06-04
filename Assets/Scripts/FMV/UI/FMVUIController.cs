using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMV.Core;
using FMV.Data;
using System.Linq;

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
                
                // If the manager has already loaded the initial scene before we subscribed, 
                // we should initialize our UI immediately to avoid missing it.
                if (FMVManager.Instance.CurrentScene != null)
                {
                    HandleSceneLoaded(FMVManager.Instance.CurrentScene);
                }
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
                VideoArea.color = Color.black;
            }
        }

        private void Update()
        {
            if (FMVManager.Instance != null && FMVManager.Instance.ActiveVideoPlayer != null && VideoArea != null)
            {
                if (FMVManager.Instance.ActiveVideoPlayer.isPlaying && FMVManager.Instance.ActiveVideoPlayer.texture != null)
                {
                    VideoArea.texture = FMVManager.Instance.ActiveVideoPlayer.texture;
                    VideoArea.color = Color.white;
                }
            }
        }

        private void HandleSceneLoaded(FMVScene scene)
        {
            if (ChoicesContainer != null) ChoicesContainer.gameObject.SetActive(false);
            if (DescriptionText != null) DescriptionText.text = "";
            if (TitleText != null) TitleText.text = ""; 
            Debug.Log("FMVUIController HandleSceneLoaded");
            // Load static image immediately so it acts as the "video" during the playback phase
            Debug.Log("FMVUIController loading: " + $"{FMVManager.Instance.ImageAssetsPath}/{scene.Id}");
            Texture2D staticImage = Resources.Load<Texture2D>($"{FMVManager.Instance.ImageAssetsPath}/{scene.Id}");
            if (staticImage != null)
            {
                Debug.Log("FMVUIController image not null");
                if (VideoArea != null) 
                {
                    VideoArea.texture = staticImage;
                    VideoArea.color = Color.white;
                }
            }
            else if (DefaultFallbackTexture != null)
            {
                if (VideoArea != null) 
                {
                    VideoArea.texture = DefaultFallbackTexture;
                    VideoArea.color = Color.white;
                }
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
                var validChoices = scene.Choices.Where(c => c.IsAvailable()).ToList();
                var choicesToDisplay = validChoices.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();

                foreach (var choice in choicesToDisplay)
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
