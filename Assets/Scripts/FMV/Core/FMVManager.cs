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
        public string VideoAssetsPath = "FMV/Videos";
        public string ImageAssetsPath = "FMV/Images";
        public string InitialSceneId = "hub_main";
        public bool AutoStart = false;
        [Tooltip("Seconds to wait for a clip to prepare before skipping it.")]
        public float PrepareTimeoutSeconds = 10f;

        [Header("Runtime State")]
        public FMVScene CurrentScene;
        private Dictionary<string, FMVScene> _scenes = new Dictionary<string, FMVScene>();
        private VideoPlayer _videoPlayer;
        private VideoClip _loadedClip;
        private Coroutine _pendingVideoRoutine;
        public VideoPlayer ActiveVideoPlayer => _videoPlayer;

        public event Action<FMVScene> OnSceneLoaded;
        public event Action<FMVScene> OnChoicePhaseStarted;
        /// <summary>Fired once per clip, right after playback starts, with the texture the frames render into.</summary>
        public event Action<Texture> OnVideoPlaybackStarted;

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
            _videoPlayer.errorReceived += OnVideoError;
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

            // Drop anything still pending from the previous scene so it can't fire for this one.
            if (_pendingVideoRoutine != null)
            {
                StopCoroutine(_pendingVideoRoutine);
                _pendingVideoRoutine = null;
            }
            _videoPlayer.Stop();
            _videoPlayer.clip = null;
            if (_loadedClip != null)
            {
                Resources.UnloadAsset(_loadedClip);
                _loadedClip = null;
            }

            VideoClip clip = Resources.Load<VideoClip>($"{VideoAssetsPath}/{CurrentScene.Id}");
            if (clip != null)
            {
                _loadedClip = clip;
                _videoPlayer.clip = clip;
                _pendingVideoRoutine = StartCoroutine(PrepareAndPlay(CurrentScene));
            }
            else
            {
                _pendingVideoRoutine = StartCoroutine(SimulateVideoPlayback(CurrentScene));
            }
        }

        private IEnumerator PrepareAndPlay(FMVScene scene)
        {
            _videoPlayer.Prepare();

            float waited = 0f;
            while (!_videoPlayer.isPrepared)
            {
                // Clamp so time spent paused or backgrounded (a huge first delta on resume) doesn't count against the budget.
                waited += Mathf.Min(Time.unscaledDeltaTime, 0.25f);
                if (waited >= PrepareTimeoutSeconds)
                {
                    Debug.LogError($"Video for scene '{scene.Id}' did not prepare within {PrepareTimeoutSeconds}s; skipping it.");
                    _pendingVideoRoutine = null;
                    HandleVideoFinished(scene);
                    yield break;
                }
                yield return null;
            }

            _pendingVideoRoutine = null;
            _videoPlayer.Play();
            OnVideoPlaybackStarted?.Invoke(_videoPlayer.texture);
        }

        private IEnumerator SimulateVideoPlayback(FMVScene scene)
        {
            yield return new WaitForSeconds(3.0f);
            _pendingVideoRoutine = null;
            HandleVideoFinished(scene);
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogError($"Video error in scene '{CurrentScene?.Id}': {message}");
            if (_pendingVideoRoutine != null)
            {
                StopCoroutine(_pendingVideoRoutine);
                _pendingVideoRoutine = null;
            }
            HandleVideoFinished(CurrentScene);
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
                _videoPlayer.errorReceived -= OnVideoError;
            }
        }
    }
}
