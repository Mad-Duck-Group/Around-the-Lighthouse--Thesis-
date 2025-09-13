using System;
using Madduck.Scripts.Fishing.Controller.Nibble;
using Madduck.Scripts.Fishing.StateMachine;
using Madduck.Scripts.Fishing.StateMachine.Nibble;
using Madduck.Scripts.Fishing.UI.Nibble;
using Madduck.Scripts.Fishing.UI.ThrowHook;
using Madduck.Scripts.Utils.Editor;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Scripts.Fishing.DI.Nibble
{
    [Serializable]
    public struct NibbleStateDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [ShowInInspector] private FishingState _state;
        [ShowInInspector] private NibbleModel _model;
        
        public NibbleStateDebugData(FishingState state, NibbleModel model)
        {
            ConstantUpdate = false;
            _state = state;
            _model = model;
        }
    }
    
    public class NibbleLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [Required]
        [SerializeField] private NibbleView nibbleView;
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            _debugWindow = DebugEditorWindow.Inspect(_nibbleStateDebugData, "Nibble Debug");
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
        private NibbleStateDebugData _nibbleStateDebugData;
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(nibbleView).AsSelf();
            builder.Register<NibbleController>(Lifetime.Scoped).AsSelf();
            builder.Register<NibbleCommander>(Lifetime.Scoped).AsSelf();
            builder.Register<NibbleViewModel>(Lifetime.Scoped).AsSelf();
            builder.Register<NibbleModel>(Lifetime.Scoped).AsSelf();
            builder.Register<NibbleState>(Lifetime.Scoped).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var nibbleState = x.Resolve<NibbleState>();
                stateMachine.AddState(FishingStateType.Nibble, nibbleState);
#if UNITY_EDITOR
                var model = x.Resolve<NibbleModel>();
                _nibbleStateDebugData = new NibbleStateDebugData(nibbleState, model);
#endif
            });
        }
        
    }
}