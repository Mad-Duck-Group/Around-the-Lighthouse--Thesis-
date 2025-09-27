using System;
using Madduck.GameData;
using Madduck.GameData.Fisherman;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Day
{
    [Serializable]
    public record DayManagerDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private DayManager _manager;
        [ShowInInspector] private PlayerInventory _playerInventory;
        [ShowInInspector] private FishWeightTableInstance _fishWeightTable;
        [ShowInInspector] private FishermanItemInstance _fishermanItemData;
        
        public DayManagerDebugData(
            DayManager manager, 
            PlayerInventory playerInventory,
            FishWeightTableInstance fishWeightTable,
            FishermanItemInstance fishermanItemData)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _playerInventory = playerInventory;
            _manager = manager;
            _fishWeightTable = fishWeightTable;
            _fishermanItemData = fishermanItemData;
        }
    }
    
    [Serializable]
    public class DayManagerInstaller : IInstaller
    {
        [Title("Day Management")]
        [Required,
         SerializeField] private DayManagerConfig dayManagerConfig;
        [Required,
         SerializeField] private PlayerInventoryConfig playerInventoryConfig;
        [Required,
         SerializeField] private FishWeightTable fishWeightTable;
        [Required,
         SerializeField] private FishermanItemData fishermanItemData;
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_dayManagerDebugData, "Day Manager Debug");
        }
        
        private DayManagerDebugData _dayManagerDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(dayManagerConfig).AsSelf();
            builder.RegisterInstance(fishWeightTable).AsSelf();
            builder.Register<FishWeightTableInstance>(Lifetime.Singleton).AsSelf();
            builder.RegisterInstance(fishermanItemData).AsSelf();
            builder.RegisterInstance(playerInventoryConfig).AsSelf();
            builder.Register<PlayerInventory>(Lifetime.Singleton).AsSelf();
            builder.Register<FishermanItemInstance>(Lifetime.Singleton).AsSelf();
            builder.Register<DayManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.RegisterBuildCallback(x =>
            {
#if UNITY_EDITOR
                var fishermanItemInstance = x.Resolve<FishermanItemInstance>();
                var manager = x.Resolve<DayManager>();
                var table = x.Resolve<FishWeightTableInstance>();
                var playerInventory = x.Resolve<PlayerInventory>();
                _dayManagerDebugData = new DayManagerDebugData(manager, playerInventory, table, fishermanItemInstance);
#endif
            });
        }
    }
}