using System.Collections.Generic;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    public enum FishingRodStatType
    {
        Power,
        Resistance,
        FishingLineDurability,
        FishingLineRegenFactor,
        ReelingSpeed
    }

    [CreateAssetMenu(fileName = "New Fishing Rod Item Data", menuName = "Madduck/Fishing Rod/Fishing Rod Item Data")]
    public class FishingRodItemData : ItemData
    {
        [Title("Throw Hook Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _throwHookSettingsTitle;

        [field: InlineProperty,
                SerializeField] public Percentage MaxThrowPercentage { get; private set; } = Percentage.Half;
        [field: InlineProperty,
                SerializeField] public UFloat ThrowSliderSpeed { get; private set; } = 20f;
        
        [Title("Nibble Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _nibbleSettingsTitle;
        [field: SerializeField] public Percentage BubbleSpawnChance { get; private set; } = Percentage.FromPercentage(30f);
        [field: SerializeField] private SerializableDictionary<BubbleType, Percentage> bubbleNibbleBonuses = new();
        public IReadOnlyDictionary<BubbleType, Percentage> BubbleNibbleBonuses =>
                (Dictionary<BubbleType, Percentage>)bubbleNibbleBonuses;
        [field: SerializeField] private SerializableDictionary<BubbleType, Percentage> bubbleNibblePenalties = new();
        public IReadOnlyDictionary<BubbleType, Percentage> BubbleNibblePenalties =>
                (Dictionary<BubbleType, Percentage>)bubbleNibblePenalties;
        [field: SerializeField] private SerializableDictionary<int, Percentage> nibbleBaseSuccessChances = new();
        public IReadOnlyDictionary<int, Percentage> NibbleBaseSuccessChances =>
                (Dictionary<int, Percentage>)nibbleBaseSuccessChances;
        [field: InlineProperty, 
                SerializeField] public UFloat FishBiteTimeFrame { get; private set; } = 3f;

        [Title("Fishing Board Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishingBoardSettingsTitle;
        
        [field: InlineProperty,
                SerializeField] public UFloat Power { get; private set; } = 1f;

        [field: InlineProperty,
                SerializeField] public UFloat Resistance { get; private set; } = 1f;
        [field: InlineProperty,
                SerializeField] public Percentage FishingBoardDecayThreshold { get; private set; } = Percentage.FromPercentage(1f);
        
        [field: InlineProperty,
                SerializeField] public UFloat HookToCenterForce { get; private set; } = 200f;

        [field: InlineProperty,
                SerializeField] public UFloat FishingLineDurability { get; private set; } = 2f;

        [field: InlineProperty,
                SerializeField] public UFloat FishingLineRegenFactor { get; private set; } = 10f;

        [Title("Reeling Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _reelingSettingsTitle;

        [field: InlineProperty,
                SerializeField] public UFloat ReelingSpeed { get; private set; } = 2f;
        
        [Title("Tug of War Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _tugOfWarSettingsTitle;
        [field: InlineProperty,
                SerializeField] public Percentage TugOfWarDecayThreshold { get; private set; } = Percentage.FromPercentage(1f);
        [field: InlineProperty,
                SerializeField] public UFloat TugOfWarGainRate { get; private set; } = 5f;
    }
}