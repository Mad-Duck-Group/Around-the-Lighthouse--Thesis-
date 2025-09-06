using System;
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
        [field: SerializeField] public FishItemInstance FishItemInstance { get; private set; }
        [field: SerializeField] public FishingRodItemInstance FishingRodItemInstance { get; private set; }
        public ReadOnlyReactiveProperty<float> FishingLineDurabilityPercent { get; private set; }
        
        [Inject]
        public FishingBoardModel(FishItemInstance fishItemInstance, FishingRodItemInstance fishingRodItemInstance)
        {
            FishItemInstance = fishItemInstance;
            FishingRodItemInstance = fishingRodItemInstance;
            IsActive = new SerializableReactiveProperty<bool>(false);
            FishPosition = new SerializableReactiveProperty<Vector2>(Vector2.zero);
            HookPosition = new SerializableReactiveProperty<Vector2>(Vector2.zero);
            FishRotation = new SerializableReactiveProperty<Quaternion>(Quaternion.identity);
            HookRotation = new SerializableReactiveProperty<Quaternion>(Quaternion.identity);
            var baseDurability =
                Observable.EveryValueChanged(FishingRodItemInstance, x => x.BaseStats.FishingLineDurability);
            var currentDurability =
                Observable.EveryValueChanged(FishingRodItemInstance, x => x.CurrentFishingLineDurability);
            FishingLineDurabilityPercent = baseDurability
                .CombineLatest(currentDurability, (@base, current) => @base <= 0 ? 0f : Mathf.Clamp01(current / @base))
                .ToReadOnlyReactiveProperty();
        }
        
        public void Dispose()
        {
            IsActive.Dispose();
            FishPosition.Dispose();
            HookPosition.Dispose();
            FishRotation.Dispose();
            HookRotation.Dispose();
            FishingLineDurabilityPercent.Dispose();
        }
    }
}