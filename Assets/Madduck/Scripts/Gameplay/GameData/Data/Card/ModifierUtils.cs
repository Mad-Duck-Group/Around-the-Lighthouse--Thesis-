using System;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    #region Modifier Data
    [Serializable]
    public class RodStatModifierData : BaseModifierData
    {
        [field: SerializeField] public FishingRodStatType FishingRodStatType { get; private set; }
        public class Builder : ModifierDataBuilder<RodStatModifierData>
        {
            private Builder(ModifierMethod modifierMethod) 
                : base(modifierMethod) { }
            
            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }
            
            public Builder WithFishingRodStatType(FishingRodStatType fishingRodStatType)
            {
                modifierData.FishingRodStatType = fishingRodStatType;
                return this;
            }
        }
    }

    [Serializable]
    public class WeatherWeightModifierData : BaseModifierData
    {
        [field: UnflagEnum,
            SerializeField] public WeatherType WeatherType { get; private set; }
        
        public class Builder : ModifierDataBuilder<WeatherWeightModifierData>
        {
            private Builder(ModifierMethod modifierMethod) : base(modifierMethod) { }

            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }
            
            public Builder WithWeatherType(WeatherType weatherType)
            {
                modifierData.WeatherType = weatherType;
                return this;
            }
        }
    }
    
    [Serializable]
    public class FishWeightModifierData : BaseModifierData
    {
        [field: SerializeField] public FishModifierType ModifierType { get; private set; }
        [field: ShowIf(nameof(ModifierType), FishModifierType.Size),
            SerializeField] public FishSize FishSize { get; private set; }
        [field: ShowIf(nameof(ModifierType), FishModifierType.Name),
            SerializeField] public FishItemData FishItemData { get; private set; }
        
        public class Builder : ModifierDataBuilder<FishWeightModifierData>
        {
            private Builder(ModifierMethod modifierMethod) : base(modifierMethod) { }

            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }

            public Builder WithSize(FishSize size)
            {
                modifierData.ModifierType = FishModifierType.Size;
                modifierData.FishSize = size;
                return this;
            }

            public Builder WithName(FishItemData fishItemData)
            {
                modifierData.ModifierType = FishModifierType.Name;
                modifierData.FishItemData = fishItemData;
                return this;
            }
        }
    }
    
    [Serializable]
    public class CardWeightModifierData : BaseModifierData
    {
        [field: SerializeField] public CardItemData ItemData { get; private set; }
        
        public class Builder : ModifierDataBuilder<CardWeightModifierData>
        {
            private Builder(ModifierMethod modifierMethod) : base(modifierMethod) { }

            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }
            
            public Builder WithName(CardItemData cardItemData)
            {
                modifierData.ItemData = cardItemData;
                return this;
            }
        }
    }
    
    [Serializable]
    public class CardRarityWeightModifierData : BaseModifierData
    {
        [field: UnflagEnum,
            SerializeField] public CardRarity CardRarity { get; private set; }
        
        public class Builder : ModifierDataBuilder<CardRarityWeightModifierData>
        {
            private Builder(ModifierMethod modifierMethod) : base(modifierMethod) { }

            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }
            
            public Builder WithRarity(CardRarity cardRarity)
            {
                modifierData.CardRarity = cardRarity;
                return this;
            }
        }
    }
    #endregion
}