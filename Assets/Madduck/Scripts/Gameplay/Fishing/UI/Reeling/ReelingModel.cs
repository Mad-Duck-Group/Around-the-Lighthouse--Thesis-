using System;
using Madduck.Fishing.Config;
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
    public class ReelingModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<UFloat> CurrentReelingProgress { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<UFloat> MaxReelingProgress { get; private set; }
        [field: SerializeField] public ReadOnlyReactiveProperty<Percentage> ReelingPercent { get; private set; }
        [field: SerializeField] public ReadOnlyReactiveProperty<Percentage> HookPositionXPercent { get; private set; }
        [field: SerializeField] public PlayerInventory Inventory { get; private set; }
        [field: SerializeField] public FishingRodItemInstance FishingRodInstance { get; private set; }
        [field: SerializeField] public FishItemInstance FishInstance { get; private set; }

        private readonly ReelingConfig _config;
        private IDisposable _bindings;
        
        [Inject]
        public ReelingModel(
            ReelingConfig config,
            PlayerInventory inventory)
        {
            Inventory = inventory;
            _config = config;
            FishingRodInstance = inventory.CurrentFishingRod;
            Bind();
        }
        
        private void Bind()
        {
            _bindings?.Dispose();
            var disposableBuilder = Disposable.CreateBuilder();
            CurrentReelingProgress = new SerializableReactiveProperty<UFloat>(0f)
                .AddTo(ref disposableBuilder);
            MaxReelingProgress = new SerializableReactiveProperty<UFloat>(_config.MaxReelingValue)
                .AddTo(ref disposableBuilder);
            ReelingPercent = CurrentReelingProgress
                .CombineLatest(MaxReelingProgress, (current, max) => max == 0f
                    ? Percentage.Full
                    : Percentage.FromFraction(Mathf.Clamp01(current / max)))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            HookPositionXPercent = CurrentReelingProgress
                .CombineLatest(MaxReelingProgress, (current, max) => Percentage.FromPercentage(max - current))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void SetFishInstance(FishItemInstance fishItemInstance)
        {
            FishInstance = fishItemInstance;
        }
        
        public void SetMaxProgress(UFloat maxProgress)
        {
            MaxReelingProgress.Value = maxProgress;
        }

        public void Reset()
        {
            CurrentReelingProgress.Value = 0f;
            MaxReelingProgress.Value = _config.MaxReelingValue;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}