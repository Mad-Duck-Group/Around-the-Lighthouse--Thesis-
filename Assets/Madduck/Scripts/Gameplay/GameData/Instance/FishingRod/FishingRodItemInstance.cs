using System;
using System.Collections.Generic;
using Madduck.GameData.Card;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public record FishingRodItemInstance(FishingRodItemData ItemData) : ItemInstance<FishingRodItemData>(ItemData)
    {
        [Title("Fishing Rod Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishingRodStatsTitle;
        [field: InlineProperty,
                SerializeReference] public FishingRodStats CurrentStats { get; private set; } = new(ItemData);
        
        private List<RodModifier> _modifiers = new();
        
        private void ApplyModifiers()
        {
            CurrentStats = ItemData.GetBaseStats();
            foreach (var modifier in _modifiers.SortModifiers())
            {
                CurrentStats = modifier.Modify(CurrentStats);
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