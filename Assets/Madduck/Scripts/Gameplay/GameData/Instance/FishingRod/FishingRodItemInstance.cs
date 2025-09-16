using System;
using Madduck.Utils;
using Sirenix.OdinInspector;

namespace Madduck.GameData
{
    [Serializable]
    public class FishingRodItemInstance : ItemInstance<FishingRodItemData>
    {
        [Title("Debug Stats"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugStatsTitle;
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentPower { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentFishingLineDurability { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentFishingLineRegenFactor { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentReelingSpeed { get; set; }
        
        public FishingRodItemInstance(FishingRodItemData itemData) : base(itemData)
        {
            InitializeStats();
        }
        
        public void InitializeStats()
        {
            CurrentPower = ItemData.Power;
            CurrentFishingLineDurability = ItemData.FishingLineDurability;
            CurrentFishingLineRegenFactor = ItemData.FishingLineRegenFactor;
            CurrentReelingSpeed = ItemData.ReelingSpeed;
        }
    }
}