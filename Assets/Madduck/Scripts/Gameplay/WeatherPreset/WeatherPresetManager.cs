using System;
using System.Collections.Generic;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using WindDirection = Madduck.Shared.WindDirection;

namespace Madduck.WeatherPreset
{
    
    public class WeatherPresetManager : IStartable, IDisposable
    {
        #region Inspector

        [BoxGroup("Debug"),
         HideLabel, Sirenix.OdinInspector.ReadOnly,
         ShowInInspector] private WeatherPreset _preset;
        [BoxGroup("Debug"),
          HideLabel, Sirenix.OdinInspector.ReadOnly,
          ShowInInspector] private WeatherPresetConfig _presetsConfig;
        [BoxGroup("Debug")]
        [DisplayAsString, 
         ShowInInspector] private WeatherType _currentWeather;
        
        [field: DisplayAsString, 
                ShowInInspector] public ReactiveProperty<WeatherType> CurrentWeather { get; private set; }
        #endregion
        
        private readonly IGenericFactory<WeatherItemInstance> _weatherFactory;
        private readonly ISubscriber<WeatherChangedEvent> _weatherChangedEventSubscriber;
        private IDisposable _subscriptions;
        
        #region Inject
        [Inject]
        public WeatherPresetManager(
            IGenericFactory<WeatherItemInstance> weatherFactory,
            ISubscriber<WeatherChangedEvent> weatherChangedEventSubscriber,
            WeatherPresetConfig weatherPresetConfig)
        {
            _presetsConfig = weatherPresetConfig;
            _weatherChangedEventSubscriber = weatherChangedEventSubscriber;
            _weatherFactory = weatherFactory;
            CurrentWeather = new ReactiveProperty<WeatherType>(_currentWeather);
            Subscribe();
        }
        #endregion
        
        #region Subscription

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _weatherChangedEventSubscriber
                .Subscribe(OnWeatherChanged)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        private void OnWeatherChanged(WeatherChangedEvent weatherChangedEvent)
        {
            _currentWeather = weatherChangedEvent.Weather.ItemData.WeatherType;
            CurrentWeather.Value = _currentWeather;
            SpawnWeather(Vector3.zero, Quaternion.identity);
        }

        #endregion
        
        #region Lifecycle
        public void Start()
        {
            _currentWeather = _weatherFactory.Current.ItemData.WeatherType;
            SpawnWeather(Vector3.zero, Quaternion.identity);
        }

        private void SpawnWeather(Vector3 zero, Quaternion identity)
        {
            switch (_currentWeather)
            {
                case WeatherType.Clear:
                    _preset = _presetsConfig.weatherPreset.TryGetValue(WeatherType.Clear, out var clearPreset)
                        ? clearPreset : null;
                    break;
                case WeatherType.Rain:
                    _preset = _presetsConfig.weatherPreset.TryGetValue(WeatherType.Rain, out var rainPreset)
                        ? rainPreset : null;
                    break;
                case WeatherType.Storm:
                    _preset = _presetsConfig.weatherPreset.TryGetValue(WeatherType.Storm, out var stormPreset)
                        ? stormPreset : null;
                    break;
                case WeatherType.StrongWinds:
                    _preset = _presetsConfig.weatherPreset.TryGetValue(WeatherType.StrongWinds, out var strongPreset)
                        ? strongPreset : null;
                    break;
                case WeatherType.Cloudy:
                    _preset = _presetsConfig.weatherPreset.TryGetValue(WeatherType.Cloudy ,out var cloudyPreset)
                        ? cloudyPreset : null;
                    break;
                default:
                    Debug.LogError("Unsupported weather type: " + _currentWeather);
                    break;
            }
            WeatherPreset instance = Object.Instantiate(_preset, zero, identity);
            instance.SetUpWeatherParticles();
        }

        #endregion
    }
}

