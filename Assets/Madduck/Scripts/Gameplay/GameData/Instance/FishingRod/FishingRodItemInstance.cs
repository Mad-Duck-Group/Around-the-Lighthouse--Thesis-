using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class FishingRodItemInstance : ItemInstance<FishingRodItemData>
    {
        [Title("Fishing Rod Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishingRodStatsTitle;
        [field: InlineProperty,
                SerializeReference] public FishingRodStats CurrentStats { get; private set; }
        
        private List<RodModifierData> _modifiers;
        private readonly IRequestHandler<ModifierRequest, ModiferResponse> _modifierRequestHandler;

        public FishingRodItemInstance(
            FishingRodItemData itemData,
            IRequestHandler<ModifierRequest, ModiferResponse> modifierRequestHandler)
            : base(itemData)
        {
             CurrentStats = new FishingRodStats(itemData);
             _modifierRequestHandler = modifierRequestHandler;
             _modifiers = _modifierRequestHandler.Invoke(ModifierRequest.For<RodModifierData>()).As<RodModifierData>();
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
            var statGroups = _modifiers.GroupBy(x => x.FishingRodStatType);
            foreach (var group in statGroups)
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