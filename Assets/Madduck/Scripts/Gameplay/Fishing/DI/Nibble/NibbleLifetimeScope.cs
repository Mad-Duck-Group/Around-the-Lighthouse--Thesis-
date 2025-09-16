using System;
using Madduck.Fishing.Controller;
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
    public record NibbleStateDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private FishingState _state;
        [ShowInInspector] private NibbleModel _model;
        
        public NibbleStateDebugData(FishingState state, NibbleModel model)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
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
            DebugEditorWindow.Inspect(_nibbleStateDebugData, "Nibble Debug");
        }
        
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