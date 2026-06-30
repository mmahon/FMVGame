using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using FMV.Data;

namespace FMV.Core
{
    [RequireComponent(typeof(VideoPlayer))]
    public class FMVManager : MonoBehaviour
    {
        public static FMVManager Instance { get; private set; }

        [Header("Settings")]
        public string TextAssetsPath = "FMV/Scenes";
        public string VideoAssetsPath = "FMVVideos";
        public string ImageAssetsPath = "FMVImages";
        public string InitialSceneId = "hub_main";
        public bool AutoStart = false;

        [Header("Runtime State")]
        public FMVScene CurrentScene;
        private Dictionary<string, FMVScene> _scenes = new Dictionary<string, FMVScene>();
        private VideoPlayer _videoPlayer;
        public VideoPlayer ActiveVideoPlayer => _videoPlayer;

        public event Action<FMVScene> OnSceneLoaded;
        public event Action<FMVScene> OnChoicePhaseStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _videoPlayer = GetComponent<VideoPlayer>();
            _videoPlayer.renderMode = VideoRenderMode.APIOnly;
            _videoPlayer.loopPointReached += OnVideoFinished;
        }

        private void Start()
        {
            LoadDatabase();
            if (_scenes.Count > 0)
            {
                FMVGameState.Instance.Reset();
                if (AutoStart)
                {
                    LoadScene(InitialSceneId);
                }
            }
            else
            {
                Debug.LogError("No scenes found or parsed. Start aborted.");
            }
        }

        private void LoadDatabase()
        {
            TextAsset textAsset = Resources.Load<TextAsset>(TextAssetsPath);
            if (textAsset != null)
            {
                _scenes = FMVSceneParser.ParseScenes(textAsset.text);
                Debug.Log($"Loaded {_scenes.Count} scenes from {TextAssetsPath}");
            }
            else
            {
                Debug.LogError($"Could not find Scenes.txt text asset at Resources/{TextAssetsPath}");
            }
        }

        public void StartGame()
        {
            if (CurrentScene == null)
            {
                LoadScene(InitialSceneId);
            }
        }

        public void LoadScene(string sceneId)
        {
            if (!_scenes.TryGetValue(sceneId, out FMVScene nextScene))
            {
                Debug.LogError($"Scene '{sceneId}' not found!");
                return;
            }

            CurrentScene = nextScene;

            FMVGameState.Instance.VisitScene(CurrentScene.Id);
            foreach (var item in CurrentScene.AutoItems)
                FMVGameState.Instance.AddItem(item);
            foreach (var flag in CurrentScene.DecisionFlags)
                FMVGameState.Instance.SetDecision(flag, true);

            OnSceneLoaded?.Invoke(CurrentScene);

            VideoClip clip = Resources.Load<VideoClip>($"{VideoAssetsPath}/{CurrentScene.Id}");
            if (clip != null)
            {
                _videoPlayer.clip = clip;
                _videoPlayer.Play();
            }
            else
            {
                _videoPlayer.Stop();
                _videoPlayer.clip = null;
                StartCoroutine(SimulateVideoPlayback(CurrentScene));
            }
        }

        private IEnumerator SimulateVideoPlayback(FMVScene scene)
        {
            yield return new WaitForSeconds(3.0f);
            HandleVideoFinished(scene);
        }

        public void MakeChoice(FMVChoice choice)
        {
            foreach (var flag in choice.SetsFlags)
                FMVGameState.Instance.SetDecision(flag, true);

            if (!string.IsNullOrEmpty(choice.TargetSceneId))
            {
                LoadScene(choice.TargetSceneId);
            }
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            HandleVideoFinished(CurrentScene);
        }

        private void HandleVideoFinished(FMVScene scene)
        {
            if (CurrentScene != scene) return;

            FMVAutoAdvance triggered = null;
            if (scene.AutoAdvances != null && scene.AutoAdvances.Count > 0)
            {
                foreach (var autoAdvance in scene.AutoAdvances)
                {
                    if (autoAdvance.IsAvailable())
                    {
                        triggered = autoAdvance;
                        break;
                    }
                }
            }

            if (triggered != null)
            {
                LoadScene(triggered.TargetSceneId);
            }
            else if (!string.IsNullOrEmpty(scene.AutoAdvanceSceneId))
            {
                LoadScene(scene.AutoAdvanceSceneId);
            }
            else
            {
                OnChoicePhaseStarted?.Invoke(scene);
            }
        }

        private void OnDestroy()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= OnVideoFinished;
            }
        }
    }
}
