using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ObservableCollections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Utils
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

    public interface IHasModifier
    {
        public List<BaseModifierData> Modifiers { get; }
    }

    /// <summary>
    /// Interface for a source of modifiers.
    /// </summary>
    public interface IModifierSource
    {
        public ISynchronizedView<KeyValuePair<ModifierId, List<BaseModifierData>>, KeyValuePair<ModifierId, List<BaseModifierData>>> ModifiersView { get; }
    }

    [Serializable]
    public record ModifierId(Guid SourceId, string DisplayName = null)
    {
        [DisplayAsString, 
         ShowInInspector] public Guid SourceId { get; private set; } = SourceId;
        [DisplayAsString, 
         ShowInInspector] public string DisplayName { get; private set; } = DisplayName;
    }
    
    
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
        
        /// <summary>
        /// Updates the modifiers dictionary based on the provided view changed event.
        /// </summary>
        /// <typeparam name="T">The type of modifier data.</typeparam>
        /// <param name="modifiers">The modifiers dictionary to update.</param>
        /// <param name="viewChangedEvent">The view changed event containing the new and old items.</param>
        public static void OnModifierChanged<T>(
            this Dictionary<ModifierId, List<T>> modifiers, 
            ViewChangedEvent<KeyValuePair<ModifierId, List<BaseModifierData>>, KeyValuePair<ModifierId, List<BaseModifierData>>> viewChangedEvent)
        where T : BaseModifierData
        {
            var newItem = viewChangedEvent.NewItem.View;
            var oldItem = viewChangedEvent.OldItem.View;
            var newModifiers = newItem.Value?.OfType<T>().ToList();
            switch (viewChangedEvent.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newItem.Value is null || 
                        newModifiers is null || 
                        newModifiers.Count == 0) return;
                    modifiers.TryAdd(newItem.Key, newModifiers);
                    break;
                case NotifyCollectionChangedAction.Move:
                    //Ignore because the modifiers are flattened
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (oldItem.Value is null) return;
                    modifiers.Remove(oldItem.Key);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (oldItem.Value is null) return;
                    modifiers.Remove(oldItem.Key);
                    if (newItem.Value is null || 
                        newModifiers is null || 
                        newModifiers.Count == 0) return;
                    modifiers.TryAdd(newItem.Key, newModifiers);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    modifiers.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    #endregion
}