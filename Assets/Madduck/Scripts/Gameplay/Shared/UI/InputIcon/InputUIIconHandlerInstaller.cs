using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Shared
{
    [Serializable]
    public class InputUIIconHandlerInstaller : IInstaller
    {
        [Title("Input Icon")]
        [Required,
         SerializeField] private InputUIIconView inputUIIconView;
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register<InputIconViewModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(inputUIIconView).As<InputUIIconView>();
            builder.RegisterBuildCallback(x =>
            {
                var vm = x.Resolve<InputIconViewModel>();
            });
        }
    }
}
