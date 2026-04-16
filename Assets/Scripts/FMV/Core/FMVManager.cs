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
        public string InitialSceneId = "hub_main";

        [Header("Runtime State")]
        public FMVScene CurrentScene;
        private Dictionary<string, FMVScene> _scenes = new Dictionary<string, FMVScene>();
        private VideoPlayer _videoPlayer;

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
            _videoPlayer.loopPointReached += OnVideoFinished;
        }

        private void Start()
        {
            LoadDatabase();
            if (_scenes.Count > 0)
            {
                FMVGameState.Instance.Reset();
                LoadScene(InitialSceneId);
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
            
            // Re-check just in case scene was changed unexpectedly during the 3 seconds
            if (CurrentScene == scene)
            {
                OnChoicePhaseStarted?.Invoke(scene);
            }
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
            OnChoicePhaseStarted?.Invoke(CurrentScene);
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
