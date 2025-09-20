using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData.Card
{
    public enum ModifierMethod
    {
        /// <summary>
        /// Flat value addition (e.g. base is 100, flat is 20, final value = 120)
        /// </summary>
        Flat,
        /// <summary>
        /// Percent of base value (e.g. base is 100, 20% of base is 20, final value = 20)
        /// </summary>
        BasePercentAdd,
        /// <summary>
        /// Multiplier based on base value (e.g. base is 100, 20% multiplier is 1.2, final value = 120)
        /// </summary>
        MultiplyTotal,
        /// <summary>
        /// Overrides the base value with the specified value
        /// </summary>
        Override
    }
    
    public interface IStatModifier
    {
        object Modifier { get; }
        object Modify(object obj);
    }
    
    public interface IStatModifier<TModifiable, out TModData> : IStatModifier 
        where TModifiable : IStatModifiable<TModifiable> 
        where TModData : BaseModifierData
    {
        new TModData Modifier { get; }
        TModifiable Modify(TModifiable obj);
    
        object IStatModifier.Modifier => Modifier;
        // Explicit implementation for the non-generic method
        object IStatModifier.Modify(object obj)
        {
            if (obj is TModifiable typedObj)
                return Modify(typedObj);
            throw new ArgumentException($"Expected {typeof(TModifiable)}");
        }
    }

    public interface IStatModifiable<out T>
    {
        public T Copy();
    }
    
    public interface IStatProvider<out T> where T : IStatModifiable<T>
    {
        public T GetBaseStats();
    }
    
    [Serializable]
    public class RodModifier : IStatModifier<FishingRodStats, RodModifierData>
    {
        [field: HideReferenceObjectPicker, HideLabel, InlineProperty,
         SerializeReference] public RodModifierData Modifier { get; private set; } = new();
        public FishingRodStats Modify(FishingRodStats baseValue)
        {
            var copy = baseValue.Copy();
            switch (Modifier.FishingRodStatType)
            {
                case FishingRodStatType.Power:
                    copy.CurrentPower = Modifier.ApplyModifier(copy.CurrentPower);
                    break;
                case FishingRodStatType.Resistance:
                    copy.CurrentResistance = Modifier.ApplyModifier(copy.CurrentResistance);
                    break;
                case FishingRodStatType.FishingLineDurability:
                    copy.CurrentFishingLineDurability = Modifier.ApplyModifier(copy.CurrentFishingLineDurability);
                    break;
                case FishingRodStatType.FishingLineRegenFactor:
                    copy.CurrentFishingLineRegenFactor = Modifier.ApplyModifier(copy.CurrentFishingLineRegenFactor);
                    break;
                case FishingRodStatType.ReelingSpeed:
                    copy.CurrentReelingSpeed = Modifier.ApplyModifier(copy.CurrentReelingSpeed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return copy;
        }
    }

    [Serializable]
    public abstract class BaseModifierData
    {
        [field: SerializeField] public ModifierMethod ModifierMethod { get; private set; }
        [field: ShowIf(nameof(ShowValue)),
            SerializeField] public float ModifierValue { get; private set; }
        [field: ShowIf(nameof(ShowPercent)), InlineProperty,
                SerializeField] public Percentage ModifierPercentage { get; private set; }

        private bool ShowPercent()
        {
            if (ModifierMethod is ModifierMethod.BasePercentAdd)
                return true;
            return false;
        }
        
        private bool ShowValue()
        {
            if (ModifierMethod is ModifierMethod.BasePercentAdd)
                return false;
            return true;
        }
    }
    
    [Serializable]
    public class RodModifierData : BaseModifierData
    {
        [field: SerializeField] public FishingRodStatType FishingRodStatType { get; private set; }
    }
    
    public static class ModifierUtils
    {
        public static float ApplyModifier(this BaseModifierData baseModifierData, float baseValue)
        {
            switch (baseModifierData.ModifierMethod)
            {
                case ModifierMethod.Flat:
                    baseValue += baseModifierData.ModifierValue;
                    break;
                case ModifierMethod.BasePercentAdd:
                    baseValue *= baseModifierData.ModifierPercentage.AsMultiplier;
                    break;
                case ModifierMethod.MultiplyTotal:
                    baseValue *= baseModifierData.ModifierValue;
                    break;
                case ModifierMethod.Override:
                    baseValue = baseModifierData.ModifierValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return baseValue;
        }
        
        public static IEnumerable<T> SortModifiers<T>(this IEnumerable<T> modifiers) where T : IStatModifier
        {
            // Sort by ModifierMethod in the order of Flat, BasePercentAdd, MultiplyTotal, Override
            var modifierList = new List<T>(modifiers);
            modifierList.Sort((a, b) =>
            {
                if (a.Modifier is BaseModifierData aData && b.Modifier is BaseModifierData bData)
                {
                    return aData.ModifierMethod.CompareTo(bData.ModifierMethod);
                }
                return 0;
            });
            return modifierList;
        }
    }

}