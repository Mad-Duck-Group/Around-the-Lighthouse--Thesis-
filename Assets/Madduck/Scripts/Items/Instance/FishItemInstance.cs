using System;
using Madduck.Scripts.Items.Data;
using MadDuck.Scripts.Items.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Instance
{
    [Serializable]
    public class FishItemInstance : ItemInstance
    {
        [field: SerializeField, DisplayAsString] public int CurrentFatigueCount { get; set; }
        public FishItemData FishItemData => ItemData as FishItemData;
        public FishBehaviorData FishBehaviorData => FishItemData ? FishItemData.FishBehaviorData : null;
        public FishItemInstance(ItemData itemData) : base(itemData)
        {
            CurrentFatigueCount = 0;
        }
    }
}