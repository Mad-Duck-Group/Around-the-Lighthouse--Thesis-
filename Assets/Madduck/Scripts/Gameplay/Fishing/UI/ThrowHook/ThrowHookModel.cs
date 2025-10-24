using System;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.GameData;
using Madduck.Utils;
using R3;
using UnityEngine;

namespace Madduck.Fishing.UI
{
    [Serializable]
    public class ThrowHookModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<UFloat> ThrowHookCurrentMaxValue { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<UFloat> ThrowHookCurrentValue { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<bool> HookThrown { get; private set; }
        public ReadOnlyReactiveProperty<Percentage> ThrowHookPercent { get; private set; }
        public FishingRodItemInstance FishingRod { get; private set; }
        
        private IDisposable _bindings;
        
        public ThrowHookModel(
            PlayerInventory playerInventory)
        {
            FishingRod = playerInventory.CurrentFishingRod;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            ThrowHookCurrentMaxValue = new SerializableReactiveProperty<UFloat>(FishingRod.CurrentStats.CurrentMaxThrowPercentage.AsPercentage)
                .AddTo(ref disposableBuilder);
            ThrowHookCurrentValue = new SerializableReactiveProperty<UFloat>(0f)
                .AddTo(ref disposableBuilder);
            HookThrown = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            ThrowHookPercent = ThrowHookCurrentValue
                .CombineLatest(new ReactiveProperty<UFloat>(Percentage.Full.AsPercentage), (current, max) => max <= 0 
                    ? Percentage.FromFraction(0f) 
                    : Percentage.FromFraction(Mathf.Clamp01(current / max)))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Reset()
        {
            ThrowHookCurrentValue.Value = 0f;
            ThrowHookCurrentMaxValue.Value = FishingRod.CurrentStats.CurrentMaxThrowPercentage.AsPercentage;
            HookThrown.Value = false;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}