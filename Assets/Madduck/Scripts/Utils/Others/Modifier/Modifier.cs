using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MessagePipe;
using ObservableCollections;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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

    public enum ModifierValueType
    {
        Constant,
        Curve,
        Step,
        Incremental
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
        private List<IObjectResolver> _containers = new();
        
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
            foreach (var modifierData in source.Modifiers.SelectMany(x => x.Value))
            {
                foreach (var container in _containers)
                {
                    modifierData.ModifierContextProvider?.Inject(container);
                }
            }
            source.Modifiers.OnModifierFirstSubscribe(_modifiers);
            source.ModifiersView.ObserveChanged()
                .Subscribe(x =>
                {
                    if (x.NewItem.View.Value is { Count: > 0 })
                    {
                        foreach (var modifierData in x.NewItem.View.Value)
                        {
                            foreach (var container in _containers)
                            {
                                modifierData.ModifierContextProvider?.Inject(container);
                            }
                        }
                    }
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
        
        public void AddContainer(IObjectResolver container)
        {
            if (!_containers.Contains(container))
                _containers.Add(container);
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
         ShowInInspector]
        public Guid SourceId { get; private set; } = SourceId;

        [DisplayAsString, HideLabel,
         ShowInInspector]
        public string DisplayName { get; private set; } = DisplayName;
    }

    public interface IModifierContextProvider
    {
        void Inject(IObjectResolver resolver);
        bool TryGetEvaluationParameter(ModifierValueType modifierValueType, out float parameter);
    }


    [Serializable]
    public abstract class BaseModifierData
    {
        [field: SerializeField] public ModifierMethod ModifierMethod { get; internal set; }
        [field: SerializeField] public ModifierValueType ModifierValueType { get; internal set; } = ModifierValueType.Constant;
        [field: ShowIf(nameof(ShowValue)),
                InlineProperty,
            SerializeField] public float ModifierValue { get; internal set; }
        [field: ShowIf(nameof(ModifierValueType), ModifierValueType.Curve),
                SerializeField] public Vector2 ModifierCurveRange { get; internal set; } = new(0, 1);
        [field: ShowIf(nameof(ModifierValueType), ModifierValueType.Curve),
                SerializeField] public AnimationCurve ModifierCurve { get; internal set; } = AnimationCurve.Linear(0, 0, 1, 1);
        [field: ShowIf(nameof(ModifierValueType), ModifierValueType.Step),
                SerializeField] public List<float> StepValues { get; internal set; } = new();
        [field: ShowIf(nameof(ModifierValueType), ModifierValueType.Incremental),
                SerializeField] public bool HasMaxIncrementCount { get; internal set; } = false;
        [field: ShowIf(nameof(ShowMaxIncrementCount)),
                SerializeField] public uint MaxIncrementCount { get; internal set; } = 5;
        [field: HideIf(nameof(ModifierValueType), ModifierValueType.Constant),
            OdinSerialize] public IModifierContextProvider ModifierContextProvider { get; internal set; }

        [field: ValueDropdown("@ModifierKeys.Keys"),
            SerializeField] public List<string> Keys { get; internal set; } = new();
        
        private bool ShowValue()
        {
            return ModifierValueType is not (ModifierValueType.Curve or ModifierValueType.Step);
        }
        
        private bool ShowMaxIncrementCount()
        {
            return ModifierValueType == ModifierValueType.Incremental && HasMaxIncrementCount;
        }
        
        public float GetCurveValue(float t)
        {
            if (ModifierValueType != ModifierValueType.Curve)
            {
                DebugUtils.LogWarning("ModifierValueType is not Curve, returning 0");
                return 0f;
            }
            var normalized = Mathf.Clamp01(t);
            var value = ModifierCurve.Evaluate(normalized);
            return Mathf.Lerp(ModifierCurveRange.x, ModifierCurveRange.y, value);
        }
        
        public float GetStepValue(int stepIndex)
        {
            if (ModifierValueType != ModifierValueType.Step)
            {
                DebugUtils.LogWarning("ModifierValueType is not Step, returning 0");
                return 0f;
            }
            if (StepValues == null || StepValues.Count == 0)
            {
                DebugUtils.LogWarning("StepValues is null or empty, returning 0");
                return 0f;
            }
            if (stepIndex < 0 || stepIndex >= StepValues.Count)
            {
                DebugUtils.LogWarning("Step index out of range, returning last value");
                return StepValues[^1];
            }
            return StepValues[stepIndex];
        }
        
        public float GetIncrementalValue(int incrementIndex)
        {
            if (ModifierValueType != ModifierValueType.Incremental)
            {
                DebugUtils.LogWarning("ModifierValueType is not Incremental, returning 0");
                return 0f;
            }
            if (HasMaxIncrementCount)
            {
                incrementIndex = Mathf.Clamp(incrementIndex, 0, (int)MaxIncrementCount);
            }
            return ModifierValue * incrementIndex;
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
            modifierData.ModifierValue = percentage.AsPercentage;
            return this;
        }

        public ModifierDataBuilder<T> WithValue(float value)
        {
            if (modifierData.ModifierMethod is (ModifierMethod.BasePercent or ModifierMethod.TotalPercent))
            {
                DebugUtils.LogWarning("Modifier method is based on percent, converting to percent from float as fraction instead");
                modifierData.ModifierValue = Percentage.FromFraction(value).AsPercentage;
                return this;
            }
            modifierData.ModifierValue = value;
            return this;
        }
        
        public ModifierDataBuilder<T> WithCurve(AnimationCurve curve, Vector2 range)
        {
            if (modifierData.ModifierValueType != ModifierValueType.Curve)
            {
                DebugUtils.LogWarning("Modifier value type is not Curve, setting it to Curve");
                modifierData.ModifierValueType = ModifierValueType.Curve;
            }
            modifierData.ModifierCurve = curve;
            modifierData.ModifierCurveRange = range;
            return this;
        }
        
        public ModifierDataBuilder<T> WithStepValues(List<float> stepValues)
        {
            if (modifierData.ModifierValueType != ModifierValueType.Step)
            {
                DebugUtils.LogWarning("Modifier value type is not Step, setting it to Step");
                modifierData.ModifierValueType = ModifierValueType.Step;
            }
            modifierData.StepValues = stepValues;
            return this;
        }
        
        public ModifierDataBuilder<T> WithIncrementalValue(float incrementalValue)
        {
            if (modifierData.ModifierValueType != ModifierValueType.Incremental)
            {
                DebugUtils.LogWarning("Modifier value type is not Incremental, setting it to Incremental");
                modifierData.ModifierValueType = ModifierValueType.Incremental;
            }
            modifierData.ModifierValue = incrementalValue;
            return this;
        }
        
        public ModifierDataBuilder<T> WithContextProvider(IModifierContextProvider contextProvider)
        {
            modifierData.ModifierContextProvider = contextProvider;
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
        public static float Calculate(this IEnumerable<BaseModifierData> modifiers, float baseValue)
        {
            var evaluationGroup = modifiers.GroupBy(m => m.ModifierValueType);
            float result = baseValue;
            foreach (var group in evaluationGroup)
            {
                switch (group.Key)
                {
                    case ModifierValueType.Constant:
                        result += group.CalculateConstant(baseValue);
                        break;
                    case ModifierValueType.Curve:
                        result += group.CalculateCurve(baseValue);
                        break;
                    case ModifierValueType.Step:
                        result += group.CalculateStep(baseValue);
                        break;
                    case ModifierValueType.Incremental:
                        result += group.CalculateIncremental(baseValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }
        private static float CalculateConstant(this IEnumerable<BaseModifierData> modifiers, float baseValue)
        {
            return CalculateInternal(modifiers, baseValue, modifier => modifier.ModifierValue);
        }
        
        private static float CalculateCurve(this IEnumerable<BaseModifierData> modifiers, float baseValue)
        {
            return CalculateInternal(modifiers, baseValue, modifier =>
            {
                if (modifier.ModifierContextProvider is null 
                    || !modifier.ModifierContextProvider.TryGetEvaluationParameter(ModifierValueType.Curve, out var t)) 
                    return modifier.ModifierValue;
                return modifier.GetCurveValue(t);
            });
        }
        
        private static float CalculateStep(this IEnumerable<BaseModifierData> modifiers, float baseValue)
        {
            return CalculateInternal(modifiers, baseValue, modifier => 
            {
                if (modifier.ModifierContextProvider is null 
                    || !modifier.ModifierContextProvider.TryGetEvaluationParameter(ModifierValueType.Step, out var parameter)) 
                    return modifier.ModifierValue;
                var stepIndex = Mathf.FloorToInt(parameter);
                return modifier.GetStepValue(stepIndex);
            });
        }
        
        private static float CalculateIncremental(this IEnumerable<BaseModifierData> modifiers, float baseValue)
        {
            return CalculateInternal(modifiers, baseValue, modifier => 
            {
                if (modifier.ModifierContextProvider is null 
                    || !modifier.ModifierContextProvider.TryGetEvaluationParameter(ModifierValueType.Incremental, out var parameter)) 
                    return modifier.ModifierValue;
                var incrementIndex = Mathf.FloorToInt(parameter);
                return modifier.GetIncrementalValue(incrementIndex);
            });
        }

        private static float CalculateInternal(IEnumerable<BaseModifierData> modifiers, float baseValue,
            Func<BaseModifierData, float> modifierValueGetter)
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
                var modifierValue = modifierValueGetter(modifier);
                switch (modifier.ModifierMethod)
                {
                    case ModifierMethod.BasePercent:
                        baseContributions += baseValue * Percentage.FromPercentage(modifierValue).AsFraction;
                        result += baseContributions;
                        break;
                    case ModifierMethod.BaseFlat:
                        baseContributions += modifierValue;
                        result += baseContributions;
                        break;
                    case ModifierMethod.TotalPercent:
                        result *= Percentage.FromPercentage(modifierValue).AsMultiplier;
                        break;
                    case ModifierMethod.TotalFlat:
                        result += modifierValue; 
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