using System;
using JetBrains.Annotations;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public class WeatherFactory : IGenericFactory<WeatherItemInstance>
    {
        private readonly WeatherWeightTableInstance _weatherWeightTable;

        public WeatherItemInstance Current
        {
            get => !_generated ? Create() : _current;
            private set
            {
                _generated = true;
                _current = value;
            }
        }
        private WeatherItemInstance _current;
        private readonly IModifierSource _modifierSource;
        private readonly IPublisher<ModifierSourceEvent> _modifierSourceEventPublisher;
        private bool _generated;
        
        [Inject]
        public WeatherFactory(
            WeatherWeightTableInstance weatherWeightTable,
            IModifierSource modifierSource,
            IPublisher<ModifierSourceEvent> modifierSourceEventPublisher)

        {
            _weatherWeightTable = weatherWeightTable;
            _modifierSourceEventPublisher = modifierSourceEventPublisher;
            _modifierSource = modifierSource;
        }
        
        public WeatherItemInstance Create()
        {
            var random = _weatherWeightTable.GetRandomItem();
            Current = new WeatherItemInstance(random, _modifierSource, _modifierSourceEventPublisher);
            return Current;
        } 
    }

    [Serializable]
    public class WeatherFactoryMock : IGenericFactory<WeatherItemInstance>
    {
        [SerializeField] private WeatherItemData fixedWeather;

        public WeatherItemInstance Current { get; private set; }
        public WeatherFactoryMock(){} // For inspector serialization
        public WeatherFactoryMock(WeatherItemData fixedWeather)
        {
            this.fixedWeather = fixedWeather;
        }

        public WeatherItemInstance Create()
        {
            Current = new WeatherItemInstance(fixedWeather, new ModifierSourceMock(), null);
            return Current;
        }
    }
}