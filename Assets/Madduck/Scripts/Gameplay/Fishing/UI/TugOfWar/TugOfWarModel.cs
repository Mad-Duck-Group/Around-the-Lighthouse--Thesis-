using System;
using Madduck.GameData;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.UI
{
    [Serializable]
    public class TugOfWarModel : IDisposable
    {
        [field: ShowInInspector] public ReactiveProperty<UFloat> CurrentTugOfWarProgress { get; private set; }
        [field: ShowInInspector] public ReactiveProperty<UFloat> MaxTugOfWarProgress { get; private set; }
        [field: ShowInInspector] public ReadOnlyReactiveProperty<Percentage> TugOfWarPercent { get; private set; }
        [field: ShowInInspector] public FishingRodItemInstance FishingRodInstance { get; private set; }
        [field: ShowInInspector] public FishItemInstance FishInstance { get; private set; }
        
        private IDisposable _bindings;

        public TugOfWarModel(PlayerInventory inventory)
        {
            FishingRodInstance = inventory.CurrentFishingRod;
            Bind();
        }
        
        public void SetFishInstance(FishItemInstance fishItemInstance)
        {
            FishInstance = fishItemInstance;
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            CurrentTugOfWarProgress = new ReactiveProperty<UFloat>(0f)
                .AddTo(ref disposableBuilder);
            MaxTugOfWarProgress = new ReactiveProperty<UFloat>(100f)
                .AddTo(ref disposableBuilder);
            TugOfWarPercent = CurrentTugOfWarProgress
                .CombineLatest(MaxTugOfWarProgress, (current, max) => max == 0f
                    ? Percentage.Zero
                    : Percentage.FromFraction(Mathf.Clamp01(current / max)))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        public void Reset()
        {
            CurrentTugOfWarProgress.Value = 0f;
            MaxTugOfWarProgress.Value = 100f;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}