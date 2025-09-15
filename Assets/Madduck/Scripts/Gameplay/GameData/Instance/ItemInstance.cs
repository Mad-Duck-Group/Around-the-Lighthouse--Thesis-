using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public abstract class ItemInstance
    {
        [Title("References")]
        [field: SerializeField, InlineEditor, Required] public ItemData ItemData { get; private set; }
        
        public ItemInstance(ItemData itemData)
        {
            ItemData = itemData;
        }
    }
}