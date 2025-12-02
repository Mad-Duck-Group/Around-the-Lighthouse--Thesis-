using System;
using Madduck.GameData;
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
         SerializeField] private Image weatherIcon;
        [Required, 
         SerializeField] private Image windDirectionIcon;
        [Required, 
         SerializeField] private Sprite noWindSprite;
        [Required, 
         SerializeField] private Sprite hasWindSprite;

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

        private void SetWeatherIcon(WeatherItemInstance weather)
        {
            if (_weatherIcons.TryGetValue(weather.ItemData.WeatherType, out var icon))
            {
                weatherIcon.sprite = icon;
            }

            var hasWind = weather.CurrentWindDirection.CurrentValue is not WindDirection.Middle
                           && weather.CurrentWindStrength.CurrentValue is not WindStrength.None;
            windDirectionIcon.sprite = hasWind ? hasWindSprite : noWindSprite;
            if (!hasWind) return;
            var isLeft = weather.CurrentWindDirection.CurrentValue is WindDirection.Left;
            windDirectionIcon.transform.rotation = Quaternion.Euler(0f, isLeft ? 0f : 180f, 0f);
        }
    }
}
