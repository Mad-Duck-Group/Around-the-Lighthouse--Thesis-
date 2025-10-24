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
        [field: SerializeField] public SerializableDictionary<BubbleType, Percentage> BubbleNibbleBonuses { get; private set; } = new();
        [field: SerializeField] public SerializableDictionary<BubbleType, Percentage> BubbleNibblePenalties { get; private set; } = new();
        [field: SerializeField] public SerializableDictionary<int, Percentage> NibbleBaseSuccessChances { get; private set; } = new();

        [Title("Fishing Board Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishingBoardSettingsTitle;
        
        [field: InlineProperty,
                SerializeField] public UFloat Power { get; private set; } = 1f;

        [field: InlineProperty,
                SerializeField] public UFloat Resistance { get; private set; } = 1f;

        [field: InlineProperty,
                SerializeField] public UFloat FishingLineDurability { get; private set; } = 2f;

        [field: InlineProperty,
                SerializeField] public UFloat FishingLineRegenFactor { get; private set; } = 10f;

        [Title("Reeling Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _reelingSettingsTitle;

        [field: InlineProperty,
                SerializeField] public UFloat ReelingSpeed { get; private set; } = 2f;
    }
}