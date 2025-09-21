using System.Collections.Generic;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.WeatherPreset.Madduck.Scripts.Gameplay.WeatherPreset
{
    public class WeatherPresetManager : IStartable
    {
        #region Inspector

        [BoxGroup("Debug"),
         HideLabel, ReadOnly,
         ShowInInspector] private  List<global::Madduck.WeatherPreset.WeatherPreset> _presets;
        [BoxGroup("Debug"),
          HideLabel, ReadOnly,
          ShowInInspector] private  WeatherPresetConfig _presetsConfig;
        [BoxGroup("Debug")]
        [DisplayAsString, 
         ShowInInspector] private WeatherType _currentWeather;
        #endregion
        
        #region Inject
        [Inject]
        public WeatherPresetManager(IGenericFactory<WeatherType> weatherFactory,
            WeatherPresetConfig weatherPresetConfig)
        {
            _presetsConfig = weatherPresetConfig;
            _currentWeather = weatherFactory.Create();
            DebugUtils.Log("Current weather: " + _currentWeather);
        }
        
        #endregion
        
        #region Lifecycle
        public void Start()
        {
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
            int index = Random.Range(0, _presets.Count);
            global::Madduck.WeatherPreset.WeatherPreset instance = Object.Instantiate(_presets[index], zero, identity);
            instance.SetUpWeatherParticles();
        }

        #endregion
    }
}

