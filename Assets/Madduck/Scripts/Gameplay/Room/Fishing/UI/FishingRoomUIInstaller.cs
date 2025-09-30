using System;
using System.Collections.Generic;
using Madduck.Day;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("fishCaughtView")]
        [Required,
         SerializeField] private FishCountView fishCountView;
        [Required,
         SerializeField] private SerializableDictionary<WeatherType, Sprite> weatherIcons;
        [Required,
         SerializeField] private SerializableDictionary<DayRoomKey, Sprite> sprites;
        [Required,
         SerializeField] private RoomTrackFactory roomTrackFactory;
        [Required,
         SerializeField] private RoomTrackView roomTrackView;
        
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => cardViewFactory, Lifetime.Singleton)
                .As<IGenericFactory<CardView>>();
            builder.Register<CardRackView>(Lifetime.Singleton);
            
            builder.Register(_ => weatherIcons, Lifetime.Singleton)
                .As<SerializableDictionary<WeatherType, Sprite>>();
            builder.RegisterComponent(weatherHUDView)
                .As<WeatherHUDView>();
            builder.Register<WeatherHUDViewModel>(Lifetime.Singleton);
            builder.RegisterComponent(fishCountView)
                .As<FishCountView>();
            builder.Register<FishCountViewModel>(Lifetime.Singleton);
            builder.Register(_ => sprites, Lifetime.Scoped)
                .As<SerializableDictionary<DayRoomKey, Sprite>>();
            builder.Register(_ => roomTrackFactory, Lifetime.Singleton)
                .As<IGenericFactory<RoomTrackView>>();
            builder.RegisterComponent(roomTrackView)
                .As<RoomTrackView>();
            builder.Register<RoomTrackViewModel>(Lifetime.Singleton);
            builder.Register<RoomTrackColumnView>(Lifetime.Singleton);
            
            
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<CardRackView>();
                x.Resolve<RoomTrackViewModel>();
                x.Resolve<RoomTrackColumnView>();
                x.Resolve<WeatherHUDViewModel>();
                x.Resolve<FishCountViewModel>();
            });
        }
    }
}