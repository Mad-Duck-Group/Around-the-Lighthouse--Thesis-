using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

namespace Madduck.GameData
{
    public enum FishSize
    {
        Small,
        Medium,
        Large
    }

    public enum FishModifierType
    {
        Name,
        Size,
    }
    
    [CreateAssetMenu(fileName = "New Fish Item Data", menuName = "Madduck/Fish/Fish Item Data")]
    public class FishItemData : ItemData
    {
        [Title("References"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _referenceTitle;
        [field: Required, 
                SerializeField] public BehaviorGraph BehaviorGraph { get; private set; }
        
        [Title("Fish Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishSettingsTitle;
        [field: SerializeField] public FishSize Size { get; private set; } = FishSize.Small;
        [field: NoNoneFlag,
            SerializeField] public WeatherType WeatherType { get; private set; } = WeatherType.Clear;
        [field: NoNoneFlag,
                SerializeField] public DayPhaseType DayPhaseType { get; private set; } = DayPhaseType.Day;
        [field: SerializeField] public string FishName { get; private set; }
        [field: TextArea(3, 20),
            SerializeField] public string FishDescription { get; private set; }
        [field: PreviewField,
            SerializeField] public Sprite FishSprite { get; private set; }
        [field: SerializeField] public uint BasePrice { get; private set; } = 10;
        
        [Title("Nibble Settings"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _nibbleSettingsTitle;
        [field: SerializeField] public uint MaxNibbleAttempts { get; private set; } = 3;
        [field: SerializeField] public Vector2 NibbleIntervalRange { get; private set; } = new(5f, 15f);
        [field: SerializeField] public Vector2 NibbleTimeFrameRange { get; private set; } = new(1f, 3f);
        
        [Title("Fishing Board Settings"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishingBoardSettingsTitle;
        [field: InlineProperty,
                SerializeField] public UFloat Power { get; private set; } = 1f;
        [field: InlineProperty,
                SerializeField] public UFloat Resistance { get; private set; } = 1f;

        [Title("Reeling Settings"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _reelingSettingsTitle;
        [field: InlineProperty,
                SerializeField] public UFloat FishWeight { get; private set; }
        [field: InlineProperty,
                 SerializeField] public UFloat FatigueDuration { get; private set; } = 10f;
        [field: SerializeField] public int MaxFatigueAttempts { get; private set; } = -1;
    }
}
