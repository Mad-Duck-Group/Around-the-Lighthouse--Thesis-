using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using MessagePipe;
using PrimeTween;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Madduck.Core
{

    #region Events
    public struct LoadSceneEvent
    {
        public readonly SceneType sceneType;
        public readonly LoadSceneMode loadSceneMode;
        public readonly bool useLoadingScene;
        
        public LoadSceneEvent(SceneType sceneType, LoadSceneMode loadSceneMode, bool useLoadingScene)
        {
            this.sceneType = sceneType;
            this.loadSceneMode = loadSceneMode;
            this.useLoadingScene = useLoadingScene;
        }
    }

    public struct LoadSceneStageEvent
    {
        public LoadSceneStage Stage { get; private set; }

        public LoadSceneStageEvent(LoadSceneStage stage)
        {
            Stage = stage;
        }
    }
    #endregion

    public enum SceneType
    {
        MainMenu,
        Loading,
        Gameplay
    }

    public enum LoadSceneStage
    {
        StartFadeOut,
        FinishFadeOut,
        StartLoading,
        FinishLoading,
        StartFadeIn,
        FinishFadeIn
    }
    
    [Serializable]
    public class LoadSceneManager : IDisposable, IStartable
    {
        #region Inspectors
        [Title("Debug")]
        [SerializeField] private SceneType debugSceneType;
        [Button("Debug Load Scene")]
        private void DebugLoadScene()
        {
            LoadScene(debugSceneType, LoadSceneMode.Single, false).Forget();
        }
        #endregion
        
        #region Fields and Properties
        private readonly LoadSceneManagerConfig _config;
        private readonly ITransitionable _currentTransitionScreen;
        private readonly ISubscriber<LoadSceneEvent> _loadSceneEventSubscriber;
        private readonly IPublisher<LoadSceneStageEvent> _loadSceneStageEventPublisher;
        
        private IDisposable _subscriptions;
        private Tween _fadeTween;
        private AsyncOperation _asyncOperation;
        private CancellationTokenSource _loadSceneCts;
        public string NextScene { get; private set; }
        public LoadSceneMode LoadSceneMode { get; private set; }
        public bool FirstSceneLoaded { get; private set; }
        #endregion

        [Inject]
        public LoadSceneManager(
            LoadSceneManagerConfig config,
            ITransitionable transitionScreen,
            ISubscriber<LoadSceneEvent> loadSceneEventSubscriber,
            IPublisher<LoadSceneStageEvent> loadSceneStageEventPublisher)
        {
            _config = config;
            _loadSceneEventSubscriber = loadSceneEventSubscriber;
            _loadSceneStageEventPublisher = loadSceneStageEventPublisher;
            _currentTransitionScreen = transitionScreen;
            Subscribe();
        }
        
        public void Start()
        {
            if (FirstSceneLoaded) return;
            _loadSceneStageEventPublisher.Publish(new LoadSceneStageEvent(LoadSceneStage.FinishLoading));
            _loadSceneStageEventPublisher.Publish(new LoadSceneStageEvent(LoadSceneStage.FinishFadeIn));
        }

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _loadSceneEventSubscriber.Subscribe(OnLoadSceneEvent)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
        
        private void OnLoadSceneEvent(LoadSceneEvent loadSceneEvent)
        {
            LoadScene(loadSceneEvent.sceneType, loadSceneEvent.loadSceneMode, loadSceneEvent.useLoadingScene).Forget();
        }
        
        #region Scene Loading
        
        public void ReloadScene(LoadSceneMode loadSceneMode, bool useLoadingScene)
        {
            var currentSceneName = SceneManager.GetActiveScene().path;
            var sceneType = _config.SceneReferences.FirstOrDefault(x => x.Value.Path == currentSceneName).Key;
            if (sceneType == default)
            {
                Debug.LogError($"Current scene '{currentSceneName}' not found in the dictionary.");
                return;
            }
            LoadScene(sceneType, loadSceneMode, useLoadingScene).Forget();
        }
        
        public async UniTaskVoid LoadScene(SceneType sceneType, LoadSceneMode loadSceneMode, bool useLoadingScene)
        {
            if (_asyncOperation is { isDone: false } || _fadeTween.isAlive) return;
            string sceneName;
            if (_config.SceneReferences.TryGetValue(sceneType, out var sceneReference))
            {
                sceneName = sceneReference.Path;
            }
            else
            {
                Debug.LogError($"Scene {sceneType} not found in the dictionary.");
                return;
            }
            NextScene = sceneName;
            LoadSceneMode = loadSceneMode;
            _loadSceneStageEventPublisher.Publish(new LoadSceneStageEvent(LoadSceneStage.StartFadeOut));
            await _currentTransitionScreen.TransitionIn();
            OnFadeOutComplete(useLoadingScene);
        }

        private void OnFadeOutComplete(bool useLoadingScene)
        {
            _loadSceneStageEventPublisher.Publish(new LoadSceneStageEvent(LoadSceneStage.FinishFadeOut));
            if (useLoadingScene)
            {
                string loadingScene;
                if (_config.SceneReferences.TryGetValue(SceneType.Loading, out SceneReference loadingSceneReference))
                {
                    loadingScene = loadingSceneReference.Path;
                }
                else
                {
                    Debug.LogError("Loading scene not found in the dictionary.");
                    return;
                }
                SceneManager.LoadScene(loadingScene);
            }
            else
            {
                _loadSceneCts = new CancellationTokenSource();
                LoadSceneAsync(_loadSceneCts.Token).Forget();
            }
        }

        private async UniTask LoadSceneAsync(CancellationToken cancellationToken = default)
        {
            SceneManager.activeSceneChanged += UnloadScene;
            _asyncOperation = SceneManager.LoadSceneAsync(NextScene, LoadSceneMode);
            if (_asyncOperation == null)
            {
                DebugUtils.LogError("Async operation is null.");
                return;
            }
            _loadSceneStageEventPublisher.Publish(new LoadSceneStageEvent(LoadSceneStage.StartLoading));
            _asyncOperation.allowSceneActivation = false;
            if (!_config.MinimumLoadingScreenDuration)
            {
                await UniTask.WaitUntil(() => _asyncOperation.progress >= 0.9f, cancellationToken: cancellationToken);
            }
            else
            {
                await UniTask.WhenAll(UniTask.WaitUntil(() => _asyncOperation.progress >= 0.9f, cancellationToken: cancellationToken),
                    UniTask.WaitForSeconds(_config.LoadingScreenDuration, ignoreTimeScale: true, cancellationToken: cancellationToken));
            }
            _asyncOperation.allowSceneActivation = true;
            SceneManager.sceneLoaded += SetActiveScene;
            FirstSceneLoaded = true;
            Time.timeScale = 1f;
            _asyncOperation = null;
        }
        
        private void SetActiveScene(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= SetActiveScene;
            SceneManager.SetActiveScene(scene);
            _loadSceneStageEventPublisher.Publish(new LoadSceneStageEvent(LoadSceneStage.FinishLoading));
        }

        public void CancelLoadScene()
        {
            _loadSceneCts?.Cancel();
        }

        private void UnloadScene(Scene lastScene, Scene current)
        {
            SceneManager.activeSceneChanged -= UnloadScene;
            UnloadSceneUniTask(lastScene, current).Forget();
        }

        private async UniTaskVoid UnloadSceneUniTask(Scene lastScene, Scene current)
        {
            Debug.Log("Unloading " + lastScene.name);
            if (LoadSceneMode == LoadSceneMode.Additive)
            {
                SceneManager.UnloadSceneAsync(lastScene);
            }
            _loadSceneStageEventPublisher.Publish(new LoadSceneStageEvent(LoadSceneStage.StartFadeIn));
            await _currentTransitionScreen.TransitionOut();
            _loadSceneStageEventPublisher.Publish(new LoadSceneStageEvent(LoadSceneStage.FinishFadeIn));
        }
        #endregion
    }
}