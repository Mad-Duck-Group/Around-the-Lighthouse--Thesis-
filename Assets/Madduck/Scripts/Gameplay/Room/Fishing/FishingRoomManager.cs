using System;
using Madduck.Day;
using Madduck.GameData;
using Madduck.GameData.Fisherman;
using Madduck.Shared;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    public class FishingRoomManager : 
        IStartable, IDisposable,
        IRequestHandler<CanContinueFishingRequest, bool>
    {
        [Title("Debug")]
        [DisplayAsString, 
         ShowInInspector] private WeatherType _currentWeather;
        
        [field: SerializeField] 
        public ReactiveProperty<uint> CurrentFishCount { get; private set; } = new();
        [field: SerializeField] 
        public ReactiveProperty<uint> MaxFishCount { get; private set; } = new();

        private readonly IGenericFactory<WeatherType> _weatherFactory;
        private readonly IGenericFactory<uint> _maxFishCountFactory ;
        private readonly IPublisher<FishingRoomStartedEvent> _fishingRoomStartedEventPublisher;
        private readonly IPublisher<OutOfFishEvent> _outOfFishEventPublisher;
        private readonly IPublisher<WeatherChangedEvent> _weatherChangedPublisher;
        private readonly ISubscriber<FishCaughtEvent> _fishCaughtEventSubscriber;
        private readonly ISubscriber<FishEscapedEvent> _fishEscapedEventSubscriber;

        private IDisposable _subscriptions;

        [Button("Next Weather")]
        private void NextWeather() => RandomWeather();
        
        private readonly FishWeightTableInstance _fishWeightTableInstance;
        
        [Inject]
        public FishingRoomManager(
            FishWeightTableInstance fishWeightTableInstanceInstance,
            IGenericFactory<WeatherType> weatherFactory,
            [Key(DIConstants.MaxFishCountFactoryId)] IGenericFactory<uint> maxFishCountFactory,
            IPublisher<FishingRoomStartedEvent> fishingRoomStartedEventPublisher,
            IPublisher<OutOfFishEvent> outOfFishEventPublisher,
            ISubscriber<FishCaughtEvent> fishCaughtEventSubscriber,
            ISubscriber<FishEscapedEvent> fishEscapedEventSubscriber,
            IPublisher<WeatherChangedEvent> weatherChangedPublisher)
        {
            _fishWeightTableInstance = fishWeightTableInstanceInstance;
            _weatherFactory = weatherFactory;
            _maxFishCountFactory = maxFishCountFactory;
            _fishingRoomStartedEventPublisher = fishingRoomStartedEventPublisher;
            _outOfFishEventPublisher = outOfFishEventPublisher;
            _fishCaughtEventSubscriber = fishCaughtEventSubscriber;
            _fishEscapedEventSubscriber = fishEscapedEventSubscriber;
            _weatherChangedPublisher = weatherChangedPublisher;
            Subscribe();
        }
        
        public void Start()
        {
            _fishingRoomStartedEventPublisher?.Publish(new FishingRoomStartedEvent());
            RandomWeather();
            MaxFishCount.Value = _maxFishCountFactory.Create();
            CurrentFishCount.Value = MaxFishCount.Value;
            
        }
        
        public void Dispose()
        {
            _subscriptions?.Dispose();
        }

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishCaughtEventSubscriber.Subscribe(_ => OnFishCaught())
                .AddTo(ref disposableBuilder);
            _fishEscapedEventSubscriber.Subscribe(_ => OnFishEscaped())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        private void OnFishCaught()
        {
            ChangeFishCount(-1);
        }
        
        private void OnFishEscaped()
        {
            ChangeFishCount(-1);
        }
        
        private void ChangeFishCount(int change)
        {
            CurrentFishCount.Value = (uint)Mathf.Clamp((int)CurrentFishCount.Value + change, 0, (int)MaxFishCount.Value);
            if (CurrentFishCount.Value == 0)
            {
                _outOfFishEventPublisher?.Publish(new OutOfFishEvent());
            }
        }
        public readonly struct WeatherChangedEvent
        {
            public WeatherType Weather { get; }
            public WeatherChangedEvent(WeatherType weather) => Weather = weather;
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

        public bool Invoke(CanContinueFishingRequest request)
        {
            return CurrentFishCount.Value > 0;
        }
    }
}