using System;
using Madduck.Scripts.Fishing.Config.Reeling;
using Madduck.Scripts.Fishing.Controller.Reeling;
using Madduck.Scripts.Fishing.StateMachine;
using Madduck.Scripts.Fishing.StateMachine.Reeling;
using Madduck.Scripts.Fishing.UI.Reeling;
using Madduck.Scripts.Utils.Editor;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Scripts.Fishing.DI.Reeling
{
    [Serializable]
    public struct ReelingStateDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [ShowInInspector] private FishingState _state;
        [ShowInInspector] private ReelingModel _model;
        
        public ReelingStateDebugData(FishingState state, ReelingModel model)
        {
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
            _debugWindow = DebugEditorWindow.Inspect(_reelingStateDebugData, "Reeling Debug");
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
        private ReelingStateDebugData _reelingStateDebugData;
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(reelingView).AsSelf();
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