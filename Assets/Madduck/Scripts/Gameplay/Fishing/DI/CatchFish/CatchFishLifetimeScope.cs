using System;
using Madduck.Fishing.Config;
using Madduck.Fishing.Controller;
using Madduck.Fishing.StateMachine;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Fishing.DI
{
    
    [Serializable]
    public record CatchFishStateDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private FishingState _state;
        
        public CatchFishStateDebugData(FishingState state)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _state = state;
        }
    }
    
    [Serializable]
    public class CatchFishLifetimeScope : IInstaller
    {
        [Title("References")]
        [Required, 
         SerializeField] private CatchFishConfig catchFishConfig;
        [Required, 
         SerializeField] private QteButtonFactory qteButtonFactory;
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_catchFishStateDebugData, "Catch Fish Debug");
        }
        
        private CatchFishStateDebugData _catchFishStateDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(catchFishConfig).AsSelf();
            builder.Register<CatchFishController>(Lifetime.Scoped).AsSelf();
            builder.Register<CatchFishState>(Lifetime.Scoped).AsSelf();
            builder.Register(x =>
                {
                    x.Inject(qteButtonFactory);
                    return qteButtonFactory;
                }, Lifetime.Scoped)
                .Keyed(FishingStateType.CatchFish)
                .As<IFactory<IQuickTimeEvent>>();
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var nibbleState = x.Resolve<CatchFishState>();
                stateMachine.AddState(FishingStateType.CatchFish, nibbleState);
#if UNITY_EDITOR
                _catchFishStateDebugData = new CatchFishStateDebugData(nibbleState);
#endif
            });
        }
    }
}