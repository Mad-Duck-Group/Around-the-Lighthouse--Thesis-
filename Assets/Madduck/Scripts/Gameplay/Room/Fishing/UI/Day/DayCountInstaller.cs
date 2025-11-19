using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class DayCountInstaller : IInstaller
    {
        [Title("Day Count")]
        [Required,
         SerializeField] private DayCountView dayCountView;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(dayCountView).AsSelf();
            builder.Register<DayCountViewModel>(Lifetime.Scoped).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<DayCountViewModel>();
            });
        }
    }
}