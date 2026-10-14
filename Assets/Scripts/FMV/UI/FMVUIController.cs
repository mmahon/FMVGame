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

        [Header("Choice screen")]
        [Tooltip("Left text block (title, accent rule, description). Hidden while the video plays, shown in the choice phase.")]
        public GameObject TextBlock;
        [Tooltip("Holder of the description text. Hidden when ShowDescription is off or the scene has no description.")]
        public GameObject DescriptionRoot;
        [Tooltip("Show the scene description under the title. Off leaves only the title and accent rule, bottom-aligned with the choices. The choice stack never moves.")]
        public bool ShowDescription = true;

        [Header("State")]
        public Texture2D DefaultFallbackTexture;

        private void Start()
        {
            if (FMVManager.Instance != null)
            {
                FMVManager.Instance.OnSceneLoaded += HandleSceneLoaded;
                FMVManager.Instance.OnChoicePhaseStarted += HandleChoicePhase;
                FMVManager.Instance.OnVideoPlaybackStarted += HandleVideoPlaybackStarted;

                // If the manager has already loaded the initial scene before we subscribed,
                // we should initialize our UI immediately to avoid missing it.
                if (FMVManager.Instance.CurrentScene != null)
                {
                    HandleSceneLoaded(FMVManager.Instance.CurrentScene);
                }
            }

            if (ChoicesContainer != null) ChoicesContainer.gameObject.SetActive(false);
            if (TextBlock != null) TextBlock.SetActive(false);
            if (TitleText != null)
            {
                TitleText.text = "";
                TitleText.textWrappingMode = TextWrappingModes.Normal;
            }
            if (DescriptionText != null)
            {
                DescriptionText.text = "";
                DescriptionText.textWrappingMode = TextWrappingModes.Normal;
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

        // Swap the freeze-frame for the live video once, when playback starts. After the clip ends nothing
        // reassigns the texture, so with no freeze-frame image the video area holds the clip's last frame.
        private void HandleVideoPlaybackStarted(Texture videoTexture)
        {
            if (videoTexture == null) return;
            SetVideoAreaTexture(videoTexture);
        }

        private void HandleSceneLoaded(FMVScene scene)
        {
            if (ChoicesContainer != null) ChoicesContainer.gameObject.SetActive(false);
            if (TextBlock != null) TextBlock.SetActive(false);
            if (DescriptionText != null) DescriptionText.text = "";
            if (TitleText != null) TitleText.text = "";

            // Load static image immediately so it acts as the "video" during the playback phase
            ShowSceneImage(scene);

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
            if (TextBlock != null) TextBlock.SetActive(true);
            SetShowDescription(ShowDescription);
            if (ChoicesContainer != null) ChoicesContainer.gameObject.SetActive(true);

            // Re-apply static freeze-frame image as background for the choice selection phase
            ShowSceneImage(scene);

            if (ChoicesContainer != null && ChoiceButtonPrefab != null)
            {
                foreach (var choice in FMVChoiceSelector.SelectForDisplay(scene.Choices))
                {
                    var captured = choice;
                    AddChoiceButton(choice.DisplayText, () => FMVManager.Instance.MakeChoice(captured));
                }

                if (scene.Choices.Count == 0 && scene.IsEnding)
                {
                    AddChoiceButton("Restart", OnRestartClicked);
                }

                RefitChoiceButtons();
            }
        }

        /// <summary>
        /// Turns the description on or off. Only the left text block changes; the choice stack is anchored
        /// on its own and does not move. The description is also hidden when the scene has none.
        /// </summary>
        public void SetShowDescription(bool show)
        {
            ShowDescription = show;

            bool visible = show && DescriptionText != null && !string.IsNullOrEmpty(DescriptionText.text);
            if (DescriptionRoot != null) DescriptionRoot.SetActive(visible);
            else if (DescriptionText != null) DescriptionText.gameObject.SetActive(visible);
        }

        private void OnValidate()
        {
            // Flipping the checkbox in the Inspector while playing takes effect straight away.
            if (Application.isPlaying) SetShowDescription(ShowDescription);
        }

        // Shows the scene's freeze-frame image (or the default fallback) in the video area.
        private void ShowSceneImage(FMVScene scene)
        {
            if (VideoArea == null) return;

            Texture2D image = Resources.Load<Texture2D>($"{FMVManager.Instance.ImageAssetsPath}/{scene.Id}");
            if (image == null) image = DefaultFallbackTexture;
            if (image == null) return;

            SetVideoAreaTexture(image);
        }

        // Routed through FMVAspectFillImage (when present) so every texture, image or video, covers the screen without stretching.
        private void SetVideoAreaTexture(Texture texture)
        {
            if (VideoArea == null) return;

            var fill = VideoArea.GetComponent<FMVAspectFillImage>();
            if (fill != null) fill.SetTexture(texture);
            else VideoArea.texture = texture;
            VideoArea.color = Color.white;
        }

        private void AddChoiceButton(string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = Instantiate(ChoiceButtonPrefab, ChoicesContainer);
            btnObj.SetActive(true);

            var choiceButton = btnObj.GetComponent<FMVChoiceButton>();
            if (choiceButton != null)
            {
                choiceButton.SetLabel(label);
            }
            else
            {
                // Older scenes that still use a plain button prefab.
                TextMeshProUGUI tmpText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = label;
                }
                else
                {
                    Text legacyText = btnObj.GetComponentInChildren<Text>();
                    if (legacyText != null) legacyText.text = label;
                }
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(onClick);
        }

        // Once the stack has its real width, re-measure the labels so wrapping (and so button height) is exact.
        private void RefitChoiceButtons()
        {
            var buttons = ChoicesContainer.GetComponentsInChildren<FMVChoiceButton>();
            if (buttons.Length == 0) return;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ChoicesContainer);
            foreach (var b in buttons) b.Refit();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ChoicesContainer);
        }

        private void OnRestartClicked()
        {
            if (FMVTitleScreen.Instance != null) FMVTitleScreen.Instance.ShowTitleScreen();
            else FMVManager.Instance.LoadScene(FMVManager.Instance.InitialSceneId);
        }

        private void OnDestroy()
        {
            if (FMVManager.Instance != null)
            {
                FMVManager.Instance.OnSceneLoaded -= HandleSceneLoaded;
                FMVManager.Instance.OnChoicePhaseStarted -= HandleChoicePhase;
                FMVManager.Instance.OnVideoPlaybackStarted -= HandleVideoPlaybackStarted;
            }
        }
    }
}
