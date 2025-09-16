using System;
using Madduck.Fishing.Controller;
using Madduck.Fishing.Shared;
using Madduck.Fishing.StateMachine;
using Madduck.Fishing.UI;
using Madduck.Utils;
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
        [ShowInInspector] private FishingBoardState _fishingBoardState;
        [ShowInInspector] private FishingBoardModel _fishingBoardModel;
        [ShowInInspector] private FishingBoardController _fishingBoardController;
        
        public FishingBoardDebugData(
            FishingBoardState fishingBoardState, 
            FishingBoardModel fishingBoardModel, 
            FishingBoardController fishingBoardController)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _fishingBoardState = fishingBoardState;
            _fishingBoardModel = fishingBoardModel;
            _fishingBoardController = fishingBoardController;
        }
    }
    
    public class FishingBoardLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;
        [SerializeField] private FishingBoardView fishingBoardView;
        [InlineEditor]
        [SerializeField] private FishingBoardConfig fishingBoardConfig;

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
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(behaviorGraphAgent).AsSelf();
            builder.RegisterComponent(fishingBoardView).AsSelf();
            builder.RegisterInstance(fishingBoardConfig).AsSelf();
            builder.Register<FishingBoardController>(Lifetime.Scoped).AsSelf();
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
                var fishingBoardController = x.Resolve<FishingBoardController>();
                _fishingBoardDebugData = new FishingBoardDebugData(
                    fishingBoardState, 
                    fishingBoardModel, 
                    fishingBoardController);
#endif
            });

        }
    }
}