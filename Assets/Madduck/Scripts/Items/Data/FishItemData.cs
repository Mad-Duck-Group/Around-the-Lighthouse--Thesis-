using Madduck.Scripts.Items.Data;
using MadDuck.Scripts.Utils.Inspectors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Data
{
    public enum FishSize
    {
        Small,
        Medium,
        Large
    }
    
    [CreateAssetMenu(fileName = "New Fish Item Data", menuName = "Madduck/Fish/Fish Item Data")]
    public class FishItemData : ItemData
    {
        [Title("References"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _referenceTitle;
        [field: InlineEditor, 
                Required,
                SerializeField] public FishBehaviorData FishBehaviorData { get; private set; }
        
        [Title("Fish Settings"),
         HideLabel,
         ShowInInspector] private InspectorVoid _fishSettingsTitle;
        [field: SerializeField] public FishSize Size { get; private set; } = FishSize.Small;
        [field: SerializeField] public string FishName { get; private set; }
        [field: SerializeField] public Sprite FishSprite { get; private set; }
        [field: SerializeField] public uint BasePrice { get; private set; } = 10;
    }
}
