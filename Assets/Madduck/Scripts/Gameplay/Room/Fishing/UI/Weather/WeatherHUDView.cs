using System;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room
{
    public class WeatherHUDView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private Image icon;

        private WeatherHUDViewModel _viewModel;
        private SerializableDictionary<WeatherType, Sprite> _weatherIcons;
        private IDisposable _bindings;

        [Inject]
        public void SetUp(
            WeatherHUDViewModel viewModel,
            SerializableDictionary<WeatherType, Sprite> weatherIcons)
        {
            _weatherIcons = weatherIcons;
            _viewModel = viewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CurrentWeather.Subscribe(SetWeatherIcon)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings.Dispose();
        }

        private void SetWeatherIcon(WeatherType weatherType)
        {
            if (_weatherIcons.TryGetValue(weatherType, out var weatherIcon))
            {
                icon.sprite = weatherIcon;
            }
        }
    }
}
