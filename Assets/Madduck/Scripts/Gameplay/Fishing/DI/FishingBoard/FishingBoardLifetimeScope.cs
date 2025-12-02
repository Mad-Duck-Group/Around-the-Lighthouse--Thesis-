using System;
using Madduck.Fishing.Config;
using Madduck.Fishing.Controller;
using Madduck.Fishing.Shared;
using Madduck.Fishing.StateMachine;
using Madduck.Fishing.UI;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Fishing.DI
{
    [Serializable]
    public record FishingBoardDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private FishingBoardModel _model;
        [ShowInInspector] private FishingBoardVariables _variables;
        
        public FishingBoardDebugData(
            FishingBoardModel model, 
            FishingBoardVariables variables)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _model = model;
            _variables = variables;
        }
    }
    
    [Serializable]
    public class FishingBoardLifetimeScope : IInstaller
    {
        [Title("References")]
        [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;
        [SerializeField] private FishingBoardView fishingBoardView;
        [InlineEditor, 
         SerializeField] private FishingBoardConfig fishingBoardConfig;

#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_fishingBoardDebugData, "Fishing Board Debug");
        }
        
        private FishingBoardDebugData _fishingBoardDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(behaviorGraphAgent).AsSelf();
            builder.Register(x =>
                {
                    x.Inject(fishingBoardView);
                    return fishingBoardView;
                }, Lifetime.Scoped)
                .Keyed(FishingStateType.FishingBoard)
                .As<ITransitionable>();
            builder.Register(_ => fishingBoardView, Lifetime.Scoped)
                .As<ICircleBoard>();
            builder.RegisterInstance(fishingBoardConfig).AsSelf();
            builder.Register<FishingBoardVariables>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingBoardController>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingBoardAIController>(Lifetime.Scoped).As<IFishingBoardAIController>();
            builder.Register<FishingBoardModel>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingBoardViewModel>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingBoardState>(Lifetime.Scoped).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var fishingBoardState = x.Resolve<FishingBoardState>();
                var stateMachine = x.Resolve<FishingStateMachine>();
                stateMachine.AddState(FishingStateType.FishingBoard, fishingBoardState);
#if UNITY_EDITOR
                var fishingBoardModel= x.Resolve<FishingBoardModel>();
                var variables = x.Resolve<FishingBoardVariables>();
                _fishingBoardDebugData = new FishingBoardDebugData(
                    fishingBoardModel, 
                    variables);
#endif
            });

        }
    }
}