using System;
using Madduck.GameData;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.Room
{
    [Serializable]
    public class FishingRoomWeatherHandler : IDisposable
    {
        [Title("Debug")]
        [ShowInInspector] private WeatherItemInstance _currentWeather;
        
        [Button("Next Weather")]
        private void NextWeather() => RandomWeather();

        private readonly FishingRoomManager _fishingRoomManager;
        private readonly CompositeWeightTableInstance _fishableWeightTable;
        private readonly IFactory<WeatherItemInstance> _weatherFactory;
        private readonly IPublisher<WeatherChangedEvent> _weatherChangedPublisher;
        
        private IDisposable _subscriptions;

        [Inject]
        public FishingRoomWeatherHandler(
            FishingRoomManager fishingRoomManager,
            [Key(ModifierKeys.FishableKey)] CompositeWeightTableInstance fishableWeightTable,
            IFactory<WeatherItemInstance> weatherFactory,
            IPublisher<WeatherChangedEvent> weatherChangedPublisher)
        {
            _fishingRoomManager = fishingRoomManager;
            _fishableWeightTable = fishableWeightTable;
            _weatherFactory = weatherFactory;
            _weatherChangedPublisher = weatherChangedPublisher;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            Observable.FromEvent(
                    h => _fishingRoomManager.OnFishingRoomStarted += h, 
                    h => _fishingRoomManager.OnFishingRoomStarted -= h)
                .Subscribe(_ => RandomWeather())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions.Dispose();
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
    }
}