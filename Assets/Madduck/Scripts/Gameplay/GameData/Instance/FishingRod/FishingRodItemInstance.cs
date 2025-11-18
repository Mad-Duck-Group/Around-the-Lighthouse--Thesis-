using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using DisposableBag = R3.DisposableBag;

namespace Madduck.GameData
{
    [Serializable]
    public class FishingRodItemInstance : ItemInstance<FishingRodItemData>
    {
        #region Inspector

        [Title("Fishing Rod Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishingRodStatsTitle;
        [field: InlineProperty,
                SerializeReference] public FishingRodStats CurrentStats { get; private set; }

        #endregion

        #region Fields

        private readonly IModifierSource _modifierSource;
        [ShowInInspector] private Dictionary<ModifierId, List<RodStatModifierData>> _modifiers = new();
        private DisposableBag _modifierChangedSubscription;
        private IDisposable _subscriptions;

        #endregion

        #region Injection

        [Inject]
        public FishingRodItemInstance(
            FishingRodItemData itemData,
            [Key(DIConstants.ModifierContainerKey)] IModifierSource modifierSource)
            : base(itemData)
        {
            CurrentStats = new FishingRodStats(itemData);
            _modifierSource = modifierSource;
            Subscribe();
        }

        #endregion

        #region Subscriptions

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            OnSubscribeModifierSource(_modifierSource);
            _subscriptions = disposableBuilder.Build();
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscriptions.Dispose();
            _modifierChangedSubscription.Dispose();
            _modifierChangedSubscription.Clear();
        }

        #endregion

        #region Events

        private void OnSubscribeModifierSource(IModifierSource source)
        {
            source.Modifiers.OnModifierFirstSubscribe(_modifiers);
            ApplyModifiers();
            source.ModifiersView.ObserveChanged()
                .Subscribe(x =>
                {
                    x.OnModifierChanged(_modifiers);
                    ApplyModifiers();
                })
                .AddTo(ref _modifierChangedSubscription);
        }

        /// <summary>
        /// Applies the modifiers to the current stats.
        /// </summary>
        /// <remarks>
        /// The modifiers are grouped by their type and then applied to the corresponding stats.
        /// </remarks>
        private void ApplyModifiers()
        {
            CurrentStats = new FishingRodStats(ItemData);
            var flattenModifiers = _modifiers.SelectMany(x => x.Value).ToList();
            var modifierGroups = flattenModifiers.GroupBy(x => x.FishingRodStatType);
            foreach (var group in modifierGroups)
            {
                switch (group.Key)
                {
                    case FishingRodStatType.Power:
                        CurrentStats.CurrentPower = group.Calculate(CurrentStats.CurrentPower);
                        break;
                    case FishingRodStatType.Resistance:
                        CurrentStats.CurrentResistance = group.Calculate(CurrentStats.CurrentResistance);
                        break;
                    case FishingRodStatType.FishingLineDurability:
                        CurrentStats.CurrentFishingLineDurability = group.Calculate(CurrentStats.CurrentFishingLineDurability);
                        break;
                    case FishingRodStatType.FishingLineRegenFactor:
                        CurrentStats.CurrentFishingLineRegenFactor = group.Calculate(CurrentStats.CurrentFishingLineRegenFactor);
                        break;
                    case FishingRodStatType.ReelingSpeed:
                        CurrentStats.CurrentReelingSpeed = group.Calculate(CurrentStats.CurrentReelingSpeed);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }

    [Serializable]
    public record FishingRodStats : IStatModifiable<FishingRodStats>
    {
        [Title("Debug Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugStatsTitle;

        #region Throw Hook
        private Percentage _currentMaxThrowPercentage;

        [InlineProperty, DisplayAsString,
         ShowInInspector]
        public Percentage CurrentMaxThrowPercentage
        {
            get => _currentMaxThrowPercentage; 
            set
            {
                var clamp = Percentage.Clamp01(value);
                _currentMaxThrowPercentage = clamp;
            }
        }
        
        [field: InlineProperty, DisplayAsString,
                SerializeField] public UFloat CurrentThrowSliderSpeed { get; set; }
        #endregion
        
        #region Nibble
        private Percentage _currentBubbleSpawnChance;
        [ShowInInspector] public Percentage CurrentBubbleSpawnChance 
        {
            get => _currentBubbleSpawnChance;
            set
            {
                var clamp = Percentage.Clamp01(value);
                _currentBubbleSpawnChance = clamp;
            }
        }
        [field: ShowInInspector] public Dictionary<BubbleType, Percentage> CurrentBubbleNibbleBonuses { get; set; } = new();
        [field: ShowInInspector] public Dictionary<BubbleType, Percentage> CurrentBubbleNibblePenalties { get; set; } = new();
        [field: ShowInInspector] public Dictionary<int, Percentage> CurrentNibbleBaseSuccessChances { get; set; } = new(); 
        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentFishBiteTimeFrame { get; set; }
        #endregion
        
        #region Fishing Board
        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentPower { get; set; }
        [field: DisplayAsString,
                ShowInInspector] public Percentage CurrentFishingBoardDecayThreshold { get; set; }

        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentResistance { get; set; }
        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentHookToCenterForce { get; set; }

        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentFishingLineDurability { get; set; }

        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentFishingLineRegenFactor { get; set; }
        #endregion
        
        #region Reeling
        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentReelingSpeed { get; set; }
        #endregion
        
        #region Tug of War
        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentTugOfWarGainRate { get; set; }
        [field: DisplayAsString,
                ShowInInspector] public Percentage CurrentTugOfWarDecayThreshold { get; set; }
        #endregion
        
        public FishingRodStats(FishingRodItemData itemData)
        {
            CurrentMaxThrowPercentage = itemData.MaxThrowPercentage;
            CurrentThrowSliderSpeed = itemData.ThrowSliderSpeed;
            CurrentBubbleSpawnChance = itemData.BubbleSpawnChance;
            CurrentBubbleNibbleBonuses = new(itemData.BubbleNibbleBonuses);
            CurrentBubbleNibblePenalties = new(itemData.BubbleNibblePenalties);
            CurrentNibbleBaseSuccessChances = new(itemData.NibbleBaseSuccessChances);
            CurrentFishBiteTimeFrame = itemData.FishBiteTimeFrame;
            CurrentPower = itemData.Power;
            CurrentFishingBoardDecayThreshold = itemData.FishingBoardDecayThreshold;
            CurrentResistance = itemData.Resistance;
            CurrentHookToCenterForce = itemData.HookToCenterForce;
            CurrentFishingLineDurability = itemData.FishingLineDurability;
            CurrentFishingLineRegenFactor = itemData.FishingLineRegenFactor;
            CurrentReelingSpeed = itemData.ReelingSpeed;
            CurrentTugOfWarGainRate = itemData.TugOfWarGainRate;
            CurrentTugOfWarDecayThreshold = itemData.TugOfWarDecayThreshold;
        }

        public FishingRodStats Copy() => this with { };
    }
    
    [Serializable]
    public class RodStatModifierData : BaseModifierData
    {
        [field: SerializeField] public FishingRodStatType FishingRodStatType { get; private set; }
        public class Builder : ModifierDataBuilder<RodStatModifierData>
        {
            private Builder(ModifierMethod modifierMethod) 
                : base(modifierMethod) { }
            
            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }
            
            public Builder WithFishingRodStatType(FishingRodStatType fishingRodStatType)
            {
                modifierData.FishingRodStatType = fishingRodStatType;
                return this;
            }
        }
    }
}