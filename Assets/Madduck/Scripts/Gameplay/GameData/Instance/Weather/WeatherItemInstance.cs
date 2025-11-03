using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class WeatherItemInstance : ItemInstance<WeatherItemData>, IModifierSource
    {
        public event Action OnDisposed;
        
        public ISynchronizedView<KeyValuePair<ModifierId, List<BaseModifierData>>, KeyValuePair<ModifierId, List<BaseModifierData>>> ModifiersView
        {
            get;
        }

        public IReadOnlyList<KeyValuePair<ModifierId, List<BaseModifierData>>> Modifiers => _modifiers.ToList();
        
        [field: DisplayAsString, 
                ShowInInspector] public ReadOnlyReactiveProperty<WindDirection> CurrentWindDirection { get; private set; }
        [field: DisplayAsString, 
                ShowInInspector] public ReadOnlyReactiveProperty<WindStrength> CurrentWindStrength { get; private set; }
        [field: ShowInInspector] public WeatherStats CurrentStats { get; private set; }
        
        [field: ShowInInspector] private WindDirectionWeightTableInstance _windDirectionWeightTableInstance;
        [field: ShowInInspector] private WindStrengthWeightTableInstance _windStrengthWeightTableInstance;
        
        private readonly IPublisher<ModifierSourceEvent> _modifierSourceEventPublisher;
        private readonly ReactiveProperty<WindDirection> _currentWindDirection = new();
        private readonly ReactiveProperty<WindStrength> _currentWindStrength = new();
        [ShowInInspector] private readonly ObservableDictionary<ModifierId, List<BaseModifierData>> _modifiers = new();
        private IDisposable _subscriptions;
        
        [Inject]
        public WeatherItemInstance(
            WeatherItemData itemData, 
            IModifierSource modifierSource,
            IPublisher<ModifierSourceEvent> modifierSourceEventPublisher)
            : this(
                itemData, 
                null,
                null,
                modifierSource,
                modifierSourceEventPublisher) { }
        
        public WeatherItemInstance(
            WeatherItemData itemData, 
            WindDirection? windDirection,
            WindStrength? windStrength,
            IModifierSource modifierSource,
            IPublisher<ModifierSourceEvent> modifierSourceEventPublisher)
            : base(itemData)
        {
            _windDirectionWeightTableInstance = new WindDirectionWeightTableInstance(itemData.WindDirectionWeightTable, modifierSource);
            _windStrengthWeightTableInstance = new WindStrengthWeightTableInstance(itemData.WindStrengthWeightTable, modifierSource);
            CurrentWindDirection = _currentWindDirection.ToReadOnlyReactiveProperty();
            CurrentWindStrength = _currentWindStrength.ToReadOnlyReactiveProperty();
            _modifierSourceEventPublisher = modifierSourceEventPublisher;
            CurrentStats = new WeatherStats(itemData);
            ModifiersView = _modifiers.CreateView(x => x);
            _modifierSourceEventPublisher?.Publish(new ModifierSourceEvent(this));
            Subscribe();
            SetWindDirection(windDirection);
            SetWindStrength(windStrength);
        }
        
        private void SetWindDirection(WindDirection? windDirection)
        {
            windDirection ??= _windDirectionWeightTableInstance.GetRandomItem();
            _currentWindDirection.Value = windDirection.Value;
        }
        
        private void SetWindStrength(WindStrength? windStrength)
        {
            windStrength ??= _windStrengthWeightTableInstance.GetRandomItem();
            _currentWindStrength.Value = windStrength.Value;
        }

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _currentWindDirection
                .Subscribe(_ => UpdateModifiers())
                .AddTo(ref disposableBuilder);
            _currentWindStrength
                .Subscribe(_ => UpdateModifiers())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _subscriptions.Dispose();
            OnDisposed?.Invoke();
        }
        
        private void UpdateModifiers()
        {
            //_modifiers.Clear();
            //NOTE: Clear in reverse to avoid collection modified errors
            var count = _modifiers.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                _modifiers.Remove(_modifiers.ElementAt(i).Key);
            }
            var weatherType = ItemData.WeatherType;
            var windDirection = _currentWindDirection.Value;
            var windStrength = _currentWindStrength.Value;
            if (!CurrentStats.CurrentWindModifiers.TryGetValue(windDirection, out var windStrengthModifier))
            {
                DebugUtils.LogError($"Wind direction {windDirection} not found in wind modifiers for weather item {weatherType}");
                return;
            }
            if (!windStrengthModifier.ModifiersDictionary.TryGetValue(windStrength, out var modifiers))
            {
                DebugUtils.LogError($"Wind strength {windStrength} not found in wind modifiers for weather item {weatherType} and direction {windDirection}");
                return;
            }
            
            var displayName = $"{weatherType}_{windDirection}_{windStrength}";
            var modifierId = new ModifierId(InstanceGuid, displayName);
            if (!_modifiers.ContainsKey(modifierId))
            {
                _modifiers[modifierId] = new List<BaseModifierData>(modifiers);
            }
        }
    }

    [Serializable]
    public record WeatherStats : IStatModifiable<WeatherStats>
    {
        [field: ShowInInspector] public Dictionary<WindDirection, WindStrengthModifier> CurrentWindModifiers { get; set; } = new();

        public WeatherStats(WeatherItemData itemData)
        {
            CurrentWindModifiers = itemData.WindModifiers.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value);
        }
        public WeatherStats Copy() => this with { };
    }
}