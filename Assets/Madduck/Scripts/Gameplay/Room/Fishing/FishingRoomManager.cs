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
        [DisplayAsString, 
         ShowInInspector] private DayPhaseType _currentDayPhase = DayPhaseType.Day;
        [ReadOnly, 
         ShowInInspector] private readonly WeatherWeightTableInstance _weatherWeightTable;
        [Button("Next Weather")]
        private void NextWeather() => _currentWeather = _weatherWeightTable.GetRandomItem();
        
        [Inject]
        public FishingRoomManager(WeatherWeightTableInstance weatherWeightTable)
        {
            _weatherWeightTable = weatherWeightTable;
        }
        
        public void Start()
        {
            var testFilter = new WeatherWeightFilter(record => record.Item != WeatherType.Fog);
            var testModifier = new WeatherWeightModifier(record => record.Item == WeatherType.Rain ? 2f : 0f);
            _weatherWeightTable.PersistentFilters.TryAdd("NoFogFilter", testFilter);
            _weatherWeightTable.PersistentModifiers.TryAdd("RainBoostModifier", testModifier);
            _currentWeather = _weatherWeightTable.GetRandomItem();
        }
    }
}