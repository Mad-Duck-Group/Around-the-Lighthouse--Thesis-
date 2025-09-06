using Madduck.Scripts.FishingBoard.UI;
using Madduck.Scripts.FishingBoard.UI.Model;
using Madduck.Scripts.FishingBoard.UI.View;
using Madduck.Scripts.FishingBoard.UI.ViewModel;
using MadDuck.Scripts.Items.Data;
using MadDuck.Scripts.Items.Instance;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Scripts.FishingBoard
{
    public class FishingBoardLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;
        [SerializeField] private FishingBoardView fishingBoardView;
        [SerializeField] private FishingBoardConfig fishingBoardConfig;
        [SerializeReference] private FishingBoardMinigameReference fishingBoardMinigameReference = new();
        [SerializeField] private FishItemData fishItemData;
        [SerializeField] private FishingRodItemData fishingRodItemData;
        
        [Title("Debug")]
        [ShowInInspector, HideInEditorMode] private FishingBoardState _fishingBoardState;
        [ShowInInspector, HideInEditorMode] private FishingBoardViewModel _fishingBoardViewModel;
        [ShowInInspector, HideInEditorMode] private FishingBoardController _fishingBoardController;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(behaviorGraphAgent).AsSelf();
            builder.RegisterComponent(fishingBoardView).AsSelf();
            builder.RegisterInstance(fishingBoardConfig).AsSelf();
            builder.RegisterInstance(new FishItemInstance(fishItemData)).AsSelf();
            builder.RegisterInstance(new FishingRodItemInstance(fishingRodItemData)).AsSelf();
            builder.RegisterInstance(fishingBoardMinigameReference).AsSelf();
            builder.Register<FishingBoardController>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingBoardModel>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingBoardViewModel>(Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<FishingBoardState>().AsSelf();
#if UNITY_EDITOR
            builder.RegisterBuildCallback(x =>
            {
                _fishingBoardState = x.Resolve<FishingBoardState>();
                _fishingBoardViewModel = x.Resolve<FishingBoardViewModel>();
                _fishingBoardController = x.Resolve<FishingBoardController>();
            });
#endif
        }
    }
}