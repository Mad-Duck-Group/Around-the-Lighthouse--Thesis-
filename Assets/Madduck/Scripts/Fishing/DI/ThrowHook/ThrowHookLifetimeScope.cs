using System;
using Madduck.Scripts.Fishing.Config.ThrowHook;
using Madduck.Scripts.Fishing.Controller.ThrowHook;
using Madduck.Scripts.Fishing.DI.StateMachine;
using Madduck.Scripts.Fishing.StateMachine;
using Madduck.Scripts.Fishing.StateMachine.ThrowHook;
using Madduck.Scripts.Fishing.UI.ThrowHook;
using Madduck.Scripts.Utils.Editor;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Scripts.Fishing.DI.ThrowHook
{
    [Serializable]
    public struct ThrowHookStateDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [ShowInInspector] private FishingState _state;
        [ShowInInspector] private ThrowHookModel _model;
        
        public ThrowHookStateDebugData(FishingState state, ThrowHookModel model)
        {
            ConstantUpdate = false;
            _state = state;
            _model = model;
        }
    }
    
    public class ThrowHookLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [Required]
        [SerializeField] private ThrowHookConfig throwHookConfig;
        [Required]
        [SerializeField] private ThrowHookView throwHookView;
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            _debugWindow = DebugEditorWindow.Inspect(_throwHookStateDebugData, "Throw Hook Debug");
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_debugWindow)
            {
                _debugWindow.Close();
            }
        }
        
        private DebugEditorWindow _debugWindow;
        private ThrowHookStateDebugData _throwHookStateDebugData;
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(throwHookConfig).AsSelf();
            builder.RegisterComponent(throwHookView).AsSelf();
            builder.Register<ThrowHookController>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowHookCommander>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowHookViewModel>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowHookModel>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowHookState>(Lifetime.Scoped).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var throwHookState = x.Resolve<ThrowHookState>();
                stateMachine.AddState(FishingStateType.ThrowHook, throwHookState);
#if UNITY_EDITOR
                var model = x.Resolve<ThrowHookModel>();
                _throwHookStateDebugData = new ThrowHookStateDebugData(throwHookState, model);
#endif
            });
        }
    }
}