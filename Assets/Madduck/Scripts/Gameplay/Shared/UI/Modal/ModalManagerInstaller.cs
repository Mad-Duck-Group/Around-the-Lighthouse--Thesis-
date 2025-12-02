using System;
using Madduck.Shared;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Utils
{
    [Serializable]
    public record ModalManagerDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; } = true;
        [ShowInInspector] private IModalManager _modalManager;
        
        public ModalManagerDebugData(IModalManager modalManager)
        {
            _modalManager = modalManager;
        }
    }
    
    [Serializable]
    public class ModalManagerInstaller : IInstaller
    {
        [Title("Modal Manager"),
            HideLabel,
            ShowInInspector] private InspectorPlaceholder _modalManagerPlaceholder;
#if UNITY_EDITOR
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_modalManagerDebugData, "Modal Manager Debug");
        }
        
        private ModalManagerDebugData _modalManagerDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register<ModalManager>(Lifetime.Singleton)
                .As<IModalManager>();
            builder.RegisterBuildCallback(x =>
            {
#if UNITY_EDITOR
                var modalManager = x.Resolve<IModalManager>();
                _modalManagerDebugData = new(modalManager);
#endif
            });
        }
    }
}