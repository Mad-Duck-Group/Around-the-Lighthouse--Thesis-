using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class BoatSpriteInstaller : IInstaller
    {
        [Title("Boat Sprite")]
        [Required, 
         SerializeField] private BoatSpriteView boatSpriteView;
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register<BoatSpriteViewModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(boatSpriteView).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<BoatSpriteViewModel>();
            });
        }
    }
}