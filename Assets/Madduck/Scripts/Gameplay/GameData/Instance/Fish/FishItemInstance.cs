using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    public enum FishQuality
    {
        Common,
        Good,
        Premium
    }
    
    [Serializable]
    public class FishItemInstance : ItemInstance<FishItemData>
    {
        [field: DisplayAsString, 
                ShowInInspector] public FishQuality CurrentFishQuality { get; set; }
        [field: ShowInInspector] public FishStats CurrentStats { get; private set; }
        public FishItemInstance(FishItemData itemData) : base(itemData)
        {
            CurrentStats = new FishStats(itemData);
        }
        
        public void SetFishQuality(FishQuality fishQuality)
        {
            CurrentFishQuality = fishQuality;
        }
        
        public void UpgradeFishQuality()
        {
            if (CurrentFishQuality == EnumUtils.Max<FishQuality>())
            {
                DebugUtils.LogWarning("Already at max quality");
                return;
            }
            CurrentFishQuality++;
        }
        
        public void DowngradeFishQuality()
        {
            if (CurrentFishQuality == EnumUtils.Min<FishQuality>())
            {
                DebugUtils.LogWarning("Already at min quality");
                return;
            }
            CurrentFishQuality--;
        }
    }

    [Serializable]
    public record FishStats : IStatModifiable<FishStats>
    {
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentPower { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentResistance { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentFishWeight { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentFatigueDuration { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentTugOfWarDecayRate { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public UFloat CurrentTugOfWarRegression { get; set; }
        
        public FishStats(FishItemData itemData)
        {
            CurrentPower = itemData.Power;
            CurrentResistance = itemData.Resistance;
            CurrentFishWeight = itemData.FishWeight;
            CurrentFatigueDuration = itemData.FatigueDuration;
            CurrentTugOfWarDecayRate = itemData.TugOfWarDecayRate;
            CurrentTugOfWarRegression = itemData.TugOfWarRegression;
        }
        
        public FishStats Copy() => this with { };
    }
}