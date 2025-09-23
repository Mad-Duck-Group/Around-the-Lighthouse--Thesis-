using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class FishingRodItemInstance : ItemInstance<FishingRodItemData>, IDisposable
    {
        [Title("Fishing Rod Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishingRodStatsTitle;
        [field: InlineProperty,
                SerializeReference] public FishingRodStats CurrentStats { get; private set; }

        private Dictionary<ModifierId, List<RodModifierData>> _modifiers = new();
        private readonly ISubscriber<ModifierUpdatedEvent> _modifierUpdatedEventSubscriber;
        private IDisposable _subscriptions;

        public FishingRodItemInstance(
            FishingRodItemData itemData,
            ISubscriber<ModifierUpdatedEvent> modifierUpdatedEventSubscriber)
            : base(itemData)
        {
             CurrentStats = new FishingRodStats(itemData);
             _modifierUpdatedEventSubscriber = modifierUpdatedEventSubscriber;
             Subscribe();
        }

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _modifierUpdatedEventSubscriber.Subscribe(OnModifierUpdated)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        private void OnModifierUpdated(ModifierUpdatedEvent eventData)
        {
            var modifiers = eventData.ModifierProvider.GetModifiers<RodModifierData>();
            _modifiers = modifiers.CombineModifiers(_modifiers);
            ApplyModifiers();
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
    }

    [Serializable]
    public record FishingRodStats : IStatModifiable<FishingRodStats>
    {
        [Title("Debug Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugStatsTitle;

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
            CurrentPower = itemData.Power;
            CurrentResistance = itemData.Resistance;
            CurrentFishingLineDurability = itemData.FishingLineDurability;
            CurrentFishingLineRegenFactor = itemData.FishingLineRegenFactor;
            CurrentReelingSpeed = itemData.ReelingSpeed;
        }

        public FishingRodStats Copy() => this with { };
    }
}