using Madduck.Utils;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Item Prefab Dictionary", menuName = "Madduck/Item/Item Prefab Dictionary")]
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
