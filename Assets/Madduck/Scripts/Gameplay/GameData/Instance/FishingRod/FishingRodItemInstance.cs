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
    public class FishingRodItemInstance : ItemInstance<FishingRodItemData>, IDisposable
    {
        #region Inspector

        [Title("Fishing Rod Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishingRodStatsTitle;
        [field: InlineProperty,
                SerializeReference] public FishingRodStats CurrentStats { get; private set; }

        #endregion

        #region Fields

        private readonly ISubscriber<ModifierSourceEvent> _modifierPublisherEventSubscriber;
        private Dictionary<ModifierId, List<RodStatModifierData>> _modifiers = new();
        private DisposableBag _modifierChangedSubscription;
        private IDisposable _subscriptions;

        #endregion

        #region Injection

        [Inject]
        public FishingRodItemInstance(
            FishingRodItemData itemData,
            ISubscriber<ModifierSourceEvent> modifierPublisherEventSubscriber)
            : base(itemData)
        {
            CurrentStats = new FishingRodStats(itemData);
            _modifierPublisherEventSubscriber = modifierPublisherEventSubscriber;
            Subscribe();
        }

        #endregion

        #region Subscriptions

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _modifierPublisherEventSubscriber.Subscribe(OnModifierPublished)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            _modifierChangedSubscription.Dispose();
            _modifierChangedSubscription.Clear();
        }

        #endregion

        #region Events

        private void OnModifierPublished(ModifierSourceEvent eventData)
        {
            eventData.ModiferSource.ModifiersView.ObserveChanged()
                .Subscribe(x =>
                {
                    _modifiers.OnModifierChanged(x);
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
                        CurrentStats.CurrentPower = group.CalculateStat(CurrentStats.CurrentPower);
                        break;
                    case FishingRodStatType.Resistance:
                        CurrentStats.CurrentResistance = group.CalculateStat(CurrentStats.CurrentResistance);
                        break;
                    case FishingRodStatType.FishingLineDurability:
                        CurrentStats.CurrentFishingLineDurability = group.CalculateStat(CurrentStats.CurrentFishingLineDurability);
                        break;
                    case FishingRodStatType.FishingLineRegenFactor:
                        CurrentStats.CurrentFishingLineRegenFactor = group.CalculateStat(CurrentStats.CurrentFishingLineRegenFactor);
                        break;
                    case FishingRodStatType.ReelingSpeed:
                        CurrentStats.CurrentReelingSpeed = group.CalculateStat(CurrentStats.CurrentReelingSpeed);
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
        [field: ShowInInspector] public SerializableDictionary<BubbleType, Percentage> CurrentBubbleNibbleBonuses { get; set; } = new();
        [field: ShowInInspector] public SerializableDictionary<BubbleType, Percentage> CurrentBubbleNibblePenalties { get; set; } = new();
        [field: ShowInInspector] public SerializableDictionary<int, Percentage> CurrentNibbleBaseSuccessChances { get; set; } = new();        
        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentPower { get; set; }

        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentResistance { get; set; }

        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentFishingLineDurability { get; set; }

        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentFishingLineRegenFactor { get; set; }

        [field: DisplayAsString,
                ShowInInspector] public UFloat CurrentReelingSpeed { get; set; }
         
        public FishingRodStats(FishingRodItemData itemData)
        {
            CurrentMaxThrowPercentage = itemData.MaxThrowPercentage;
            CurrentThrowSliderSpeed = itemData.ThrowSliderSpeed;
            CurrentBubbleNibbleBonuses = new(itemData.BubbleNibbleBonuses);
            CurrentBubbleNibblePenalties = new(itemData.BubbleNibblePenalties);
            CurrentNibbleBaseSuccessChances = new(itemData.NibbleBaseSuccessChances);
            CurrentPower = itemData.Power;
            CurrentResistance = itemData.Resistance;
            CurrentFishingLineDurability = itemData.FishingLineDurability;
            CurrentFishingLineRegenFactor = itemData.FishingLineRegenFactor;
            CurrentReelingSpeed = itemData.ReelingSpeed;
        }

        public FishingRodStats Copy() => this with { };
    }
}