using System;
using Madduck.Fishing.Shared;
using Madduck.GameData;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.UI
{
    [Serializable]
    public class NibbleModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<bool> IsNibbling { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Sign> PullHookResult { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Percentage> CatchChance { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<uint> CatchStage { get; private set; }
        [field: SerializeField] public FishingRodItemInstance FishingRod { get; private set; }
        [field: SerializeField] public FishItemInstance FishItemInstance { get; private set; }
        [field: SerializeField] public PlayerInventory PlayerInventory { get; private set; }
        private IDisposable _bindings;
        
        [Inject]
        public NibbleModel(PlayerInventory playerInventory)
        {
            PlayerInventory = playerInventory;
            FishingRod = playerInventory.CurrentFishingRod;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            IsNibbling = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            PullHookResult = new SerializableReactiveProperty<Sign>(Sign.Zero)
                .AddTo(ref disposableBuilder);
            CatchChance = new SerializableReactiveProperty<Percentage>(Percentage.Zero)
                .AddTo(ref disposableBuilder);
            CatchStage = new SerializableReactiveProperty<uint>(0)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void SetFishInstance(FishItemInstance fishItemInstance)
        {
            FishItemInstance = fishItemInstance;
        }
        
        public void Reset()
        {
            IsNibbling.Value = false;
            PullHookResult.Value = Sign.Zero;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}