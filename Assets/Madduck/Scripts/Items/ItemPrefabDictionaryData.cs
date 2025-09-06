using System;
using System.Collections.Generic;
using System.Linq;
using MadDuck.Scripts.Items.Data;
using MadDuck.Scripts.Items.Display;
using MadDuck.Scripts.Utils.Inspectors;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items
{
    [CreateAssetMenu(fileName = "New Item Prefab Dictionary", menuName = "Item/Item Prefab Dictionary")]
    public class ItemPrefabDictionaryData : ScriptableObject
    {
        public SerializableDictionary<ItemType, ItemDisplay> itemPrefabDictionary;

        public ItemDisplay GetPrefab(ItemData data)
        {
            var type = data.ItemType;
            if (itemPrefabDictionary.TryGetValue(type, out var prefab))
            {
                return prefab;
            }
            Debug.LogError($"Prefab for type {type} not found in dictionary {name}");
            return null;
        }
    }
}
