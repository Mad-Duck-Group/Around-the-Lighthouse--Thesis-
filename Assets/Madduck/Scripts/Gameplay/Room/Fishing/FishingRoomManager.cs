using System;
using Madduck.Audio;
using Madduck.Core;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Shared.Events;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Madduck.Room
{
    [Serializable]
    public class FishingRoomManager : 
        IDisposable,
        IRequestHandler<CanContinueFishingRequest, bool>
    {
        #region Inspector

        [Title("Debug")]
        [DisplayAsString, 
         ShowInInspector] private WeatherType _currentWeather;
        
        [field: SerializeField] 
        public SerializableReactiveProperty<uint> CurrentFishCount { get; private set; } = new();
        [field: SerializeField] 
        public SerializableReactiveProperty<uint> MaxFishCount { get; private set; } = new();
        
        [Button("Next Weather")]
        private void NextWeather() => RandomWeather();

        #endregion

        #region Fields

        private readonly FishWeightTableInstance _fishWeightTableInstance;
        private readonly FishingRoomConfig _config;
        private readonly IAudioManager _audioManager;
        private readonly IGenericFactory<WeatherType> _weatherFactory;
        private readonly IGenericFactory<uint> _maxFishCountFactory ;
        private readonly IPublisher<FishingRoomStartedEvent> _fishingRoomStartedEventPublisher;
        private readonly IPublisher<OutOfFishEvent> _outOfFishEventPublisher;
        private readonly IPublisher<WeatherChangedEvent> _weatherChangedPublisher;
        private readonly ISubscriber<FishCaughtEvent> _fishCaughtEventSubscriber;
        private readonly ISubscriber<FishEscapedEvent> _fishEscapedEventSubscriber;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        private AudioReference _bgm;
        private IDisposable _subscriptions;

        #endregion
        
        #region Injection

        [Inject]
        public FishingRoomManager(
            FishWeightTableInstance fishWeightTableInstanceInstance,
            FishingRoomConfig config,
            IAudioManager audioManager,
            IGenericFactory<WeatherType> weatherFactory,
            [Key(DIConstants.MaxFishCountFactoryId)] IGenericFactory<uint> maxFishCountFactory,
            IPublisher<FishingRoomStartedEvent> fishingRoomStartedEventPublisher,
            IPublisher<OutOfFishEvent> outOfFishEventPublisher,
            IPublisher<WeatherChangedEvent> weatherChangedPublisher,
            ISubscriber<FishCaughtEvent> fishCaughtEventSubscriber,
            ISubscriber<FishEscapedEvent> fishEscapedEventSubscriber,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber)
        {
            _fishWeightTableInstance = fishWeightTableInstanceInstance;
            _config = config;
            _weatherFactory = weatherFactory;
            _maxFishCountFactory = maxFishCountFactory;
            _audioManager = audioManager;
            _fishingRoomStartedEventPublisher = fishingRoomStartedEventPublisher;
            _outOfFishEventPublisher = outOfFishEventPublisher;
            _weatherChangedPublisher = weatherChangedPublisher;
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
            _fishCaughtEventSubscriber.Subscribe(_ => OnFishCaught())
                .AddTo(ref disposableBuilder);
            _fishEscapedEventSubscriber.Subscribe(_ => OnFishEscaped())
                .AddTo(ref disposableBuilder);
            _loadSceneStageEventSubscriber
                .AsObservable().ToObservable()
                .Where(x => x.Stage is LoadSceneStage.FinishLoading)
                .Subscribe(_ => OnStartFishingRoom())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions?.Dispose();
        }

        #endregion

        #region Events

        private void OnStartFishingRoom()
        {
            MaxFishCount.Value = _maxFishCountFactory.Create();
            CurrentFishCount.Value = MaxFishCount.Value;
            _fishingRoomStartedEventPublisher?.Publish(new FishingRoomStartedEvent());
            _bgm = _audioManager.PlayAudio(_config.FishingRoomBGM, Vector3.zero);
            RandomWeather();
        }
        
        private void OnFishCaught()
        {
            ChangeFishCount(-1);
        }
        
        private void OnFishEscaped()
        {
            ChangeFishCount(-1);
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
            if (CurrentFishCount.Value != 0) return;
            _audioManager.StopAudio(_bgm);
            _outOfFishEventPublisher?.Publish(new OutOfFishEvent());
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
            var filter = new FishWeightFilter(record => record.Item.WeatherType.HasFlag(_currentWeather));
            _fishWeightTableInstance.PersistentFilters.TryAdd("WeatherFilter", filter);
        }

        #endregion
    }
}