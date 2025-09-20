using System.Collections.Generic;
using Madduck.Shared;
using Madduck.WeatherPreset;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class WeatherPresetManager : IStartable
    {
        #region Inspector

        [Title("Debug"),
         BoxGroup("Debug"),
         HideLabel, ReadOnly,
         ShowInInspector] private  List<WeatherPreset> _presets;
        [Title("Debug")]
        [DisplayAsString, 
         ShowInInspector] private WeatherType _currentWeather;
        #endregion
        
        #region Inject
        [Inject]
        public WeatherPresetManager(List<WeatherPreset> presets , WeatherType currentWeather)
        {
            _currentWeather = currentWeather;
            _presets = presets;
        }
        
        #endregion
        
        #region Lifecycle
        public void Start()
        {
            SpawnRandomWeather(Vector3.zero, Quaternion.identity);
        }

        private void SpawnRandomWeather(Vector3 zero, Quaternion identity)
        {
            int index = Random.Range(0, _presets.Count);
            WeatherPreset instance = Object.Instantiate(_presets[index], zero, identity);
            instance.SetUpWeatherParticles();
        }

        #endregion
    }

