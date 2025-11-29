using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Core;
using Madduck.Day;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Save;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using DisposableBag = R3.DisposableBag;

namespace Madduck.Room
{
    [Serializable]
    public class FishingRoomManager : 
        IDisposable,
        IRequestHandler<CanContinueFishingRequest, bool>
    {
        #region Inspector
        [field: SerializeField] 
        public SerializableReactiveProperty<uint> CurrentFishCount { get; private set; } = new();
        [field: SerializeField] 
        public SerializableReactiveProperty<uint> MaxFishCount { get; private set; } = new();

        [Button("Change Fish Count")]
        private void DebugChangeFishCount(int change) => ChangeFishCount(change);
    
        #endregion

        internal event Action OnFishingRoomStarted;
        internal event Action<FishableCaughtEvent> OnFishableCaught;

        #region Fields
        private readonly FishingRoomConfig _config;
        private readonly LoadSceneManager _loadSceneManager;
        private readonly IAudioManager _audioManager;
        private readonly IFactory<uint> _maxFishCountFactory;
        private readonly ISubscriber<FishingStateEvent> _fishingStateEventSubscriber;
        private readonly IPublisher<FishingRoomStartedEvent> _fishingRoomStartedEventPublisher;
        private readonly IPublisher<FishingRoomEndedEvent> _fishingRoomEndedEventPublisher;
        
        private CancellationTokenSource _bgmCts = new();
        private AudioReference _previousBgm;
        private AudioReference _bgm;
        private AudioReference _ambient;
        private IDisposable _subscriptions;
        private DisposableBag _disposables;

        internal bool bossCaught;

        #endregion
        
        #region Injection

        [Inject]
        public FishingRoomManager(
            FishingRoomConfig config,
            LoadSceneManager loadSceneManager,
            IAudioManager audioManager,
            [Key(DIConstants.MaxFishCountFactoryId)] IFactory<uint> maxFishCountFactory,
            ISubscriber<FishingStateEvent> fishingStateEventSubscriber,
            IPublisher<FishingRoomStartedEvent> fishingRoomStartedEventPublisher,
            IPublisher<FishingRoomEndedEvent> fishingRoomEndedEventPublisher)
        {
            _config = config;
            _loadSceneManager = loadSceneManager;
            _audioManager = audioManager;
            _maxFishCountFactory = maxFishCountFactory;
            _fishingStateEventSubscriber = fishingStateEventSubscriber;
            _fishingRoomStartedEventPublisher = fishingRoomStartedEventPublisher;
            _fishingRoomEndedEventPublisher = fishingRoomEndedEventPublisher;
            Subscribe();
        }

        #endregion

        #region Subscriptions

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions.Dispose();
            _disposables.Dispose();
        }

        #endregion

        #region Events

        internal void StartFishingRoom()
        {
            MaxFishCount.Value = _maxFishCountFactory.Create();
            CurrentFishCount.Value = MaxFishCount.Value;
            _fishingRoomStartedEventPublisher?.Publish(new FishingRoomStartedEvent());
            if (Percentage.TryRoll(_config.BgmChance))
            {
                var randomBgm = _config.BgmPlaylist[UnityEngine.Random.Range(0, _config.BgmPlaylist.Count)];
                UniTask.WaitForSeconds(_config.BgmDelay, cancellationToken: _bgmCts.Token).ContinueWith(() =>
                {
                    _bgm = _audioManager.PlayAudio(randomBgm, Vector3.zero);
                });
            }
            _ambient = _audioManager.PlayAudio(_config.SeaAmbient, Vector3.zero);
            OnFishingRoomStarted?.Invoke();
        }

        internal void OnSceneFadeOut()
        {
            _bgmCts.Cancel();
            _bgmCts = new();
            _audioManager.StopAudio(_bgm);
            _audioManager.StopAudio(_ambient);
        }

        [Button("To Main Menu")]
        internal void ToMainMenu()
        {
            DebugUtils.Log("Returning to Main Menu...");
            Time.timeScale = 1;
            GameConstants.SetGameState(GameState.Normal);
            _loadSceneManager.LoadScene(SceneType.MainMenu, LoadSceneMode.Single, false).Forget();
        }

        internal void FishEscaped(FishEscapedEvent eventData)
        {
            ChangeFishCount(-1);
            if (eventData.FishItemInstance.ItemData.EnemyType is FishEnemyType.Boss)
            {
                if (_previousBgm == null) return;
                _audioManager.StopAudio(_bgm);
                _bgm = _audioManager.PlayAudio(_previousBgm.eventReference, Vector3.zero);
                _previousBgm = null;
                return;
            }
            if (CurrentFishCount.Value != 0) return;
            //_modalManager.Queue(_cardSelectionController); //NOTE: Disable for now
            _fishingStateEventSubscriber
                .AsObservable().ToObservable()
                .Where(x => x.StateType is FishingStateType.None)
                .Subscribe(_ => EndFishingRoom())
                .AddTo(ref _disposables);
        }

        internal void FishEmerged(FishEmergedEvent eventData)
        {
            if (eventData.FishItemInstance.ItemData.EnemyType is not FishEnemyType.Boss) return;
            _previousBgm = _bgm;
            _audioManager.StopAudio(_bgm);
            _bgm = _audioManager.PlayAudio(_config.BossBgm, Vector3.zero);
        }

        internal void FishCaught(FishableCaughtEvent eventData)
        {
            OnFishableCaught?.Invoke(eventData);
            var anyBoss = eventData.FishableItemInstances
                .OfType<FishItemInstance>()
                .Any(x => x.ItemData.EnemyType is FishEnemyType.Boss);
            if (!anyBoss) return;
            if (_previousBgm == null) return;
            _audioManager.StopAudio(_bgm);
            _bgm = _audioManager.PlayAudio(_previousBgm.eventReference, Vector3.zero);
            _previousBgm = null;
        }

        #endregion

        #region Request

        public bool Invoke(CanContinueFishingRequest request)
        {
            return CurrentFishCount.Value > 0 && !bossCaught;
        }

        #endregion

        #region Utils

        internal void ChangeFishCount(int change)
        {
            CurrentFishCount.Value =
                (uint)Mathf.Clamp((int)CurrentFishCount.Value + change, 0, (int)MaxFishCount.Value);
        }

        internal void EndFishingRoom()
        {
            _disposables.Dispose();
            _fishingRoomEndedEventPublisher.Publish(new FishingRoomEndedEvent());
        }

        #endregion
    }
}