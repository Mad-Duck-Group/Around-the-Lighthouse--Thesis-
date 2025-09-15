using System;
using Sirenix.OdinInspector;

namespace Madduck.GameData
{
    public enum FishQuality
    {
        Common,
        Good,
        Premium
    }
    
    [Serializable]
    public class FishItemInstance : ItemInstance
    {
        [field: DisplayAsString, 
                ShowInInspector] public int CurrentFatigueCount { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public FishQuality CurrentFishQuality { get; set; }
        public FishItemData FishItemData => ItemData as FishItemData;
        public FishBehaviorData FishBehaviorData => FishItemData ? FishItemData.FishBehaviorData : null;
        public FishItemInstance(ItemData itemData) : base(itemData)
        {
            CurrentFatigueCount = 0;
        }
    }
}