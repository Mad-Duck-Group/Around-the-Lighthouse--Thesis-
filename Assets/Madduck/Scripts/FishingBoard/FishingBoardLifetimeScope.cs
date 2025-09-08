using System;
using System.Collections.Generic;
using Madduck.Scripts.FishingBoard.UI;
using Madduck.Scripts.FishingBoard.UI.Model;
using Madduck.Scripts.FishingBoard.UI.View;
using Madduck.Scripts.FishingBoard.UI.ViewModel;
using MadDuck.Scripts.Items.Data;
using MadDuck.Scripts.Items.Instance;
using Madduck.Scripts.Utils.Editor;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Scripts.FishingBoard
{
    [Serializable]
    public record FishingBoardDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [ShowInInspector] private FishingBoardState _fishingBoardState;
        [ShowInInspector] private FishingBoardModel _fishingBoardModel;
        [ShowInInspector] private FishingBoardController _fishingBoardController;
        
        public FishingBoardDebugData(
            FishingBoardState fishingBoardState, 
            FishingBoardModel fishingBoardModel, 
            FishingBoardController fishingBoardController)
        {
            _fishingBoardState = fishingBoardState;
            _fishingBoardModel = fishingBoardModel;
            _fishingBoardController = fishingBoardController;
            ConstantUpdate = false;
        }
    }
    
    public class FishingBoardLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;
        [SerializeField] private FishingBoardView fishingBoardView;
        [InlineEditor]
        [SerializeField] private FishingBoardConfig fishingBoardConfig;
        [InlineEditor]
        [SerializeField] private FishItemData fishItemData;
        [InlineEditor]
        [SerializeField] private FishingRodItemData fishingRodItemData;

#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            _debugWindow = DebugEditorWindow.Inspect(_fishingBoardDebugData);
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
        private FishingBoardDebugData _fishingBoardDebugData;
#endif
        
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(behaviorGraphAgent).AsSelf();
            builder.RegisterComponent(fishingBoardView).AsSelf();
            builder.RegisterInstance(fishingBoardConfig).AsSelf();
            builder.RegisterInstance(new FishItemInstance(fishItemData)).AsSelf();
            builder.RegisterInstance(new FishingRodItemInstance(fishingRodItemData)).AsSelf();
            builder.Register<FishingBoardController>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingBoardModel>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingBoardViewModel>(Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<FishingBoardState>().AsSelf();
#if UNITY_EDITOR
            builder.RegisterBuildCallback(x =>
            {
                var fishingBoardState = x.Resolve<FishingBoardState>();
                var fishingBoardModel= x.Resolve<FishingBoardModel>();
                var fishingBoardController = x.Resolve<FishingBoardController>();
                _fishingBoardDebugData = new FishingBoardDebugData(
                    fishingBoardState, 
                    fishingBoardModel, 
                    fishingBoardController);
            });
#endif
        }
    }
}