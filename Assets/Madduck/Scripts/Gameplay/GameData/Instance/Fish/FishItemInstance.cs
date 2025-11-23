using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public enum FishQuality
    {
        Common,
        Good,
        Premium
    }
    
    [Serializable]
    public class FishItemInstance : ItemInstance<FishItemData>, IFishableItemInstance
    {
        [field: DisplayAsString, 
                ShowInInspector] public FishQuality CurrentFishQuality { get; set; }
        [field: ShowInInspector] private Dictionary<ModifierId, List<FishStatModifierData>> _modifiers = new();
        [field: ShowInInspector] public FishStats CurrentStats { get; private set; }
        
        private readonly IModifierSource _modifierSource;
        
        private DisposableBag _modifierChangedSubscription;
        
        [Inject]
        public FishItemInstance(
            FishItemData itemData, 
            [Key(DIConstants.ModifierContainerKey)] IModifierSource modifierSource) 
            : base(itemData)
        {
            _modifierSource = modifierSource ?? throw new ArgumentNullException(nameof(modifierSource));
            CurrentStats = new FishStats(itemData);
            CurrentFishQuality = EnumUtils.RandomValue<FishQuality>(); //TODO: Replace with Weight Table later
            OnSubscribeModifierSource();
        }

        #region Modifier 
        private void OnSubscribeModifierSource()
        {
            _modifierSource.Modifiers.OnModifierFirstSubscribe(_modifiers);
            ApplyModifiers();
            _modifierSource.ModifiersView.ObserveChanged()
                .Subscribe(x =>
                {
                    x.OnModifierChanged(_modifiers);
                    ApplyModifiers();
                })
                .AddTo(ref _modifierChangedSubscription);
        }
        
        private void ApplyModifiers()
        {
            CurrentStats = new FishStats(ItemData);
            var flattenModifiers = _modifiers.SelectMany(x => x.Value).ToList();
            flattenModifiers = FilterModifier(flattenModifiers);
            var modifierGroups = flattenModifiers.GroupBy(x => x.FishStatType);
            foreach (var group in modifierGroups)
            {
                switch (group.Key)
                {
                    case FishStatType.Power:
                        CurrentStats.CurrentPower = group.Calculate(CurrentStats.CurrentPower);
                        break;
                    case FishStatType.Resistance:
                        CurrentStats.CurrentResistance = group.Calculate(CurrentStats.CurrentResistance);
                        break;
                    case FishStatType.FishWeight:
                        CurrentStats.CurrentFishWeight = group.Calculate(CurrentStats.CurrentFishWeight);
                        break;
                    case FishStatType.FatigueDuration:
                        CurrentStats.CurrentFatigueDuration = group.Calculate(CurrentStats.CurrentFatigueDuration);
                        break;
                    case FishStatType.TugOfWarDecayRate:
                        CurrentStats.CurrentTugOfWarDecayRate = group.Calculate(CurrentStats.CurrentTugOfWarDecayRate);
                        break;
                    case FishStatType.TugOfWarRegression:
                        CurrentStats.CurrentTugOfWarRegression = group.Calculate(CurrentStats.CurrentTugOfWarRegression);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private List<FishStatModifierData> FilterModifier(List<FishStatModifierData> flatten)
        {
            var result = new List<FishStatModifierData>();
            foreach (var modifier in flatten)
            {
                switch (modifier.ModifierType)
                {
                    case FishModifierType.All:
                        break;
                    case FishModifierType.Name:
                        if (!modifier.FishItemData.Guid.Equals(ItemData.Guid)) continue;
                        break;
                    case FishModifierType.Size:
                        if (modifier.FishSize != ItemData.Size) continue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                result.Add(modifier);
            }
            return result;
        }

        public override void Dispose()
        {
            base.Dispose();
            _modifierChangedSubscription.Dispose();
            _modifierChangedSubscription.Clear();
        }
        #endregion

        #region Fish Quality
        public void SetFishQuality(FishQuality fishQuality)
        {
            CurrentFishQuality = fishQuality;
        }
        
        public void UpgradeFishQuality()
        {
            if (CurrentFishQuality == EnumUtils.Max<FishQuality>())
            {
                DebugUtils.LogWarning("Already at max quality");
                return;
            }
            CurrentFishQuality++;
        }
        
        public void DowngradeFishQuality()
        {
            if (CurrentFishQuality == EnumUtils.Min<FishQuality>())
            {
                DebugUtils.LogWarning("Already at min quality");
                return;
            }
            CurrentFishQuality--;
        }
        #endregion
    }

    [Serializable]
    public record FishStats : IStatModifiable<FishStats>
    {
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentPower { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentResistance { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentFishWeight { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentFatigueDuration { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentTugOfWarDecayRate { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentTugOfWarRegression { get; set; }
        
        public FishStats(FishItemData itemData)
        {
            CurrentPower = itemData.Power;
            CurrentResistance = itemData.Resistance;
            CurrentFishWeight = itemData.FishWeight;
            CurrentFatigueDuration = itemData.FatigueDuration;
            CurrentTugOfWarDecayRate = itemData.TugOfWarDecayRate;
            CurrentTugOfWarRegression = itemData.TugOfWarRegression;
        }
        
        public FishStats Copy() => this with { };
    }
    
    [Serializable]
    public class FishStatModifierData : BaseModifierData
    {
        [field: SerializeField] public FishModifierType ModifierType { get; private set; }
        [field: ShowIf(nameof(ModifierType), FishModifierType.Size),
                SerializeField] public FishSize FishSize { get; private set; }
        [field: ShowIf(nameof(ModifierType), FishModifierType.Name),
                SerializeField] public FishItemData FishItemData { get; private set; }
        [field: SerializeField] public FishStatType FishStatType { get; private set; }
        public class Builder : ModifierDataBuilder<FishStatModifierData>
        {
            private Builder(ModifierMethod modifierMethod) 
                : base(modifierMethod) { }
            
            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }
            
            public Builder WithSize(FishSize size)
            {
                modifierData.ModifierType = FishModifierType.Size;
                modifierData.FishSize = size;
                return this;
            }
            
            public Builder WithName(FishItemData fishItemData)
            {
                modifierData.ModifierType = FishModifierType.Name;
                modifierData.FishItemData = fishItemData;
                return this;
            }
            
            public Builder WithFishStatType(FishStatType fishStatType)
            {
                modifierData.FishStatType = fishStatType;
                return this;
            }
        }
    }
}