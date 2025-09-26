using System;
using System.Collections.Generic;
using Madduck.Shared;
using Madduck.Shared.Events;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class WeatherHUDViewModel : IDisposable
    {
        public ReactiveProperty<WeatherType> CurrentWeather { get; } = new();
        
        private readonly ISubscriber<WeatherChangedEvent> _weatherChangedSubscriber;
        private IDisposable _bindings;
        
        [Inject]
        public WeatherHUDViewModel( 
            ISubscriber<WeatherChangedEvent> weatherChangedSubscriber)
        {
            _weatherChangedSubscriber = weatherChangedSubscriber;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _weatherChangedSubscriber.Subscribe(e => OnWeatherChanged(e.Weather))
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnWeatherChanged(WeatherType newWeather)
        {
            CurrentWeather.Value = newWeather;
        }
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}
