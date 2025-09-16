using System;
using Madduck.Utils;
using Sirenix.OdinInspector;

namespace Madduck.GameData.Fisherman
{
    [Serializable]
    public class FishermanItemInstance : ItemInstance<FishermanItemData>
    {
        [field: DisplayAsString, InlineProperty,
                ShowInInspector] public UFloat CurrentStamina { get; set; }
        [field: ReadOnly, 
                ShowInInspector] public FishingRodItemInstance CurrentFishingRod { get; private set; }
        
        public FishermanItemInstance(FishermanItemData itemData) : base(itemData)
        {
            CurrentStamina = ItemData.MaxStamina;
            CurrentFishingRod = new FishingRodItemInstance(ItemData.FishingRod);
        }
    }
}