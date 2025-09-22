using System.Collections.Generic;
using Madduck.Shared;
using Madduck.Utils;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.WeatherPreset
{
    public class WeatherPresetManager : IStartable
    {
        #region Inspector

        [BoxGroup("Debug"),
         HideLabel, Sirenix.OdinInspector.ReadOnly,
         ShowInInspector] private  List<WeatherPreset> _presets;
        [BoxGroup("Debug"),
          HideLabel, Sirenix.OdinInspector.ReadOnly,
          ShowInInspector] private  WeatherPresetConfig _presetsConfig;
        [BoxGroup("Debug")]
        [DisplayAsString, 
         ShowInInspector] private WeatherType _currentWeather;
        #endregion
        
        private readonly IGenericFactory<WeatherType> _weatherFactory;
        
        #region Inject
        [Inject]
        public WeatherPresetManager(IGenericFactory<WeatherType> weatherFactory,
            WeatherPresetConfig weatherPresetConfig)
        {
            _presetsConfig = weatherPresetConfig;
            _weatherFactory = weatherFactory;
            DebugUtils.Log("Current weather: " + _currentWeather);
        }
        
        #endregion
        
        #region Lifecycle
        public void Start()
        {
            _currentWeather = _weatherFactory.Current;
            SpawnWeather(Vector3.zero, Quaternion.identity);
        }

        private void SpawnWeather(Vector3 zero, Quaternion identity)
        {
            switch (_currentWeather)
            {
                case WeatherType.Clear:
                    _presets = _presetsConfig.clearWeatherPreset;
                    break;
                case WeatherType.Rain:
                    _presets = _presetsConfig.rainyWeatherPreset;
                    break;
                case WeatherType.Fog:
                    _presets = _presetsConfig.fogWeatherPreset;
                    break;
                default:
                    Debug.LogError("Unsupported weather type: " + _currentWeather);
                    break;
            }
            WeatherPreset instance = Object.Instantiate(_presets.GetRandomElement(), zero, identity);
            instance.SetUpWeatherParticles();
        }

        #endregion
    }
}

