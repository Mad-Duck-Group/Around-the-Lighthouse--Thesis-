using Madduck.Day;
using Madduck.GameData;
using Madduck.Shared;
using Sirenix.OdinInspector;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    public class FishingRoomManager : IStartable
    {
        [Title("Debug")]
        [DisplayAsString, 
         ShowInInspector] private WeatherType _currentWeather;
        
        private readonly WeatherWeightTableInstance _weatherWeightTable;

        [Button("Next Weather")]
        private void NextWeather() => RandomWeather();
        
        private readonly DayManager _dayManager;
        
        [Inject]
        public FishingRoomManager(
            DayManager dayManager,
            WeatherWeightTableInstance weatherWeightTable)
        {
            _dayManager = dayManager;
            _weatherWeightTable = weatherWeightTable;
        }
        
        public void Start()
        {
            var testFilter = new WeatherWeightFilter(record => record.Item != WeatherType.Fog);
            var testModifier = new WeatherWeightModifier(record => record.Item == WeatherType.Rain ? 2f : 0f);
            _weatherWeightTable.PersistentFilters.TryAdd("NoFogFilter", testFilter);
            _weatherWeightTable.PersistentModifiers.TryAdd("RainBoostModifier", testModifier);
            RandomWeather();
        }

        private void RandomWeather()
        {
            _currentWeather = _weatherWeightTable.GetRandomItem();
            FilterFishByWeather();
        }
        
        private void FilterFishByWeather()
        {
            _dayManager.FishWeightTable.PersistentFilters.Remove("WeatherFilter");
            var filter = new FishWeightFilter(record => record.Item.WeatherType.HasFlag(_currentWeather));
            _dayManager.FishWeightTable.PersistentFilters.TryAdd("WeatherFilter", filter);
        }
    }
}