using UnityEngine;
using UnityEngine.UI;

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

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            transform.SetAsLastSibling();

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
