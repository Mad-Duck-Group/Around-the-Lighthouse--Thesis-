using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Shared
{
    [Serializable]
    public class InputInstructionInstaller : IInstaller
    {
        [Title("Input Instruction")]
        [Required,
         SerializeField] private InputInstructionView instructionView;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(instructionView)
                .AsSelf();
            builder.Register<InputInstructionViewModel>(Lifetime.Scoped)
                .AsSelf();
            builder.Register<InputInstructionManager>(Lifetime.Singleton)
                .AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<InputInstructionViewModel>();
            });
        }
    }
}