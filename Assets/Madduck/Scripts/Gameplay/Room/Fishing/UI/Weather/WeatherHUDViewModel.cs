using System;
using System.Collections.Generic;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class WeatherHUDModel : IDisposable
    {
        
        private readonly SerializableDictionary<WeatherType, Sprite> _weatherIcons;
        private readonly ISubscriber<FishingRoomManager.WeatherChangedEvent> _weatherChangedSubscriber;
        private readonly WeatherHUDView _view;
        private IDisposable _bindings;
        
        [Inject]
        public WeatherHUDModel( SerializableDictionary<WeatherType, Sprite> weatherIcons , WeatherHUDView view,
            ISubscriber<FishingRoomManager.WeatherChangedEvent> weatherChangedSubscriber)
        {
            _weatherIcons = weatherIcons;
            _view = view;
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
            if (_weatherIcons.TryGetValue(newWeather, out var icon))
            {
                _view.SetWeatherIcon(icon);
            }
        }
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}
