using System;
using System.Collections.Generic;
using Madduck.Fishing.Shared;
using Madduck.GameData;
using Madduck.GameData.Fisherman;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.UI
{
    [Serializable]
    public class FishingBoardModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<Vector2> FishPosition { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Vector2> HookPosition { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Quaternion> FishRotation { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Quaternion> HookRotation { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<UFloat> CurrentFatigueLevel { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<UFloat> MaxFatigueLevel { get; private set; }
        [field: SerializeField] public FishItemInstance FishItemInstance { get; private set; }
        [field: SerializeField] public FishingRodItemInstance FishingRodItemInstance { get; private set; }
        [field: SerializeField] public PlayerInventory Inventory { get; private set; }
        public ReadOnlyReactiveProperty<Percentage> FishingLineDurabilityPercent { get; private set; }
        public ReadOnlyReactiveProperty<Percentage> FatigueLevelPercent { get; private set; }

        private IDisposable _bindings;
        
        [Inject]
        public FishingBoardModel(
            PlayerInventory inventory)
        {
            Inventory = inventory;
            FishingRodItemInstance = inventory.CurrentFishingRod;
            Bind();
        }
        
        public void SetFishInstance(FishItemInstance fishItemInstance)
        {
            FishItemInstance = fishItemInstance;
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            FishPosition = new SerializableReactiveProperty<Vector2>(Vector2.zero)
                .AddTo(ref disposableBuilder);
            HookPosition = new SerializableReactiveProperty<Vector2>(Vector2.zero)
                .AddTo(ref disposableBuilder);
            FishRotation = new SerializableReactiveProperty<Quaternion>(Quaternion.identity)
                .AddTo(ref disposableBuilder);
            HookRotation = new SerializableReactiveProperty<Quaternion>(Quaternion.identity)
                .AddTo(ref disposableBuilder);
            CurrentFatigueLevel = new SerializableReactiveProperty<UFloat>(0f)
                .AddTo(ref disposableBuilder);
            MaxFatigueLevel = new SerializableReactiveProperty<UFloat>(100f)
                .AddTo(ref disposableBuilder);
            var baseDurability =
                Observable.EveryValueChanged(FishingRodItemInstance, x => x.ItemData.FishingLineDurability);
            var currentDurability =
                Observable.EveryValueChanged(FishingRodItemInstance, x => x.CurrentStats.CurrentFishingLineDurability);
            FishingLineDurabilityPercent = baseDurability
                .CombineLatest(currentDurability, (@base, current) => @base <= 0 
                    ? Percentage.Zero
                    : Percentage.FromFraction(Mathf.Clamp01(current / @base)))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            FatigueLevelPercent = CurrentFatigueLevel
                .CombineLatest(MaxFatigueLevel, (current, max) => max <= 0 
                    ? Percentage.Zero
                    : Percentage.FromFraction(Mathf.Clamp01(current / max)))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        public void Reset()
        {
            FishPosition.Value = Vector2.zero;
            HookPosition.Value = Vector2.zero;
            FishRotation.Value = Quaternion.identity;
            HookRotation.Value = Quaternion.identity;
            CurrentFatigueLevel.Value = 0f;
            MaxFatigueLevel.Value = 100f;
            FishingRodItemInstance.CurrentStats.CurrentFishingLineDurability = FishingRodItemInstance.ItemData.FishingLineDurability;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}