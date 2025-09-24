using System;
using System.Collections.Generic;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class FishingRoomUIInstaller : IInstaller
    {
        [Title("References")]
        [Required,
            SerializeField] private CardViewFactory cardViewFactory;
        [Required,
         SerializeField] private WeatherHUDView weatherHUDView;
        [Required,
         SerializeField] private FishCaughtView fishCaughtView;
        [Required,
         SerializeField] private SerializableDictionary<WeatherType, Sprite> weatherIcons;
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => cardViewFactory, Lifetime.Singleton)
                .As<IGenericFactory<CardView>>();
            builder.Register<CardRackView>(Lifetime.Singleton);
            
            builder.Register(_ => weatherIcons, Lifetime.Singleton)
                .As<SerializableDictionary<WeatherType, Sprite>>();
            builder.Register(_ => weatherHUDView, Lifetime.Singleton)
                .As<WeatherHUDView>();
            builder.Register<WeatherHUDViewModel>(Lifetime.Singleton);
            builder.Register(_ => fishCaughtView, Lifetime.Singleton)
                .As<FishCaughtView>();
            builder.Register<FishCaughtViewModel>(Lifetime.Singleton);
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<CardRackView>();
                x.Resolve<WeatherHUDViewModel>();
                x.Resolve<FishCaughtViewModel>();
            });
        }
    }
}