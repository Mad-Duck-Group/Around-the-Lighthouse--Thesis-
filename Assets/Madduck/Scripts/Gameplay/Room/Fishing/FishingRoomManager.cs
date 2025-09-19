using System;
using Madduck.Day;
using Madduck.GameData;
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
        [DisplayAsString, 
         ShowInInspector] private uint _currentFishCount;
        [DisplayAsString,
         ShowInInspector] private uint _maxFishCount;

        private readonly IGenericFactory<WeatherType> _weatherFactory;
        private readonly IGenericFactory<uint> _maxFishCountFactory;
        private readonly IPublisher<OutOfFishEvent> _outOfFishEventPublisher;
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
            IPublisher<OutOfFishEvent> outOfFishEventPublisher,
            ISubscriber<FishCaughtEvent> fishCaughtEventSubscriber,
            ISubscriber<FishEscapedEvent> fishEscapedEventSubscriber)
        {
            _fishWeightTableInstance = fishWeightTableInstanceInstance;
            _weatherFactory = weatherFactory;
            _maxFishCountFactory = maxFishCountFactory;
            _outOfFishEventPublisher = outOfFishEventPublisher;
            _fishCaughtEventSubscriber = fishCaughtEventSubscriber;
            _fishEscapedEventSubscriber = fishEscapedEventSubscriber;
            Subscribe();
        }
        
        public void Start()
        {
            RandomWeather();
            _maxFishCount = _maxFishCountFactory.Create();
            _currentFishCount = _maxFishCount;
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
            _currentFishCount = (uint)Mathf.Clamp(_currentFishCount + change, 0, _maxFishCount);
            if (_currentFishCount == 0)
            {
                _outOfFishEventPublisher?.Publish(new OutOfFishEvent());
            }
        }

        private void RandomWeather()
        {
            _currentWeather = _weatherFactory.Create();
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
            return _currentFishCount > 0;
        }
    }
}