using System;
using JetBrains.Annotations;
using Madduck.Shared;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public class WeatherFactory : IGenericFactory<WeatherType>
    {
        private readonly WeatherWeightTableInstance _weatherWeightTable;

        public WeatherType Current
        {
            get => !_generated ? Create() : _current;
            private set
            {
                _generated = true;
                _current = value;
            }
        }
        private WeatherType _current;
        private bool _generated;
        
        [Inject]
        public WeatherFactory(
            WeatherWeightTableInstance weatherWeightTable)
        {
            _weatherWeightTable = weatherWeightTable;
        }
        
        public WeatherType Create()
        {
            Current = _weatherWeightTable.GetRandomItem();
            return Current;
        } 
    }

    [Serializable]
    public class WeatherFactoryMock : IGenericFactory<WeatherType>
    {
        [UnflagEnum, 
         SerializeField] private WeatherType fixedWeather;

        public WeatherType Current => fixedWeather;
        public WeatherFactoryMock(){} // For inspector serialization
        public WeatherFactoryMock(WeatherType fixedWeather)
        {
            this.fixedWeather = fixedWeather;
        }
        
        public WeatherType Create() => fixedWeather;
    }
}