using System;
using Madduck.Fishing.Controller;
using Madduck.Fishing.Shared;
using Madduck.Fishing.StateMachine;
using Madduck.Fishing.UI;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Fishing.DI
{
    [Serializable]
    public record ReelingStateDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private FishingState _state;
        [ShowInInspector] private ReelingModel _model;
        
        public ReelingStateDebugData(FishingState state, ReelingModel model)
        {
            AutoCloseWhenPlayModeEnds = true;
            ConstantUpdate = false;
            _state = state;
            _model = model;
        }
    }
    public class ReelingLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [Required]
        [SerializeField] private ReelingView reelingView;
        [InlineEditor]
        [Required]
        [SerializeField] private ReelingConfig reelingConfig;
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_reelingStateDebugData, "Reeling Debug");
        }
        
        private ReelingStateDebugData _reelingStateDebugData;
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(reelingView).AsImplementedInterfaces();
            builder.RegisterInstance(reelingConfig).AsSelf();
            builder.Register<ReelingController>(Lifetime.Scoped).AsSelf();
            builder.Register<ReelingCommander>(Lifetime.Scoped).AsSelf();
            builder.Register<ReelingViewModel>(Lifetime.Scoped).AsSelf();
            builder.Register<ReelingModel>(Lifetime.Scoped).AsSelf();
            builder.Register<ReelingState>(Lifetime.Scoped).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var reelingState = x.Resolve<ReelingState>();
                stateMachine.AddState(FishingStateType.Reeling, reelingState);
#if UNITY_EDITOR
                var model = x.Resolve<ReelingModel>();
                _reelingStateDebugData = new ReelingStateDebugData(reelingState, model);
#endif
            });
        }
    }
}