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
    public record FishingRoomManagerDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private FishingRoomManager _manager;
        [ShowInInspector] private WeatherWeightTableInstance _weatherWeightTable;
        
        public FishingRoomManagerDebugData(FishingRoomManager manager, WeatherWeightTableInstance weatherWeightTable)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _manager = manager;
            _weatherWeightTable = weatherWeightTable;
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
            DebugEditorWindow.Inspect(_fishingRoomManagerDebugData, "Fishing Room Manager Debug");
        }
        
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
                var table = container.Resolve<WeatherWeightTableInstance>();
                _fishingRoomManagerDebugData = new FishingRoomManagerDebugData(manager, table);
#endif
            });

        }
    }
}