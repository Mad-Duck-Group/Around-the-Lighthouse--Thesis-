using System;
using Madduck.Fishing.Config;
using Madduck.Fishing.Controller;
using Madduck.Fishing.StateMachine;
using Madduck.Fishing.UI;
using Madduck.Input;
using Madduck.Shared;
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
    
    [Serializable]
    public class NibbleLifetimeScope : IInstaller
    {
        [Title("References")]
        [Required, 
         SerializeField] private NibbleView nibbleView;
        [Required, 
         SerializeField] private NibbleConfig nibbleConfig;
        [Required, 
         SerializeField] private QteSequenceFactory qteSequenceFactory;
        
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
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(x =>
                {
                    x.Inject(nibbleView);
                    return nibbleView;
                }, Lifetime.Scoped)
                .Keyed(FishingStateType.Nibble)
                .AsImplementedInterfaces();
            builder.RegisterInstance(nibbleConfig).AsSelf();
            builder.Register<NibbleController>(Lifetime.Scoped).AsSelf();
            builder.Register<NibbleCommander>(Lifetime.Scoped).AsSelf();
            builder.Register<NibbleViewModel>(Lifetime.Scoped).AsSelf();
            builder.Register<NibbleModel>(Lifetime.Scoped).AsSelf();
            builder.Register<NibbleState>(Lifetime.Scoped).AsSelf();
            builder.Register(x =>
                {
                    x.Inject(qteSequenceFactory);
                    return qteSequenceFactory;
                }, Lifetime.Scoped)
                .Keyed(FishingStateType.Nibble)
                .As<IFactory<IQuickTimeEvent>>();
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