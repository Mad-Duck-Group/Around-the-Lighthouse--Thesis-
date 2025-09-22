using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public record WeatherWeightRecord : IWeightRecord<WeatherType>, IStatModifiable<WeatherWeightRecord>
    {
        [field: UnflagEnum, 
                Required,
                SerializeField]
        public WeatherType Item { get; internal set; }

        [field: MinValue(0f),
                SerializeField]
        public UFloat Weight { get; set; } = 1f;

        [field: DisplayAsString(TextAlignment.Center),
                ShowInInspector]
        public Percentage Probability { get; internal set; }

        public WeatherWeightRecord Copy() => this with {};
    }

    public class WeatherWeightFilter : IWeightFilter<WeatherWeightRecord>
    {
        private readonly Func<WeatherWeightRecord, bool> _predicate;

        public WeatherWeightFilter(Func<WeatherWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<WeatherWeightRecord> Filter(List<WeatherWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }
    
    [Serializable]
    public class WeatherWeightTableInstance : 
        IWeightTable<WeatherWeightRecord, WeatherModifierData, WeatherType>, 
        IDisposable
    {
        private List<WeatherWeightRecord> BaseRecords { get; set; }
        public Dictionary<string, IWeightFilter<WeatherWeightRecord>> PersistentFilters { get; private set; }
        public Dictionary<ModifierId, List<WeatherModifierData>> PersistentModifiers { get; private set; }

        [Title("Debug")] 
        [ReadOnly, TableList,
         ShowInInspector] private List<WeatherWeightRecord> _modifiedRecords;
        [Button("Refresh")]
        private void Refresh() => ApplyFiltersAndModifiers();
        
        private readonly ISubscriber<ModifierUpdatedEvent> _modifierUpdatedEventSubscriber;
        private IDisposable _subscriptions;

        [Inject]
        public WeatherWeightTableInstance(
            WeatherWeightTable weatherWeightTable,
            ISubscriber<ModifierUpdatedEvent> modifierUpdatedEventSubscriber)
        {
            BaseRecords = weatherWeightTable.Records.Select(x => x.Copy()).ToList();
            PersistentFilters = new Dictionary<string, IWeightFilter<WeatherWeightRecord>>();
            PersistentModifiers = new Dictionary<ModifierId, List<WeatherModifierData>>();
            _modifierUpdatedEventSubscriber = modifierUpdatedEventSubscriber;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _modifierUpdatedEventSubscriber.Subscribe(OnModifierUpdated)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
        }

        private void OnModifierUpdated(ModifierUpdatedEvent eventData)
        {
            var newModifiers = eventData.ModifierProvider.GetModifiers<WeatherModifierData>();
            PersistentModifiers.UpdateModifierDictionary(newModifiers);
            ApplyFiltersAndModifiers();
        }
        
        private void ApplyFiltersAndModifiers()
        {
            _modifiedRecords = BaseRecords.Select(x => x.Copy()).ToList();
            foreach (var filter in PersistentFilters.Values)
            {
                _modifiedRecords = filter.Filter(_modifiedRecords);
            }
            var flattenModifiers = PersistentModifiers.SelectMany(x => x.Value).ToList();
            _modifiedRecords = flattenModifiers.ModifyBy(_modifiedRecords, data => data.WeatherType, record => record.Item);
            var totalWeight = _modifiedRecords.Sum(record => record.Weight);
            foreach (var record in _modifiedRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
        }

        public WeatherType GetRandomItem()
        {
            ApplyFiltersAndModifiers();
            var totalWeight = _modifiedRecords.Sum(record => record.Weight);
            var randomValue = UnityEngine.Random.Range(0f, totalWeight);
            var cumulativeWeight = 0f;
            foreach (var record in _modifiedRecords)
            {
                cumulativeWeight += record.Weight;
                if (randomValue <= cumulativeWeight)
                {
                    return record.Item;
                }
            }
            return default;
        }
    }
}