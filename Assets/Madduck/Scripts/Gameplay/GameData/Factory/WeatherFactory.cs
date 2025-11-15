using System;
using JetBrains.Annotations;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public class WeatherFactory : IFactory<WeatherItemInstance>
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
            [Key(DIConstants.ModifierContainerKey)] IModifierSource modifierSource,
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
    public class WeatherFactoryMock : IFactory<WeatherItemInstance>
    {
        [SerializeField] private WeatherItemData fixedWeather;
        [SerializeField] private bool spoofDirection;
        [ShowIf(nameof(spoofDirection)), 
         SerializeField] private WindDirection fixedWindDirection;
        [SerializeField] private bool spoofStrength;
        [ShowIf(nameof(spoofStrength)),
         SerializeField] private WindStrength fixedWindStrength;
        
        [Inject] private readonly IModifierSource _modifierSource;
        [Inject] private readonly IPublisher<ModifierSourceEvent> _modifierSourceEventPublisher;

        public WeatherItemInstance Current { get; private set; }
        public WeatherFactoryMock(){} // For inspector serialization
        public WeatherFactoryMock(
            WeatherItemData fixedWeather,
            IModifierSource modifierSource,
            IPublisher<ModifierSourceEvent> modifierSourceEventPublisher)
        {
            this.fixedWeather = fixedWeather;
            _modifierSourceEventPublisher = modifierSourceEventPublisher;
            _modifierSource = modifierSource;
        }

        public WeatherItemInstance Create()
        {
            WindDirection? direction = null;
            WindStrength? strength = null;
            if (spoofDirection)
                direction = fixedWindDirection;
            if (spoofStrength)
                strength = fixedWindStrength;
            DebugUtils.Log($"Modifier Source in WeatherFactoryMock: {_modifierSource != null}");
            
            Current = new WeatherItemInstance(
                fixedWeather, 
                direction, 
                strength,
                _modifierSource ?? new ModifierSourceMock(),
                _modifierSourceEventPublisher);
            return Current;
        }
    }
}