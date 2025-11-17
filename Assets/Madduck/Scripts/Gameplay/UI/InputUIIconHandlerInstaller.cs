using System;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Room;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class InputUIIconHandlerInstaller : IInstaller
    {
        [Required,
         SerializeField] private InputUIIconView inputUIIconView;
        [Required,
         SerializeField] private InputIconData  inputIconData;
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => inputIconData, Lifetime.Singleton)
                .As<InputIconData>();
            builder.Register<InputIconViewModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(inputUIIconView).As<InputUIIconView>();
            builder.RegisterBuildCallback(x =>
            {
                var vm = x.Resolve<InputIconViewModel>();
            });
        }
    }
}
