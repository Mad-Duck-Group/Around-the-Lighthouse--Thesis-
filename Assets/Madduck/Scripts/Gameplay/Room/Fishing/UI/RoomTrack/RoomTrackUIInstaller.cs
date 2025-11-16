using System;
using Madduck.Day;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class RoomTrackUIInstaller : IInstaller
    {
        [Title("Room Track")]
        
        [Required,
         SerializeField] private RoomTrackFactory roomTrackFactory;
        [Required,
         SerializeField] private BoatTrackViewFactory boatTrackViewFactory;
        [Required,
         SerializeField] private DayRoomSpriteConfig dayRoomSpriteConfig;

        public void Install(IContainerBuilder builder)
        {
            
            builder.Register(_ => roomTrackFactory, Lifetime.Singleton)
                .As<IFactory<RoomTrackView>>();
            
            builder.Register(_ => boatTrackViewFactory, Lifetime.Singleton)
                .As<IFactory<BoatTrackView>>();
            builder.RegisterInstance(dayRoomSpriteConfig).AsSelf();
            builder.Register<RoomTrackViewModel>(Lifetime.Singleton);
            builder.Register<RoomTrackColumnViewModel>(Lifetime.Singleton);
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<RoomTrackViewModel>();
                x.Resolve<RoomTrackColumnViewModel>();
            });
        }
    }
}