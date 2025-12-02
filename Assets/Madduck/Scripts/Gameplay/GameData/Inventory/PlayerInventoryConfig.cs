using System;
using System.Collections.Generic;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "PlayerInventoryConfig", menuName = "Madduck/Inventory/Player Inventory Config")]
    public class PlayerInventoryConfig : ScriptableObject
    {
        [Title("References"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _referencesTitle;
        [field: Required,
                SerializeField] public FishingRodItemData FishingRod { get; private set; }
        [field: InlineProperty,
                SerializeField] public List<CardItemData> StartingCards { get; private set; } = new();
        [field: InlineProperty,
                SerializeField] public SerializableDictionary<BaitType, ItemDataAndCount<BaitItemData>> StartingBaits { get; private set; } = new();
    }

    [Serializable]
    public record ItemDataAndCount<T> where T : ItemData
    {
        [field: SerializeField] public T ItemData { get; private set; } 
        [field: SerializeField] public uint Count { get; private set; }
        
        public ItemDataAndCount(){} // For Serialization

        public ItemDataAndCount(T ItemData, uint Count)
        {
             this.ItemData = ItemData;
             this.Count = Count;
        }
    }
}