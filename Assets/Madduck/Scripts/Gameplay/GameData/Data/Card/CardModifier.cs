using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    public enum ModifierMethod
    {
        /// <summary>
        /// Overrides the current value.
        /// </summary>
        Override,
        /// <summary>
        /// Percent increase based on the base value.
        /// </summary>
        BasePercent,
        /// <summary>
        /// Flat increase based on the base value.
        /// </summary>
        BaseFlat,
        /// <summary>
        /// Percent increase based on the total value after base modifiers have been applied.
        /// </summary>
        TotalPercent,
        /// <summary>
        /// Flat increase to the total value after base modifiers have been applied.
        /// </summary>
        TotalFlat,
    }

    public interface IStatModifiable<out T>
    {
        public T Copy();
    }

    #region Modifier Data
    [Serializable]
    public abstract class BaseModifierData
    {
        [field: SerializeField] public ModifierMethod ModifierMethod { get; internal set; }
        [field: ShowIf(nameof(ShowValue)),
            SerializeField] public float ModifierValue { get; internal set; }
        [field: ShowIf(nameof(ShowPercent)), InlineProperty,
                SerializeField] public Percentage ModifierPercentage { get; internal set; }

        private bool ShowPercent()
        {
            if (ModifierMethod is ModifierMethod.BasePercent or ModifierMethod.TotalPercent)
                return true;
            return false;
        }
        
        private bool ShowValue()
        {
            if (ModifierMethod is ModifierMethod.BasePercent or ModifierMethod.TotalPercent)
                return false;
            return true;
        }
    }
    
    [Serializable]
    public class RodModifierData : BaseModifierData
    {
        [field: SerializeField] public FishingRodStatType FishingRodStatType { get; private set; }
        public class Builder : ModifierDataBuilder<RodModifierData>
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
    public class WeatherModifierData : BaseModifierData
    {
        [field: UnflagEnum,
            SerializeField] public WeatherType WeatherType { get; private set; }
        
        public class Builder : ModifierDataBuilder<WeatherModifierData>
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
    public class FishModifierData : BaseModifierData
    {
        [field: SerializeField] public FishModifierType ModifierType { get; private set; }
        [field: ShowIf(nameof(ModifierType), FishModifierType.Size),
            SerializeField] public FishSize FishSize { get; private set; }
        [field: ShowIf(nameof(ModifierType), FishModifierType.Name),
            SerializeField] public FishItemData FishItemData { get; private set; }
        
        public class Builder : ModifierDataBuilder<FishModifierData>
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
    #endregion
    
    #region Builder
    public abstract class ModifierDataBuilder<T> where T : BaseModifierData, new()
    {
        protected readonly T modifierData;

        protected ModifierDataBuilder(ModifierMethod modifierMethod)
        {
            modifierData = new T
            {
                ModifierMethod = modifierMethod
            };
        }

        public ModifierDataBuilder<T> WithPercentage(Percentage percentage)
        {
            if (modifierData.ModifierMethod is not (ModifierMethod.BasePercent or ModifierMethod.TotalPercent))
            {
                DebugUtils.LogWarning("Modifier method is not based on percent, converting to float as fraction instead");
                modifierData.ModifierValue = percentage.AsFraction;
                return this;
            }
            modifierData.ModifierPercentage = percentage;
            return this;
        }

        public ModifierDataBuilder<T> WithValue(float value)
        {
            if (modifierData.ModifierMethod is (ModifierMethod.BasePercent or ModifierMethod.TotalPercent))
            {
                DebugUtils.LogWarning("Modifier method is based on percent, converting to percent from float as fraction instead");
                modifierData.ModifierPercentage = Percentage.FromFraction(value);
                return this;
            }
            modifierData.ModifierValue = value;
            return this;
        }

        public T Build()
        {
            return modifierData;
        }
    }
    #endregion
    
    #region Utils
    public static class ModifierUtils
    {
        /// <summary>
        /// Calculates a new value based on the provided base value and a list of modifiers.
        /// </summary>
        /// <param name="modifiers">A list of modifiers to apply to the base value.</param>
        /// <param name="baseValue">The base value to which the modifiers will be applied.</param>
        /// <returns>A new value calculated based on the provided base value and modifiers.</returns>
        /// <remarks>
        /// Modifiers are applied in the order of their <see cref="ModifierMethod"/>.
        /// If a modifier with <see cref="ModifierMethod.Override"/> is found, its value will be returned immediately.
        /// </remarks>
        public static float CalculateStat(this IEnumerable<BaseModifierData> modifiers, float baseValue)
        {
            var modifierList = modifiers.OrderBy(m => m.ModifierMethod).ToList();
            
            // Check for override
            var overrideMod = modifierList.FirstOrDefault(m => m.ModifierMethod == ModifierMethod.Override);
            if (overrideMod != null)
                return overrideMod.ModifierValue;
        
            float result = baseValue;
            float baseContributions = 0f;
        
            foreach (var modifier in modifierList)
            {
                switch (modifier.ModifierMethod)
                {
                    case ModifierMethod.BasePercent:
                        baseContributions += baseValue * modifier.ModifierPercentage.AsFraction;
                        result += baseContributions;
                        break;
                    case ModifierMethod.BaseFlat:
                        baseContributions += modifier.ModifierValue;
                        result += baseContributions;
                        break;
                    case ModifierMethod.TotalPercent:
                        result *= modifier.ModifierPercentage.AsMultiplier;
                        break;
                    case ModifierMethod.TotalFlat:
                        result += modifier.ModifierValue; 
                        break;
                }
            }
        
            return result;
        }
    }
    #endregion
}