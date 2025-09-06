using System;
using MadDuck.Scripts.Items.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Instance
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