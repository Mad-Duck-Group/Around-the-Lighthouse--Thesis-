using System;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class WeatherUIInstaller : IInstaller
    {
        [Title("Weather")]
        [Required,
         SerializeField] private WeatherHUDView weatherHUDView;
        [Required,
         SerializeField] private SerializableDictionary<WeatherType, Sprite> weatherIcons;

        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => weatherIcons, Lifetime.Singleton)
                .As<SerializableDictionary<WeatherType, Sprite>>();
            builder.RegisterComponent(weatherHUDView)
                .As<WeatherHUDView>();
            builder.Register<WeatherHUDViewModel>(Lifetime.Singleton);
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<WeatherHUDViewModel>();
            });
        }
    }
}