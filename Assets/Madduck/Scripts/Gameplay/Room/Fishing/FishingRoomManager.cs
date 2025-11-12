using System;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Core;
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

        private readonly FishWeightTableInstance _fishWeightTableInstance;
        private readonly FishingRoomConfig _config;
        private readonly FishCatalogue _fishCatalogue;
        private readonly IModal _cardSelectionController;
        private readonly IAudioManager _audioManager;
        private readonly IModalManager _modalManager;
        private readonly IGenericFactory<WeatherItemInstance> _weatherFactory;
        private readonly IGenericFactory<uint> _maxFishCountFactory;
        private readonly IPopUpFactory<FishItemPopUpObject> _fishItemPopUpFactory;
        private readonly IPopUpFactory<NewFishPopUpObject> _newFishPopUpFactory;
        private readonly IPublisher<FishingRoomStartedEvent> _fishingRoomStartedEventPublisher;
        private readonly IPublisher<FishingRoomEndedEvent> _fishingRoomEndedEventPublisher;
        private readonly IPublisher<WeatherChangedEvent> _weatherChangedPublisher;
        private readonly ISubscriber<FishingStateEvent> _fishingStateEventSubscriber;
        private readonly ISubscriber<FishCaughtEvent> _fishCaughtEventSubscriber;
        private readonly ISubscriber<FishEscapedEvent> _fishEscapedEventSubscriber;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        private AudioReference _bgm;
        private AudioReference _ambient;
        private IDisposable _subscriptions;
        private DisposableBag _disposables;

        #endregion
        
        #region Injection

        [Inject]
        public FishingRoomManager(
            FishWeightTableInstance fishWeightTableInstanceInstance,
            FishingRoomConfig config,
            FishCatalogue fishCatalogue,
            IModal cardSelectionController,
            IAudioManager audioManager,
            IModalManager modalManager,
            IGenericFactory<WeatherItemInstance> weatherFactory,
            [Key(DIConstants.MaxFishCountFactoryId)] IGenericFactory<uint> maxFishCountFactory,
            IPopUpFactory<FishItemPopUpObject> fishItemPopUpFactory,
            IPopUpFactory<NewFishPopUpObject> newFishPopUpFactory,
            IPublisher<FishingRoomStartedEvent> fishingRoomStartedEventPublisher,
            IPublisher<FishingRoomEndedEvent> fishingRoomEndedEventPublisher,
            IPublisher<WeatherChangedEvent> weatherChangedPublisher,
            ISubscriber<FishingStateEvent> fishingStateEventSubscriber,
            ISubscriber<FishCaughtEvent> fishCaughtEventSubscriber,
            ISubscriber<FishEscapedEvent> fishEscapedEventSubscriber,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber)
        {
            _fishWeightTableInstance = fishWeightTableInstanceInstance;
            _config = config;
            _fishCatalogue = fishCatalogue;
            _cardSelectionController = cardSelectionController;
            _weatherFactory = weatherFactory;
            _maxFishCountFactory = maxFishCountFactory;
            _fishItemPopUpFactory = fishItemPopUpFactory;
            _newFishPopUpFactory = newFishPopUpFactory;
            _audioManager = audioManager;
            _modalManager = modalManager;
            _fishingRoomStartedEventPublisher = fishingRoomStartedEventPublisher;
            _fishingRoomEndedEventPublisher = fishingRoomEndedEventPublisher;
            _weatherChangedPublisher = weatherChangedPublisher;
            _fishingStateEventSubscriber = fishingStateEventSubscriber;
            _fishCaughtEventSubscriber = fishCaughtEventSubscriber;
            _fishEscapedEventSubscriber = fishEscapedEventSubscriber;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            Subscribe();
        }

        #endregion

        #region Subscriptions

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishCaughtEventSubscriber.Subscribe(OnFishCaught)
                .AddTo(ref disposableBuilder);
            _fishEscapedEventSubscriber.Subscribe(_ => OnFishEscaped())
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
                _bgm = _audioManager.PlayAudio(randomBgm, Vector3.zero);
            }
            _ambient = _audioManager.PlayAudio(_config.SeaAmbient, Vector3.zero);
            RandomWeather();
        }

        private void OnEndFishingRoom()
        {
            _audioManager.StopAudio(_bgm);
            _audioManager.StopAudio(_ambient);
        }
        
        private void OnFishCaught(FishCaughtEvent eventData)
        {
            var fishGuid = eventData.FishItemInstance.ItemData.Guid;
            if (_fishCatalogue.HasCaught(fishGuid))
            {
                var popUp = _fishItemPopUpFactory.Create();
                popUp.SetPopUpObject(new FishItemPopUpObject(eventData.FishItemInstance));
                _modalManager.Queue(popUp);
            }
            else
            {
                var popUp = _newFishPopUpFactory.Create();
                popUp.SetPopUpObject(new NewFishPopUpObject(eventData.FishItemInstance));
                _modalManager.Queue(popUp);
                _fishCatalogue.SetCaught(fishGuid);
                _fishCatalogue.Save();
            }
            ChangeFishCount(-1);
            if (CurrentFishCount.Value != 0) return;
            //_modalManager.Queue(_cardSelectionController); //NOTE: Disable for now
            Observable.FromEvent(
                    h => _modalManager.OnAllModalsClosed += h,
                    h => _modalManager.OnAllModalsClosed -= h)
                .Subscribe(_ => OnFishingRoomEnded())
                .AddTo(ref _disposables);
        }
        
        private void OnFishEscaped()
        {
            ChangeFishCount(-1);
            if (CurrentFishCount.Value != 0) return;
            //_modalManager.Queue(_cardSelectionController); //NOTE: Disable for now
            _fishingStateEventSubscriber
                .AsObservable().ToObservable()
                .Where(x => x.StateType is FishingStateType.None)
                .Subscribe(_ => OnFishingRoomEnded())
                .AddTo(ref _disposables);
        }

        #endregion

        #region Request

        public bool Invoke(CanContinueFishingRequest request)
        {
            return CurrentFishCount.Value > 0;
        }

        #endregion

        #region Utils

        private void ChangeFishCount(int change)
        {
            CurrentFishCount.Value =
                (uint)Mathf.Clamp((int)CurrentFishCount.Value + change, 0, (int)MaxFishCount.Value);
        }

        private void OnFishingRoomEnded()
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
            _fishWeightTableInstance.PersistentFilters.Remove("WeatherFilter");
            var filter = new FishWeightFilter(record => record.Item.WeatherType.HasFlag(_currentWeather.ItemData.WeatherType));
            _fishWeightTableInstance.PersistentFilters.TryAdd("WeatherFilter", filter);
        }

        #endregion
    }
}