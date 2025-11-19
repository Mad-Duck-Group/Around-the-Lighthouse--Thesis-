using System;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Spine.Unity;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;

namespace Madduck.GameData
{
    public enum FishSize
    {
        Small,
        Medium,
        Large
    }

    public enum FishEnemyType
    { 
            Normal,
            Boss,
    }

    public enum FishModifierType
    { 
        All,
        Name,
        Size,
    }
    
    public enum FishStatType
    {
        Power,
        Resistance,
        FishWeight,
        FatigueDuration,
        TugOfWarDecayRate,
        TugOfWarRegression,
    }
    
    [CreateAssetMenu(fileName = "New Fish Item Data", menuName = "Madduck/Fish/Fish Item Data")]
    public class FishItemData : ItemData, IFishableItemData
    {
        [Title("References"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _referenceTitle;
        [field: Required, 
                SerializeField] public BehaviorGraph BehaviorGraph { get; private set; }
        
        [Title("Fish Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishSettingsTitle;
        [field: SerializeField] public FishEnemyType EnemyType { get; private set; } = FishEnemyType.Normal;
        [field: SerializeField] public FishSize Size { get; private set; } = FishSize.Small;
        [field: NoNoneFlag,
            SerializeField] public WeatherType WeatherType { get; private set; } = WeatherType.Clear;
        [field: NoNoneFlag,
                SerializeField] public DayPhaseType DayPhaseType { get; private set; } = DayPhaseType.Day;
        [field: SerializeField] public string FishName { get; private set; }
        [field: TextArea(3, 20),
            SerializeField] public string FishDescription { get; private set; }
        [field: PreviewField,
                SerializeField] public Sprite FishIcon { get; private set; }
        [field: PreviewField,
                SerializeField] public Sprite FishSprite { get; private set; }
        [field: Required, 
                SerializeField] public FishSpriteAnimatorConfig FishSpriteAnimatorConfig { get; private set; }
        [field: Required, 
                SerializeField] public SkeletonDataAsset FishSkeletonDataAsset { get; private set; }
        [field: ShowIf(nameof(EnemyType), FishEnemyType.Normal), 
                SpineSkin(dataField: "<FishSkeletonDataAsset>k__BackingField"),
                SerializeField] public string FishSkin { get; private set; }
        [field: SerializeField] public Vector2 FatigueSliderOffset { get; private set; }
        [field: SerializeField] public Vector2 SpriteAnchorOffset { get; private set; }
        [field: SerializeField] public uint BasePrice { get; private set; } = 10;
        
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
        
        [Title("Tug of War Settings"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _tugOfWarSettingsTitle;

        [field: InlineProperty,
                SerializeField] public UFloat TugOfWarDecayRate { get; private set; } = 30f;
        [field: InlineProperty,
                SerializeField] public UFloat TugOfWarRegression { get; private set; } = 20f;

        public string Name => FishName;
        public string Description => FishDescription;
        public Sprite Icon => FishIcon;
    }
}
