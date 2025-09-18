using System;
using System.Collections.Generic;
using Madduck.GameData;
using Madduck.RoomPreset;
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
        [ShowInInspector] private FishingRoomManager _fishingRoomManager;
        [ShowInInspector] private WeatherWeightTableInstance _weatherWeightTable;
        [ShowInInspector] private RoomPresetManager _roomPresetManager;
        
        public FishingRoomManagerDebugData(FishingRoomManager fishingRoomManager,
            WeatherWeightTableInstance weatherWeightTable,
            RoomPresetManager roomPresetManager 
            )
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _fishingRoomManager = fishingRoomManager;
            _roomPresetManager = roomPresetManager;
            _weatherWeightTable = weatherWeightTable;
        }
    }
    
    public class FishingRoomManagerLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [Required,
         SerializeField] private WeatherWeightTable weatherWeightTable;
        [Required,
         SerializeField] private List<RoomPreset.RoomPreset> roomPresets;
        
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
            builder.RegisterInstance(roomPresets).As<List<RoomPreset.RoomPreset>>();
            builder.RegisterEntryPoint<FishingRoomManager>().AsSelf();
            builder.RegisterEntryPoint<RoomPresetManager>().AsSelf();
            builder.RegisterBuildCallback(container =>
            {
#if UNITY_EDITOR
                var fishingRoomManager = container.Resolve<FishingRoomManager>();
                var table = container.Resolve<WeatherWeightTableInstance>();
                var roomPresetManager = container.Resolve<RoomPresetManager>();
                _fishingRoomManagerDebugData = new FishingRoomManagerDebugData(fishingRoomManager, table,roomPresetManager);
#endif
            });

        }
    }
}