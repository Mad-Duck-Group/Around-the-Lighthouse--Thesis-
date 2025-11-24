using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Core;
using Madduck.Day;
using Madduck.GameData;
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

        [Title("Debug")]
        [ShowInInspector] private WeatherItemInstance _currentWeather;
        
        [field: SerializeField] 
        public SerializableReactiveProperty<uint> CurrentFishCount { get; private set; } = new();
        [field: SerializeField] 
        public SerializableReactiveProperty<uint> MaxFishCount { get; private set; } = new();
        
        [Button("Next Weather")]
        private void NextWeather() => RandomWeather();

        [Button("Change Fish Count")]
        private void DebugChangeFishCount(int change) => ChangeFishCount(change);
    
        #endregion

        #region Fields

        private readonly CompositeWeightTableInstance _fishableWeightTable;
        private readonly FishingRoomConfig _config;
        private readonly MessagePackSaveManager _saveManager;
        private readonly LoadSceneManager _loadSceneManager;
        private readonly DayManager _dayManager;
        private readonly IAudioManager _audioManager;
        private readonly IFactory<WeatherItemInstance> _weatherFactory;
        private readonly IFactory<uint> _maxFishCountFactory;
        private readonly IPublisher<FishingRoomStartedEvent> _fishingRoomStartedEventPublisher;
        private readonly IPublisher<FishingRoomEndedEvent> _fishingRoomEndedEventPublisher;
        private readonly IPublisher<WeatherChangedEvent> _weatherChangedPublisher;
        private readonly ISubscriber<FishingStateEvent> _fishingStateEventSubscriber;
        private readonly ISubscriber<FishEmergedEvent> _fishEmergedEventSubscriber;
        private readonly ISubscriber<FishEscapedEvent> _fishEscapedEventSubscriber;
        private readonly ISubscriber<FishableCaughtEvent> _fishCaughtEventSubscriber;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        private CancellationTokenSource _bgmCts = new();
        private AudioReference _previousBgm;
        private AudioReference _bgm;
        private AudioReference _ambient;
        private IDisposable _subscriptions;
        private DisposableBag _disposables;

        public bool bossCaught;

        #endregion
        
        #region Injection

        [Inject]
        public FishingRoomManager(
            [Key(ModifierKeys.FishableKey)] CompositeWeightTableInstance fishableWeightTable,
            FishingRoomConfig config,
            MessagePackSaveManager saveManager,
            LoadSceneManager loadSceneManager,
            DayManager dayManager,
            IAudioManager audioManager,
            IFactory<WeatherItemInstance> weatherFactory,
            [Key(DIConstants.MaxFishCountFactoryId)] IFactory<uint> maxFishCountFactory,
            IPublisher<FishingRoomStartedEvent> fishingRoomStartedEventPublisher,
            IPublisher<FishingRoomEndedEvent> fishingRoomEndedEventPublisher,
            IPublisher<WeatherChangedEvent> weatherChangedPublisher,
            ISubscriber<FishingStateEvent> fishingStateEventSubscriber,
            ISubscriber<FishEmergedEvent> fishEmergedEventSubscriber,
            ISubscriber<FishEscapedEvent> fishEscapedEventSubscriber,
            ISubscriber<FishableCaughtEvent> fishCaughtEventSubscriber,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber)
        {
            _fishableWeightTable = fishableWeightTable;
            _config = config;
            _saveManager = saveManager;
            _loadSceneManager = loadSceneManager;
            _dayManager = dayManager;
            _weatherFactory = weatherFactory;
            _maxFishCountFactory = maxFishCountFactory;
            _audioManager = audioManager;
            _fishingRoomStartedEventPublisher = fishingRoomStartedEventPublisher;
            _fishingRoomEndedEventPublisher = fishingRoomEndedEventPublisher;
            _weatherChangedPublisher = weatherChangedPublisher;
            _fishingStateEventSubscriber = fishingStateEventSubscriber;
            _fishEmergedEventSubscriber = fishEmergedEventSubscriber;
            _fishEscapedEventSubscriber = fishEscapedEventSubscriber;
            _fishCaughtEventSubscriber = fishCaughtEventSubscriber;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            Subscribe();
        }

        #endregion

        #region Subscriptions

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishEscapedEventSubscriber.Subscribe(OnFishEscaped)
                .AddTo(ref disposableBuilder);
            _fishEmergedEventSubscriber.Subscribe(OnFishEmerged)
                .AddTo(ref disposableBuilder);
            _fishCaughtEventSubscriber.Subscribe(OnFishCaught)
                .AddTo(ref disposableBuilder);
            _loadSceneStageEventSubscriber
                .AsObservable().ToObservable()
                .Where(x => x.Stage is LoadSceneStage.FinishLoading)
                .Subscribe(_ => OnStartFishingRoom())
                .AddTo(ref disposableBuilder);
            _loadSceneStageEventSubscriber
                .AsObservable().ToObservable()
                .Where(x => x.Stage is LoadSceneStage.StartFadeOut)
                .Subscribe(_ => OnEndFishingRoom())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions?.Dispose();
            _disposables.Dispose();
        }

        #endregion

        #region Events

        private void OnStartFishingRoom()
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
            RandomWeather();
        }

        private void OnEndFishingRoom()
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
            _saveManager.ResetAll();
            _saveManager.SaveAll();
            _dayManager.SetDayIndex(0);
            _loadSceneManager.LoadScene(SceneType.MainMenu, LoadSceneMode.Single, false).Forget();
        }
        
        private void OnFishEscaped(FishEscapedEvent eventData)
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
        
        private void OnFishEmerged(FishEmergedEvent eventData)
        {
            if (eventData.FishItemInstance.ItemData.EnemyType is not FishEnemyType.Boss) return;
            _previousBgm = _bgm;
            _audioManager.StopAudio(_bgm);
            _bgm = _audioManager.PlayAudio(_config.BossBgm, Vector3.zero);
        }
        
        private void OnFishCaught(FishableCaughtEvent eventData)
        {
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

        private void RandomWeather()
        {
            _currentWeather = _weatherFactory.Create();
            _weatherChangedPublisher.Publish(new WeatherChangedEvent(_currentWeather));
            FilterFishByWeather();
        }
        
        private void FilterFishByWeather()
        {
            if (!_fishableWeightTable.TryGetFirstInstanceOfType<FishWeightTableInstance>(out var fishWeightTableInstance)) return;
            if (fishWeightTableInstance == null) return;
            fishWeightTableInstance.PersistentFilters.Remove("WeatherFilter");
            var filter = new FishWeightFilter(record => record.Item.WeatherType.HasFlag(_currentWeather.ItemData.WeatherType));
            fishWeightTableInstance.PersistentFilters.TryAdd("WeatherFilter", filter);
        }

        #endregion
    }

    public class FishingRoomPopUpHandler : IDisposable
    {
        private readonly FishingRoomManager _fishingRoomManager;
        private readonly FishCatalogue _fishCatalogue;
        private readonly IModalManager _modalManager;
        private readonly IModal _cardSelectionController;
        private readonly IPopUpFactory<FishableItemPopUpObject> _fishableItemPopUpFactory;
        private readonly IPopUpFactory<NewFishPopUpObject> _newFishPopUpFactory;
        private readonly IPopUpFactory<EndGamePopUpObject> _endGamePopUpFactory;
        private readonly ISubscriber<FishableCaughtEvent> _fishCaughtEventSubscriber;
        
        private IDisposable _subscriptions;
        private DisposableBag _disposables = new();

        [Inject]
        public FishingRoomPopUpHandler(
            FishingRoomManager fishingRoomManager,
            FishCatalogue fishCatalogue,
            IModalManager modalManager,
            IModal cardSelectionController,
            IPopUpFactory<FishableItemPopUpObject> fishableItemPopUpFactory,
            IPopUpFactory<NewFishPopUpObject> newFishPopUpFactory,
            IPopUpFactory<EndGamePopUpObject> endGamePopUpFactory,
            ISubscriber<FishableCaughtEvent> fishCaughtEventSubscriber)
        {
            _fishingRoomManager = fishingRoomManager;
            _fishCatalogue = fishCatalogue;
            _modalManager = modalManager;
            _cardSelectionController = cardSelectionController;
            _fishableItemPopUpFactory = fishableItemPopUpFactory;
            _newFishPopUpFactory = newFishPopUpFactory;
            _endGamePopUpFactory = endGamePopUpFactory;
            _fishCaughtEventSubscriber = fishCaughtEventSubscriber;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishCaughtEventSubscriber
                .Subscribe(OnFishableCaught)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions?.Dispose();
            _disposables.Dispose();
        }

        private void OnFishableCaught(FishableCaughtEvent eventData)
        {
            HandlePopUp(eventData.FishableItemInstances, out var gotBoss);
            if (gotBoss)
            {
                _fishingRoomManager.bossCaught = true;
                Observable.FromEvent(
                        h => _modalManager.OnAllModalsClosed += h,
                        h => _modalManager.OnAllModalsClosed -= h)
                    .Subscribe(_ => _fishingRoomManager.ToMainMenu())
                    .AddTo(ref _disposables);
                return;
            }
            //_modalManager.Queue(_cardSelectionController); //NOTE: Disable for now
            if (_fishingRoomManager.CurrentFishCount.Value != 0) return;
            Observable.FromEvent(
                    h => _modalManager.OnAllModalsClosed += h,
                    h => _modalManager.OnAllModalsClosed -= h)
                .Subscribe(_ => _fishingRoomManager.EndFishingRoom())
                .AddTo(ref _disposables);
        }

        private void HandlePopUp(List<IFishableItemInstance> fishableItemInstances, out bool gotBoss)
        {
            gotBoss = false;
            var unCaughtFishItems = new List<FishItemInstance>();
            var others = new List<IFishableItemInstance>();
            var fishCount = 0;
            foreach (var fishable in fishableItemInstances)
            {
                switch (fishable)
                {
                    case FishItemInstance fishItemInstance:
                        if (fishItemInstance.ItemData.EnemyType is FishEnemyType.Boss)
                        {
                            gotBoss = true;
                            var popUp = _endGamePopUpFactory.Create();
                            popUp.SetPopUpObject(new EndGamePopUpObject());
                            _modalManager.Queue(popUp);
                            return;
                        }
                        var fishGuid = fishItemInstance.ItemData.Guid;
                        if (_fishCatalogue.HasCaught(fishGuid))
                        {
                            others.Add(fishItemInstance);
                        }
                        else
                        {
                            unCaughtFishItems.Add(fishItemInstance);
                        }
                        fishCount++;
                        break;
                    case ResourceItemInstance resourceItemInstance:
                        others.Add(resourceItemInstance);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(fishable));
                }
            }
            foreach (var unCaught in unCaughtFishItems)
            {
                var popUp = _newFishPopUpFactory.Create();
                popUp.SetPopUpObject(new NewFishPopUpObject(unCaught));
                _modalManager.Queue(popUp);
                _fishCatalogue.SetCaught(unCaught.ItemData.Guid);
                _fishCatalogue.Save();
            }
            var chunked = others.Chunk(3).Select(x => x.ToList()).ToList();
            foreach (var chunk in chunked)
            {
                if (chunk.Count <= 0) continue;
                var popUp = _fishableItemPopUpFactory.Create();
                popUp.SetPopUpObject(new FishableItemPopUpObject(chunk));
                _modalManager.Queue(popUp);
            }
            _fishingRoomManager.ChangeFishCount(-fishCount);
        }
    }
}