using System;
using Madduck.Shared;
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
        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => cardViewFactory, Lifetime.Singleton)
                .As<IGenericFactory<CardView>>();
            builder.Register<CardRackView>(Lifetime.Singleton);
            builder.RegisterBuildCallback(x => x.Resolve<CardRackView>());
        }
    }
}