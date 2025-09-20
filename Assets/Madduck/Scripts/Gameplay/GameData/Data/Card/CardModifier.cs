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
        /// Multiplier based on the base value.
        /// </summary>
        BaseMultiply,
        /// <summary>
        /// Flat increase based on the base value.
        /// </summary>
        BaseFlat,
        /// <summary>
        /// Percent increase based on the total value after base modifiers have been applied.
        /// </summary>
        TotalPercent,
        /// <summary>
        /// Multiplier based on the total value after base modifiers have been applied.
        /// </summary>
        TotalMultiply,
        /// <summary>
        /// Flat increase to the total value after base modifiers have been applied.
        /// </summary>
        TotalFlat,
    }

    public interface IStatModifiable<out T>
    {
        public T Copy();
    }

    [Serializable]
    public abstract class BaseModifierData
    {
        [field: SerializeField] public ModifierMethod ModifierMethod { get; private set; }
        [field: ShowIf(nameof(ShowValue)),
            SerializeField] public float ModifierValue { get; private set; }
        [field: ShowIf(nameof(ShowPercent)), InlineProperty,
                SerializeField] public Percentage ModifierPercentage { get; private set; }

        public BaseModifierData() { } // For serialization

        public BaseModifierData(ModifierMethod modifierMethod, float modifierValue)
        {
            ModifierMethod = modifierMethod;
            ModifierValue = modifierValue;
        }

        public BaseModifierData(ModifierMethod modifierMethod, Percentage modifierPercentage)
        {
            ModifierMethod = modifierMethod;
            ModifierPercentage = modifierPercentage;
        }

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
        public RodModifierData() { } // For serialization

        public RodModifierData(ModifierMethod modifierMethod, float modifierValue,
            FishingRodStatType fishingRodStatType) : base(modifierMethod, modifierValue)
        {
            FishingRodStatType = fishingRodStatType;
        }
        
        public RodModifierData(ModifierMethod modifierMethod, Percentage modifierPercentage,
            FishingRodStatType fishingRodStatType) : base(modifierMethod, modifierPercentage)
        {
            FishingRodStatType = fishingRodStatType;
        }
    }

    [Serializable]
    public class WeatherModifierData : BaseModifierData
    {
        [field: UnflagEnum,
            SerializeField] public WeatherType WeatherType { get; private set; }
        
        public WeatherModifierData() { } // For serialization
        public WeatherModifierData(ModifierMethod modifierMethod, float modifierValue,
            WeatherType weatherType) : base(modifierMethod, modifierValue)
        {
            WeatherType = weatherType;
        }
        
        public WeatherModifierData(ModifierMethod modifierMethod, Percentage modifierPercentage,
            WeatherType weatherType) : base(modifierMethod, modifierPercentage)
        {
            WeatherType = weatherType;
        }
    }
    
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
                    case ModifierMethod.BaseMultiply:
                        baseContributions += baseValue * modifier.ModifierValue;
                        result += baseContributions;
                        break;
                    case ModifierMethod.BaseFlat:
                        baseContributions += modifier.ModifierValue;
                        result += baseContributions;
                        break;
                    case ModifierMethod.TotalPercent:
                        result *= modifier.ModifierPercentage.AsMultiplier;
                        break;
                    case ModifierMethod.TotalMultiply:
                        result *= modifier.ModifierValue;
                        break;
                    case ModifierMethod.TotalFlat:
                        result += modifier.ModifierValue;
                        break;
                }
            }
        
            return result;
        }
    }
}