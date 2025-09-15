using System;
using Madduck.GameData;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public struct FishingRoomManagerDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [ShowInInspector] private FishingRoomManager _manager;
        
        public FishingRoomManagerDebugData(FishingRoomManager manager)
        {
            ConstantUpdate = false;
            _manager = manager;
        }
    }
    
    public class FishingRoomManagerLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [Required,
         SerializeField] private WeatherWeightTable weatherWeightTable;
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            _debugWindow = DebugEditorWindow.Inspect(_fishingRoomManagerDebugData, "Fishing Room Manager Debug");
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
        private FishingRoomManagerDebugData _fishingRoomManagerDebugData;
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(weatherWeightTable.GetInstance()).AsSelf();
            builder.RegisterEntryPoint<FishingRoomManager>().AsSelf();
            builder.RegisterBuildCallback(container =>
            {
#if UNITY_EDITOR
                var manager = container.Resolve<FishingRoomManager>();
                _fishingRoomManagerDebugData = new FishingRoomManagerDebugData(manager);
#endif
            });

        }
    }
}