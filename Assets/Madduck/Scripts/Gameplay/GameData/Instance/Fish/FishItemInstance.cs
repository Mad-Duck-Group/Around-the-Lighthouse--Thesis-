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
                ShowInInspector] public uint CurrentFatigueCount { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public FishQuality CurrentFishQuality { get; set; }
        [field: ShowInInspector] public FishStats CurrentStats { get; private set; }
        public FishItemInstance(FishItemData itemData) : base(itemData)
        {
            CurrentFatigueCount = 0;
            CurrentStats = new FishStats(itemData);
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
        
        public FishStats(FishItemData itemData)
        {
            CurrentPower = itemData.Power;
            CurrentResistance = itemData.Resistance;
            CurrentFishWeight = itemData.FishWeight;
            CurrentFatigueDuration = itemData.FatigueDuration;
        }
        
        public FishStats Copy() => this with { };
    }
}