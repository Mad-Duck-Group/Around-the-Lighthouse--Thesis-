using System;
using Madduck.Scripts.Items.Data;
using MadDuck.Scripts.Items.Data;
using MadDuck.Scripts.Utils.Inspectors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Instance
{
    [Serializable]
    public class FishingRodItemInstance : ItemInstance
    {
        [Title("Debug Stats"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _debugStatsTitle;
        [field: DisplayAsString, 
                ShowInInspector] public float CurrentPower { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public float CurrentFishingLineDurability { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public float CurrentFishingLineRegenFactor { get; set; }
        [field: DisplayAsString, 
                ShowInInspector] public float CurrentReelingSpeed { get; set; }
        
        public FishingRodItemData FishingRodItemData => ItemData as FishingRodItemData;
        public FishingRodStatsData BaseStats => FishingRodItemData ? FishingRodItemData.BaseStats : null;
        
        public FishingRodItemInstance(ItemData itemData) : base(itemData)
        {
            InitializeStats();
        }
        
        public void InitializeStats()
        {
            CurrentPower = BaseStats.Power;
            CurrentFishingLineDurability = BaseStats.FishingLineDurability;
            CurrentFishingLineRegenFactor = BaseStats.FishingLineRegenFactor;
            CurrentReelingSpeed = BaseStats.ReelingSpeed;
        }
    }
}