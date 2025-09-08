using System;
using System.Collections.Generic;
using MadDuck.Scripts.Items.Instance;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.FishingBoard.UI.Model
{
    [Serializable]
    public class FishingBoardModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<bool> IsActive { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Vector2> FishPosition { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Vector2> HookPosition { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Quaternion> FishRotation { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Quaternion> HookRotation { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<float> CurrentFatigueLevel { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<float> MaxFatigueLevel { get; private set; }
        [field: SerializeField] public FishItemInstance FishItemInstance { get; private set; }
        [field: SerializeField] public FishingRodItemInstance FishingRodItemInstance { get; private set; }
        public ReadOnlyReactiveProperty<float> FishingLineDurabilityPercent { get; private set; }
        public ReadOnlyReactiveProperty<float> FatigueLevelPercent { get; private set; }
        public Dictionary<FishZone, CircleBoardState> CircleBoardState { get; set; }

        private IDisposable _bindings;
        
        [Inject]
        public FishingBoardModel(FishItemInstance fishItemInstance, FishingRodItemInstance fishingRodItemInstance)
        {
            FishItemInstance = fishItemInstance;
            FishingRodItemInstance = fishingRodItemInstance;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            IsActive = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            FishPosition = new SerializableReactiveProperty<Vector2>(Vector2.zero)
                .AddTo(ref disposableBuilder);
            HookPosition = new SerializableReactiveProperty<Vector2>(Vector2.zero)
                .AddTo(ref disposableBuilder);
            FishRotation = new SerializableReactiveProperty<Quaternion>(Quaternion.identity)
                .AddTo(ref disposableBuilder);
            HookRotation = new SerializableReactiveProperty<Quaternion>(Quaternion.identity)
                .AddTo(ref disposableBuilder);
            CurrentFatigueLevel = new SerializableReactiveProperty<float>(0f)
                .AddTo(ref disposableBuilder);
            MaxFatigueLevel = new SerializableReactiveProperty<float>(100f)
                .AddTo(ref disposableBuilder);
            var baseDurability =
                Observable.EveryValueChanged(FishingRodItemInstance, x => x.BaseStats.FishingLineDurability);
            var currentDurability =
                Observable.EveryValueChanged(FishingRodItemInstance, x => x.CurrentFishingLineDurability);
            FishingLineDurabilityPercent = baseDurability
                .CombineLatest(currentDurability, (@base, current) => @base <= 0 ? 0f : Mathf.Clamp01(current / @base))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            FatigueLevelPercent = CurrentFatigueLevel
                .CombineLatest(MaxFatigueLevel, (current, max) => max <= 0 ? 0f : Mathf.Clamp01(current / max))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        public void Reset()
        {
            IsActive.Value = false;
            FishPosition.Value = Vector2.zero;
            HookPosition.Value = Vector2.zero;
            FishRotation.Value = Quaternion.identity;
            HookRotation.Value = Quaternion.identity;
            CurrentFatigueLevel.Value = 0f;
            MaxFatigueLevel.Value = 100f;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}