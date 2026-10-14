using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FMV.UI
{
    public class FMVTitleScreen : MonoBehaviour
    {
        public static FMVTitleScreen Instance { get; private set; }

        public GameObject TitleScreenPanel;
        public GameObject OptionsPanel;
        public Button PlayButton;
        public Button OptionsButton;
        public Button CloseOptionsButton;

        [Header("Content (optional)")]
        [Tooltip("Title, subtitle, footer and freeze-frame. When unset, the texts and image placed in the scene are left as they are.")]
        public FMVTitleScreenConfig Config;
        public TMP_Text TitleText;
        public TMP_Text SubtitleText;
        public TMP_Text FooterText;
        [Tooltip("Full-screen freeze-frame behind the menu. Needs an FMVAspectFillImage on the same object to crop instead of stretch.")]
        public RawImage BackgroundImage;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>Copies the config's title, subtitle, footer and freeze-frame into the UI.</summary>
        public void ApplyConfig()
        {
            if (Config == null) return;

            if (TitleText != null) TitleText.text = Config.Title;
            if (SubtitleText != null) SubtitleText.text = Config.Subtitle;
            if (FooterText != null) FooterText.text = Config.Footer;

            if (BackgroundImage != null)
            {
                BackgroundImage.color = Config.FreezeFrameTint;

                if (Config.FreezeFrame != null)
                {
                    var fill = BackgroundImage.GetComponent<FMVAspectFillImage>();
                    if (fill != null) fill.SetTexture(Config.FreezeFrame);
                    else BackgroundImage.texture = Config.FreezeFrame;
                }
            }
        }

        private void Start()
        {
            transform.SetAsLastSibling();
            ApplyConfig();

            if (TitleScreenPanel != null)
                TitleScreenPanel.SetActive(true);

            if (OptionsPanel != null)
                OptionsPanel.SetActive(false);

            if (PlayButton != null)
                PlayButton.onClick.AddListener(OnPlayClicked);

            if (OptionsButton != null)
                OptionsButton.onClick.AddListener(OnOptionsClicked);

            if (CloseOptionsButton != null)
                CloseOptionsButton.onClick.AddListener(OnCloseOptionsClicked);
        }

        private void OnPlayClicked()
        {
            if (TitleScreenPanel != null)
                TitleScreenPanel.SetActive(false);

            if (FMV.Core.FMVManager.Instance != null)
            {
                FMV.Core.FMVManager.Instance.StartGame();
            }
        }

        private void OnOptionsClicked()
        {
            if (OptionsPanel != null)
                OptionsPanel.SetActive(true);
        }

        private void OnCloseOptionsClicked()
        {
            if (OptionsPanel != null)
                OptionsPanel.SetActive(false);
        }

        public void ShowTitleScreen()
        {
            if (TitleScreenPanel != null)
            {
                TitleScreenPanel.SetActive(true);
                transform.SetAsLastSibling();
            }

            if (FMV.Core.FMVGameState.Instance != null)
            {
                FMV.Core.FMVGameState.Instance.Reset();
            }

            if (FMV.Core.FMVManager.Instance != null)
            {
                if (FMV.Core.FMVManager.Instance.ActiveVideoPlayer != null)
                {
                    FMV.Core.FMVManager.Instance.ActiveVideoPlayer.Stop();
                    FMV.Core.FMVManager.Instance.ActiveVideoPlayer.clip = null;
                }
                FMV.Core.FMVManager.Instance.CurrentScene = null;
            }
        }
    }
}
