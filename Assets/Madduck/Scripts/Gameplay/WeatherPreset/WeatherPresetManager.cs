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
using WindDirection = Madduck.GameData.WindDirection;

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
        [BoxGroup("Debug")]
        [DisplayAsString, 
         ShowInInspector] private WindDirection _currentWindDirection;
        
        [field: DisplayAsString, 
                ShowInInspector] public ReactiveProperty<WeatherType> CurrentWeather { get; private set; }
        #endregion
        
        private readonly IFactory<WeatherItemInstance> _weatherFactory;
        private readonly ISubscriber<WeatherChangedEvent> _weatherChangedEventSubscriber;
        private IDisposable _subscriptions;
        
        #region Inject
        [Inject]
        public WeatherPresetManager(
            IFactory<WeatherItemInstance> weatherFactory,
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
            _currentWindDirection = weatherChangedEvent.Weather.CurrentWindDirection.CurrentValue;
            CurrentWeather.Value = _currentWeather;
            
            SpawnWeather();
        }

        #endregion
        
        #region Lifecycle
        public void Start()
        {
            _currentWeather = _weatherFactory.Current.ItemData.WeatherType;
            SpawnWeather();
        }

        private void SpawnWeather()
        {
            if (!_presetsConfig.weatherPreset.TryGetValue(_currentWeather, out var preset))
            {
                Debug.LogError($"WeatherPreset not found for {_currentWeather}");
                return;
            }
            if (!_presetsConfig.weatherParticles.TryGetValue(_currentWeather, out var particleGroup))
            {
                Debug.LogWarning($"No particle config found for {_currentWeather}");
                return;
            }
            List<WeatherParticlesSettings> selectedList = null;
            
            var windDirection = _currentWindDirection;
            var particleSetting = particleGroup.ParticlesSettings
                .Find(p => p.WindDirections == windDirection);
            var instance = Object.Instantiate(preset, Vector3.zero, Quaternion.identity);
            if (particleGroup.isStormy)
            {
                
                if (particleGroup.StormParticleSystem != null)
                {
                    var pos = (Vector2)instance.transform.position + particleGroup.StormPositionOffset;
                    var rotation = particleGroup.StormParticleSystem.transform.rotation;

                    var stormFX = Object.Instantiate(
                        particleGroup.StormParticleSystem,
                        pos,
                        rotation,
                        instance.transform
                    );

                    stormFX.Play();
                }
                else
                {
                    Debug.LogWarning($"Stormy weather {_currentWeather} has no StormParticleSystem assigned!");
                }
                
            }
            if (particleSetting is not null && particleSetting.ParticleSystem.Length > 0)
            {
                foreach (var ps in particleSetting.ParticleSystem)
                {
                    if (!ps) continue;
                    var pos = (Vector2)instance.transform.position + particleSetting.PositionOffset;
                    var particleInstance = Object.Instantiate(ps, pos,ps.transform.rotation, instance.transform);
                    particleInstance.Play();
                }
            }
            if (particleGroup.isStormy && particleGroup.windsStormyParticle != null)
            {
                foreach (var w in particleGroup.windsStormyParticle)
                {
                    if (w.WindDirections != windDirection) continue;

                    foreach (var ps in w.ParticleSystem)
                    {
                        if (!ps) continue;
                        var pos = (Vector2)instance.transform.position + w.PositionOffset;
                        var particleInstance = Object.Instantiate(ps, pos, ps.transform.rotation, instance.transform);
                        particleInstance.Play();
                    }
                }
            }

            
            if (particleGroup.isRainy && particleGroup.windsRainyParticle != null)
            {
                foreach (var w in particleGroup.windsRainyParticle)
                {
                    if (w.WindDirections != windDirection) continue;

                    foreach (var ps in w.ParticleSystem)
                    {
                        if (!ps) continue;
                        var pos = (Vector2)instance.transform.position + w.PositionOffset;
                        var particleInstance = Object.Instantiate(ps, pos, ps.transform.rotation, instance.transform);
                        particleInstance.Play();
                    }
                }
            }
            instance.SetUpWeatherParticles();
        }

        #endregion
    }
}

