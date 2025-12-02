using System;
using Madduck.Utils;

namespace Madduck.GameData
{
    [Serializable]
    public class CardItemInstance : ItemInstance<CardItemData>
    {
        public CardRarity CurrentRarity { get; private set; } = CardRarity.Common;
        public CardItemInstance(CardItemData itemData) : base(itemData) { }
        public CardRarityData GetRarityData()
        {
            if (ItemData.RarityData.TryGetValue(CurrentRarity, out var rarityData)) return rarityData;
            DebugUtils.LogError($"There is no rarity data for rarity {CurrentRarity}");
            return null;
        }
        
        public void SetRarity(CardRarity rarity)
        {
            CurrentRarity = rarity;
        }

        public void UpgradeRarity()
        {
            if (CurrentRarity == EnumUtils.Max<CardRarity>())
            {
                DebugUtils.LogWarning("Card is already at max rarity");
                return;
            }
            CurrentRarity++;
        }
        
        public void DowngradeRarity()
        {
            if (CurrentRarity == EnumUtils.Min<CardRarity>())
            {
                DebugUtils.LogWarning("Card is already at min rarity");
                return;
            }
            CurrentRarity--;
        }
    }
}