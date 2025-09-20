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
    public record FishItemInstance : ItemInstance<FishItemData>
    {
        [field: DisplayAsString, 
                ShowInInspector] public uint CurrentFatigueCount { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public FishQuality CurrentFishQuality { get; set; }
        public FishItemInstance(FishItemData itemData) : base(itemData)
        {
            CurrentFatigueCount = 0;
        }
    }
}