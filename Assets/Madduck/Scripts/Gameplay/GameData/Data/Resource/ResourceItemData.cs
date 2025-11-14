using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    public enum ResourceType
    {
        Trash,
    }
    
    public enum ResourceModifierType
    {
        All,
        Type,
        Name,
    }
    
    [CreateAssetMenu(fileName = "New Resource Item Data", menuName = "Madduck/Resource/Resource Item Data")]
    public class ResourceItemData : ItemData, IFishableItemData
    {
        [Title("Resource Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _resourceSettingsTitle;
        [field: SerializeField] public ResourceType ResourceType { get; private set; } = ResourceType.Trash;
        [field: SerializeField] public string ResourceName { get; private set; }
        [field: TextArea(3, 20),
                SerializeField] public string ResourceDescription { get; private set; }
        [field: PreviewField,
                SerializeField] public Sprite ResourceIcon { get; private set; }
    }
}