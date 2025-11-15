using System;
using Madduck.GameData;
using Madduck.GameData.Fisherman;
using Madduck.Shared;
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
        //[ShowInInspector] private FishWeightTableInstance _fishWeightTable;
        [ShowInInspector] private CompositeWeightTableInstance _fishableWeightTable;
        [ShowInInspector] private FishermanItemInstance _fishermanItemData;
        [ShowInInspector] private ModifierContainer _modifierContainer;
        
        public DayManagerDebugData(
            DayManager manager, 
            PlayerInventory playerInventory,
            CompositeWeightTableInstance fishableWeightTable,
            FishermanItemInstance fishermanItemData,
            ModifierContainer modifierContainer)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _playerInventory = playerInventory;
            _manager = manager;
            _fishableWeightTable = fishableWeightTable;
            _fishermanItemData = fishermanItemData;
            _modifierContainer = modifierContainer;
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
        // [Required,
        //  SerializeField] private FishWeightTable fishWeightTable;
        [Required,
         SerializeField] private CompositeWeightTable fishableWeightTable;
        [Required,
         SerializeField] private CardWeightTable cardWeightTable;
        [Required,
         SerializeField] private CardRarityWeightTable cardRarityWeightTable;
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
            builder.Register<ModifierContainer>(Lifetime.Singleton)
                .Keyed(DIConstants.ModifierContainerKey)
                .AsSelf()
                .As<IModifierSource>();
            builder.RegisterInstance(dayManagerConfig).AsSelf();
            builder.Register(x =>
            {
                var modifierSource = x.Resolve<IModifierSource>(DIConstants.ModifierContainerKey);
                var instance = new CompositeWeightTableInstance(fishableWeightTable, modifierSource);
                instance.SetKeys(ModifierKeys.FishableKey);
                return instance;
            }, Lifetime.Singleton)
            .Keyed(ModifierKeys.FishableKey).AsSelf();
            builder.RegisterInstance(cardWeightTable).As<IWeightTable<CardWeightRecord>>();
            builder.Register<CardWeightTableInstance>(Lifetime.Singleton).AsSelf();
            builder.RegisterInstance(cardRarityWeightTable).As<IWeightTable<CardRarityWeightRecord>>();
            builder.Register<CardRarityWeightTableInstance>(Lifetime.Singleton).AsSelf();
            builder.RegisterInstance(fishermanItemData).AsSelf();
            builder.RegisterInstance(playerInventoryConfig).AsSelf();
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
            builder.Register<PlayerInventory>(Lifetime.Singleton)
                .AsSelf()
                .As<IPostInitializable>();
            builder.Register<FishermanItemInstance>(Lifetime.Singleton).AsSelf();
            builder.Register<DayManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.RegisterBuildCallback(x =>
            {
#if UNITY_EDITOR
                var fishermanItemInstance = x.Resolve<FishermanItemInstance>();
                var manager = x.Resolve<DayManager>();
                //var table = x.Resolve<FishWeightTableInstance>();
                var table = x.Resolve<CompositeWeightTableInstance>(ModifierKeys.FishableKey);
                var playerInventory = x.Resolve<PlayerInventory>();
                var modifierContainer = x.Resolve<ModifierContainer>(DIConstants.ModifierContainerKey);
                _dayManagerDebugData = new DayManagerDebugData(manager, playerInventory, table, fishermanItemInstance, modifierContainer);
#endif
            });
        }
    }
}