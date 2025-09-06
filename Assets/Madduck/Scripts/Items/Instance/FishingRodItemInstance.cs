using System;
using MadDuck.Scripts.FishingRods;
using MadDuck.Scripts.Items.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Instance
{
    [Serializable]
    public class FishingRodItemInstance : ItemInstance
    {
        [Title("Debug Stats")]
        [ShowInInspector, DisplayAsString] 
        public Vector2 CurrentThrowingDistance { get; set; }
        [ShowInInspector, DisplayAsString] 
        public float CurrentPower { get; set; }
        [field: SerializeField, DisplayAsString] 
        public float CurrentFishingLineDurability { get; set; }
        [field: SerializeField, DisplayAsString] 
        public float CurrentFishingLineRegenFactor { get; set; }
        [field: SerializeField, DisplayAsString]
        public float CurrentFishingRodDurability { get; set; }
        [field: SerializeField] 
        public float CurrentReelingSpeed { get; set; }
        
        public FishingRodItemData FishingRodItemData => ItemData as FishingRodItemData;
        public FishingRodStatsData BaseStats => FishingRodItemData ? FishingRodItemData.BaseStats : null;
        
        public FishingRodItemInstance(ItemData itemData) : base(itemData)
        {
            CurrentThrowingDistance = BaseStats.ThrowingDistanceRange;
            CurrentPower = BaseStats.Power;
            CurrentFishingLineDurability = BaseStats.FishingLineDurability;
            CurrentFishingLineRegenFactor = BaseStats.FishingLineRegenFactor;
            CurrentFishingRodDurability = BaseStats.FishingRodDurability;
            CurrentReelingSpeed = BaseStats.ReelingSpeed;
        }
        
        public void InitializeStats()
        {
            CurrentThrowingDistance = BaseStats.ThrowingDistanceRange;
            CurrentPower = BaseStats.Power;
            CurrentFishingLineDurability = BaseStats.FishingLineDurability;
            CurrentFishingLineRegenFactor = BaseStats.FishingLineRegenFactor;
            CurrentReelingSpeed = BaseStats.ReelingSpeed;
        }
    }
}