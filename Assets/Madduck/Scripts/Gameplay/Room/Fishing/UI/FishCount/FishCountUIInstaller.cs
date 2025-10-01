using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class FishCountUIInstaller : IInstaller
    {
        [Title("Fish Count")]
        [Required,
         SerializeField] private FishCountView fishCountView;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(fishCountView)
                .As<FishCountView>();
            builder.Register<FishCountViewModel>(Lifetime.Singleton);
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<FishCountViewModel>();
            });
        }
    }
}