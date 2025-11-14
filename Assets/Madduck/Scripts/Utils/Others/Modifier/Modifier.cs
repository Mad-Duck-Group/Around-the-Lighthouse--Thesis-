using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MessagePipe;
using ObservableCollections;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using DisposableBag = R3.DisposableBag;

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
        public event Action OnDisposed;
        public ISynchronizedView<KeyValuePair<ModifierId, List<BaseModifierData>>, KeyValuePair<ModifierId, List<BaseModifierData>>> ModifiersView { get; }
        public IReadOnlyList<KeyValuePair<ModifierId, List<BaseModifierData>>> Modifiers { get; }
    }
    
    /// <summary>
    /// Event that is sent out by ModifierSource to let subscribers subscribe to the modifiers.
    /// </summary>
    public readonly struct ModifierSourceEvent
    {
        public IModifierSource ModiferSource { get; }

        public ModifierSourceEvent(IModifierSource source)
        {
            ModiferSource = source;
        }
    }

    [Serializable]
    public class ModifierContainer : IModifierSource, IDisposable
    {
        public event Action OnDisposed;

        public ISynchronizedView<KeyValuePair<ModifierId, List<BaseModifierData>>, KeyValuePair<ModifierId, List<BaseModifierData>>> ModifiersView
        {
            get;
        }
        public IReadOnlyList<KeyValuePair<ModifierId, List<BaseModifierData>>> Modifiers => _modifiers.ToList();
        [ShowInInspector] private readonly ObservableDictionary<ModifierId, List<BaseModifierData>> _modifiers = new();
        private readonly Dictionary<IModifierSource, IDisposable> _disposeDictionary = new();
        private readonly ISubscriber<ModifierSourceEvent> _modifierSourceEvent;
        
        private DisposableBag _disposableBag;

        [Inject]
        public ModifierContainer(
            ISubscriber<ModifierSourceEvent> modifierSourceEvent)
        {
            _modifierSourceEvent = modifierSourceEvent;
            _disposableBag = new();
            ModifiersView = _modifiers.CreateView(x => x)
                .AddTo(ref _disposableBag);
            Subscribe();
        }

        private void Subscribe()
        {
            _modifierSourceEvent.Subscribe(x =>
                {
                    OnSubscribeModifierSource(x.ModiferSource);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnSubscribeModifierSource(IModifierSource source)
        {
            var disposable = Observable.FromEvent(
                    h => source.OnDisposed += h,
                    h => source.OnDisposed -= h)
                .Subscribe(_ => OnSourceDisposed(source));
            _disposeDictionary.Add(source, disposable);
            source.Modifiers.OnModifierFirstSubscribe(_modifiers);
            source.ModifiersView.ObserveChanged()
                .Subscribe(x =>
                {
                    x.OnModifierChanged(_modifiers);
                })
                .AddTo(ref _disposableBag);
        }
        
        private void OnSourceDisposed(IModifierSource source)
        {
            _disposeDictionary[source].Dispose();
            _disposeDictionary.Remove(source);
            source.Modifiers.ForEach(x => _modifiers.Remove(x.Key));
        }

        public void Dispose()
        {
            OnDisposed?.Invoke();
            _disposableBag.Dispose();
        }
    }
    
    public class ModifierSourceMock : IModifierSource
    {
        public event Action OnDisposed;
        public ISynchronizedView<KeyValuePair<ModifierId, List<BaseModifierData>>, KeyValuePair<ModifierId, List<BaseModifierData>>> ModifiersView
        {
            get;
        }

        public IReadOnlyList<KeyValuePair<ModifierId, List<BaseModifierData>>> Modifiers => _modifiers.ToList();
        
        private readonly ObservableDictionary<ModifierId, List<BaseModifierData>> _modifiers = new();
        public ModifierSourceMock()
        {
            ModifiersView = _modifiers.CreateView(x => x);
        }
    }

    [Serializable]
    public record ModifierId(Guid SourceId, string DisplayName = null)
    {
        [DisplayAsString, HideLabel,
         ShowInInspector] public Guid SourceId { get; private set; } = SourceId;
        [DisplayAsString, HideLabel,
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

        [field: ValueDropdown("@ModifierKeys.Keys"),
            SerializeField] public List<string> Keys { get; internal set; } = new();

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
        
        public BaseModifierData Copy()
        {
            return (BaseModifierData)MemberwiseClone();
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
        
        public ModifierDataBuilder<T> AddKey(string key)
        {
            modifierData.Keys ??= new List<string>();
            modifierData.Keys.Add(key);
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

        public static void OnModifierFirstSubscribe<T>(
            this IReadOnlyList<KeyValuePair<ModifierId, List<BaseModifierData>>> currentState,
            IDictionary<ModifierId, List<T>> modifiers,
            string[] keys = null)
        where T : BaseModifierData
        {
            currentState.ForEach(x => 
            {
                var newModifiers = x.Value?.OfType<T>().ToList();
                if (keys is { Length: > 0 })
                {
                    newModifiers = newModifiers?
                        .Where(modifier => modifier.Keys.Any(keys.Contains))
                        .ToList();
                }
                if (newModifiers is { Count: > 0 })
                    modifiers.TryAdd(x.Key, newModifiers);
            });
        }

        /// <summary>
        /// Updates the modifiers dictionary based on the provided view changed event.
        /// </summary>
        /// <typeparam name="T">The type of modifier data.</typeparam>
        /// <param name="modifiers">The modifiers dictionary to update.</param>
        /// <param name="viewChangedEvent">The view changed event containing the new and old items.</param>
        /// <param name="keys">Optional keys to filter the modifiers.</param>
        public static void OnModifierChanged<T>(
             this ViewChangedEvent<KeyValuePair<ModifierId, List<BaseModifierData>>, KeyValuePair<ModifierId, List<BaseModifierData>>> viewChangedEvent, 
             IDictionary<ModifierId, List<T>> modifiers,
             string[] keys = null)
        where T : BaseModifierData
        {
            var newItem = viewChangedEvent.NewItem.View;
            var newModifiers = newItem.Value?.OfType<T>().ToList();
            if (keys is { Length: > 0 })
            {
                newModifiers = newModifiers?
                    .Where(modifier => modifier.Keys.Any(keys.Contains))
                    .ToList();
            }
            OnModifierChangedInternal(modifiers, viewChangedEvent, newModifiers);
        }

        private static void OnModifierChangedInternal<T>(
            IDictionary<ModifierId, List<T>> modifiers, 
            ViewChangedEvent<KeyValuePair<ModifierId, List<BaseModifierData>>, KeyValuePair<ModifierId, List<BaseModifierData>>> viewChangedEvent,
            List<T> newModifiers)
        where T : BaseModifierData
        {
            var action = viewChangedEvent.Action;
            var newItem = viewChangedEvent.NewItem.View;
            var oldItem = viewChangedEvent.OldItem.View;
            switch (action)
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