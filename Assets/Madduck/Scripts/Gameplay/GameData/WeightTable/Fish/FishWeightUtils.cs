using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using DisposableBag = R3.DisposableBag;

namespace Madduck.GameData
{
    [Serializable]
    public record FishWeightRecord : IWeightRecord<FishItemData>, IStatModifiable<FishWeightRecord>
    {
        [field: Required, 
                SerializeField] public FishItemData Item { get; internal set; }
        [field: MinValue(0f), 
                SerializeField] public UFloat Weight { get; set; } = 1f;
        [field: DisplayAsString(TextAlignment.Center), 
                ShowInInspector] public Percentage Probability { get; internal set; }

        public FishWeightRecord Copy() => this with {};
    }
    
    public class FishWeightFilter : IWeightFilter<FishWeightRecord>
    {
        private readonly Func<FishWeightRecord, bool> _predicate;

        public FishWeightFilter(Func<FishWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<FishWeightRecord> Filter(List<FishWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }
    
    [Serializable]
    public class FishWeightTableInstance : IWeightTable<FishWeightRecord, FishWeightModifierData, FishItemData>, IDisposable
    {
        [Title("Debug")] 
        private List<FishWeightRecord> BaseRecords { get; set; }
        [ReadOnly, 
         ShowInInspector] public Dictionary<string, IWeightFilter<FishWeightRecord>> PersistentFilters { get; private set; }
        [ReadOnly, 
         ShowInInspector] public Dictionary<ModifierId, List<FishWeightModifierData>> PersistentModifiers { get; private set; }
        
        [ReadOnly, TableList,
         ShowInInspector] private List<FishWeightRecord> _modifiedRecords;

        [Button("Refresh")]
        private void Refresh() => ApplyFiltersAndModifiers();
        
        private readonly ISubscriber<ModifierSourceEvent> _modifierPublisherEventSubscriber;
        private IDisposable _subscriptions;
        private DisposableBag _modifierChangedSubscription;

        [Inject]
        public FishWeightTableInstance(
            FishWeightTable fishWeightTable,
            ISubscriber<ModifierSourceEvent> modifierPublisherEventSubscriber)
        {
            BaseRecords = fishWeightTable.Records.Select(x => x.Copy()).ToList();
            PersistentFilters = new Dictionary<string, IWeightFilter<FishWeightRecord>>();
            PersistentModifiers = new Dictionary<ModifierId, List<FishWeightModifierData>>();
            _modifierPublisherEventSubscriber = modifierPublisherEventSubscriber;
            Subscribe();
        }
        
        private void Subscribe()
        {
            DebugUtils.Log("Subscribing to modifier updated event");
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
            _modifiedRecords = Modify(_modifiedRecords);
            var totalWeight = _modifiedRecords.Sum(record => record.Weight);
            foreach (var record in _modifiedRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
        }
        
        private List<FishWeightRecord> Modify(List<FishWeightRecord> records)
        {
            var copy = records.Select(x => x.Copy()).ToList();
            var flattenedModifiers = PersistentModifiers.Values.SelectMany(x => x).ToList();
            var bucket = BucketModifiers(copy, flattenedModifiers);
            foreach (var pair in bucket)
            {
                pair.Key.Weight = pair.Value.CalculateStat(pair.Key.Weight);
            }
            return copy;
        }

        private static Dictionary<FishWeightRecord, List<FishWeightModifierData>> BucketModifiers(
            List<FishWeightRecord> records,
            List<FishWeightModifierData> modifiers)
        {
            var dictionary = records.Distinct().ToDictionary(x => x, _ => new List<FishWeightModifierData>());

            foreach (var modifier in modifiers)
            {
                foreach (var record in records)
                {
                    switch (modifier.ModifierType)
                    {
                        case FishModifierType.Name:
                            if (!modifier.FishItemData.Guid.Equals(record.Item.Guid)) break;
                            dictionary[record].Add(modifier);
                            break;
                        case FishModifierType.Size:
                            if (modifier.FishSize != record.Item.Size) break;
                            dictionary[record].Add(modifier);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            return dictionary;
        }
        
        public FishItemData GetRandomItem()
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
            return null;
        }
    }
}