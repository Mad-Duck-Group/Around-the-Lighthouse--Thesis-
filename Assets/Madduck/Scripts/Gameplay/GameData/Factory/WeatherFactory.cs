using System;
using JetBrains.Annotations;
using Madduck.Shared;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public class WeatherFactory : IGenericFactory<WeatherType>
    {
        private readonly WeatherWeightTableInstance _weatherWeightTable;
        private readonly IRequestHandler<ModifierRequest, ModifierResponse> _modifierRequestHandler;

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
            WeatherWeightTableInstance weatherWeightTable,
            IRequestHandler<ModifierRequest, ModifierResponse> modifierRequestHandler)
        {
            _weatherWeightTable = weatherWeightTable;
            _modifierRequestHandler = modifierRequestHandler;
            var modifiers = _modifierRequestHandler.Invoke(ModifierRequest.For<WeatherModifierData>()).As<WeatherModifierData>();
            _weatherWeightTable.PersistentModifiers.Remove("CardModifiers");
            _weatherWeightTable.PersistentModifiers.TryAdd("CardModifiers", new WeatherWeightModifier(modifiers));
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