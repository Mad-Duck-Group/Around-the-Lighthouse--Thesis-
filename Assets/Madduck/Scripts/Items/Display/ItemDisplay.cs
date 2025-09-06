using MadDuck.Scripts.Items.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Display
{
    public abstract class ItemDisplay : MonoBehaviour
    {
        [Title("Base References")] 
        [SerializeField] protected SpriteRenderer spriteRenderer;

        [Title("Base Debug")] 
        [SerializeField, ReadOnly, InlineEditor] protected ItemData currentData;
        public ItemData CurrentData => currentData;
        
        public virtual void Initialize(ItemData itemData)
        {
            currentData = itemData;
        }
    }
}
