using System;
using Madduck.Fishing.Config;
using Madduck.Fishing.Controller;
using Madduck.Fishing.StateMachine;
using Madduck.Fishing.UI;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Fishing.DI
{
    [Serializable]
    public record TugOfWarStateDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private FishingState _state;
        [ShowInInspector] private TugOfWarModel _model;
        
        public TugOfWarStateDebugData(FishingState state, TugOfWarModel model)
        {
            AutoCloseWhenPlayModeEnds = true;
            ConstantUpdate = false;
            _state = state;
            _model = model;
        }
    }
    
    [Serializable]
    public class TugOfWarLifetimeScope : IInstaller
    {
        [Title("References")] 
        [Required, 
         SerializeField] private TugOfWarConfig tugOfWarConfig;
        [Required, 
         SerializeField] private TugOfWarView tugOfWarView;
        [Required,
        SerializeField] private TugOfWarUIIconConfig tugOfWarUIIconConfig;
        
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_tugOfWarStateDebugData, "Tug Of War Debug");
        }
        
        private TugOfWarStateDebugData _tugOfWarStateDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(tugOfWarConfig).AsSelf();
            builder.RegisterInstance(tugOfWarUIIconConfig).AsSelf();
            builder.Register(x =>
                {
                    x.Inject(tugOfWarView);
                    return tugOfWarView;
                }, Lifetime.Scoped)
                .Keyed(FishingStateType.TugOfWar)
                .AsImplementedInterfaces();
            builder.Register<TugOfWarController>(Lifetime.Scoped).AsSelf();
            builder.Register<TugOfWarViewModel>(Lifetime.Scoped).AsSelf();
            builder.Register<TugOfWarModel>(Lifetime.Scoped).AsSelf();
            builder.Register<TugOfWarState>(Lifetime.Scoped).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var tugOfWarState = x.Resolve<TugOfWarState>();
                stateMachine.AddState(FishingStateType.TugOfWar, tugOfWarState);
#if UNITY_EDITOR
                var model = x.Resolve<TugOfWarModel>();
                _tugOfWarStateDebugData = new TugOfWarStateDebugData(tugOfWarState, model);
#endif
            });
        }
    }
}