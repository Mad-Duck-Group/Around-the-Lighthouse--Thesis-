using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using DisposableBag = R3.DisposableBag;

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
        IWeightTable<WeatherWeightRecord, WeatherWeightModifierData, WeatherType>, 
        IDisposable
    {
        [Title("Debug")] 
        private List<WeatherWeightRecord> BaseRecords { get; set; }
        [ReadOnly, 
         ShowInInspector] public Dictionary<string, IWeightFilter<WeatherWeightRecord>> PersistentFilters { get; private set; }
        [ReadOnly, 
         ShowInInspector] public Dictionary<ModifierId, List<WeatherWeightModifierData>> PersistentModifiers { get; private set; }

        [Title("Debug")] 
        [ReadOnly, TableList,
         ShowInInspector] private List<WeatherWeightRecord> _modifiedRecords;
        [Button("Refresh")]
        private void Refresh() => ApplyFiltersAndModifiers();
        
        private readonly ISubscriber<ModifierSourceEvent> _modifierPublisherEventSubscriber;
        private IDisposable _subscriptions;
        private DisposableBag _modifierChangedSubscription;

        [Inject]
        public WeatherWeightTableInstance(
            WeatherWeightTable weatherWeightTable,
            ISubscriber<ModifierSourceEvent> modifierPublisherEventSubscriber)
        {
            BaseRecords = weatherWeightTable.Records.Select(x => x.Copy()).ToList();
            PersistentFilters = new Dictionary<string, IWeightFilter<WeatherWeightRecord>>();
            PersistentModifiers = new Dictionary<ModifierId, List<WeatherWeightModifierData>>();
            _modifierPublisherEventSubscriber = modifierPublisherEventSubscriber;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _modifierPublisherEventSubscriber.Subscribe(OnModifierPublished)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _modifierChangedSubscription.Dispose();
            _modifierChangedSubscription.Clear();
        }

        private void OnModifierPublished(ModifierSourceEvent eventData)
        {
            eventData.ModiferSource.ModifiersView.ObserveChanged()
                .Subscribe(x =>
                {
                    PersistentModifiers.OnModifierChanged(x);
                    ApplyFiltersAndModifiers();
                })
                .AddTo(ref _modifierChangedSubscription);
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