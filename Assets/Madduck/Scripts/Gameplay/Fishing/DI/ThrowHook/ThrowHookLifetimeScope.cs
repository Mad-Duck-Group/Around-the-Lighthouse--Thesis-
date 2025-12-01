using System;
using Madduck.Fishing.Config;
using Madduck.Fishing.Controller;
using Madduck.Fishing.Shared;
using Madduck.Fishing.StateMachine;
using Madduck.Fishing.UI;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Fishing.DI
{
    [Serializable]
    public record ThrowHookStateDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private FishingState _state;
        [ShowInInspector] private ThrowHookModel _model;
        
        public ThrowHookStateDebugData(FishingState state, ThrowHookModel model)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _state = state;
            _model = model;
        }
    }
    
    [Serializable]
    public class ThrowHookLifetimeScope : IInstaller
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
            DebugEditorWindow.Inspect(_throwHookStateDebugData, "Throw Hook Debug");
        }
        
        private ThrowHookStateDebugData _throwHookStateDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(throwHookConfig).AsSelf();
            builder.Register(x =>
                {
                    x.Inject(throwHookView);
                    return throwHookView;
                }, Lifetime.Scoped)
                .Keyed(FishingStateType.ThrowHook)
                .AsImplementedInterfaces();
            builder.Register<ThrowHookController>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowHookCommander>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowHookViewModel>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowHookModel>(Lifetime.Scoped).AsSelf();
            builder.Register<PrepareBaitState>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowHookState>(Lifetime.Scoped).AsSelf();
            builder.Register<ThrowingHookState>(Lifetime.Scoped).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var prepareBaitState = x.Resolve<PrepareBaitState>();
                var throwHookState = x.Resolve<ThrowHookState>();
                var throwingHookState = x.Resolve<ThrowingHookState>();
                stateMachine.AddState(FishingStateType.PrepareBait, prepareBaitState);
                stateMachine.AddState(FishingStateType.ThrowHook, throwHookState);
                stateMachine.AddState(FishingStateType.ThrowingHook, throwingHookState);
#if UNITY_EDITOR
                var model = x.Resolve<ThrowHookModel>();
                _throwHookStateDebugData = new ThrowHookStateDebugData(throwHookState, model);
#endif
            });
        }
    }
}