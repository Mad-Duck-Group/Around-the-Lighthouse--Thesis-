using System;
using Madduck.Shared;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public class WeatherFactory : IGenericFactory<WeatherType>
    {
        private readonly WeatherWeightTableInstance _weatherWeightTable;
        
        [Inject]
        public WeatherFactory(WeatherWeightTableInstance weatherWeightTable)
        {
            _weatherWeightTable = weatherWeightTable;
        }

        public WeatherType Create() => _weatherWeightTable.GetRandomItem();
    }

    [Serializable]
    public class WeatherFactoryMock : IGenericFactory<WeatherType>
    {
        [SerializeField] private WeatherType fixedWeather;

        public WeatherFactoryMock(){} // For inspector serialization
        public WeatherFactoryMock(WeatherType fixedWeather)
        {
            this.fixedWeather = fixedWeather;
        }
        
        public WeatherType Create() => fixedWeather;
    }
}