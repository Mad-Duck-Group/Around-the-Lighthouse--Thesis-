using Madduck.Scripts.Items.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Data
{
    [CreateAssetMenu(fileName = "New Fish Item Data", menuName = "Fish/Fish Item")]
    public class FishItemData : ItemData
    {
        [Title("References")]
        [field: SerializeField, InlineEditor, Required] public FishBehaviorData FishBehaviorData { get; private set; }
        
        [Title("Fish Settings")]
        [field: SerializeField] 
        public string FishName { get; private set; }
        [field: SerializeField]
        public Sprite FishSprite { get; private set; }
    }
}
