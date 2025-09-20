using Madduck.GameData.Card;
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
    public class FishingRodItemData : ItemData, IStatProvider<FishingRodStats>
    {
        [Title("Fishing Rod Settings"),
         HideLabel,
         ShowInInspector]
        private InspectorPlaceholder _fishingRodSettingsTitle;

        [field: InlineProperty,
                SerializeField] public UFloat Power { get; private set; } = 1f;

        [field: InlineProperty,
                SerializeField] public UFloat Resistance { get; private set; } = 1f;

        [field: InlineProperty,
                SerializeField] public UFloat FishingLineDurability { get; private set; } = 2f;

        [field: InlineProperty,
                SerializeField] public UFloat FishingLineRegenFactor { get; private set; } = 10f;

        [field: InlineProperty,
                SerializeField] public UFloat ReelingSpeed { get; private set; } = 2f;

        public FishingRodStats GetBaseStats()
        { 
                return new FishingRodStats(this);
        }
    }
}