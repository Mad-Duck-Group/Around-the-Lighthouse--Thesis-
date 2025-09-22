using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

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
    
    public class FishWeightModifier : IWeightModifier<FishWeightRecord>
    {
        private readonly List<FishModifierData> _modifier;

        public FishWeightModifier(List<FishModifierData> modifier)
        {
            _modifier = modifier;
        }

        public List<FishWeightRecord> Modify(List<FishWeightRecord> records)
        {
            var copy = records.Select(x => x.Copy()).ToList();
            var bucket = BucketModifiers(copy, _modifier);
            foreach (var pair in bucket)
            {
                pair.Key.Weight = pair.Value.CalculateStat(pair.Key.Weight);
            }
            return copy;
        }

        private static Dictionary<FishWeightRecord, List<FishModifierData>> BucketModifiers(
            List<FishWeightRecord> records,
            List<FishModifierData> modifiers)
        {
            var dictionary = records.Distinct().ToDictionary(x => x, _ => new List<FishModifierData>());

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
    }
    
    [Serializable]
    public class FishWeightTableInstance : IWeightTable<FishWeightRecord, FishItemData>
    {
        [Title("Debug")] 
        private List<FishWeightRecord> BaseRecords { get; set; }
        public Dictionary<string, IWeightFilter<FishWeightRecord>> PersistentFilters { get; private set; }
        public Dictionary<string, IWeightModifier<FishWeightRecord>> PersistentModifiers { get; private set; }
        
        [ReadOnly, TableList,
         ShowInInspector] private List<FishWeightRecord> _modifiedRecords = new();
        [Button("Refresh")]
        private void Refresh() => ApplyFiltersAndModifiers(out _);

        public FishWeightTableInstance(List<FishWeightRecord> baseRecords)
        {
            BaseRecords = baseRecords.Select(x => x.Copy()).ToList();
            PersistentFilters = new Dictionary<string, IWeightFilter<FishWeightRecord>>();
            PersistentModifiers = new Dictionary<string, IWeightModifier<FishWeightRecord>>();
        }
        
        private void ApplyFiltersAndModifiers(out float totalWeight)
        {
            var filteredRecords = BaseRecords;
            foreach (var filter in PersistentFilters.Values)
            {
                filteredRecords = filter.Filter(filteredRecords);
            }
            foreach (var modifier in PersistentModifiers.Values)
            {
                filteredRecords = modifier.Modify(filteredRecords);
            }
            totalWeight = filteredRecords.Sum(record => record.Weight);
            foreach (var record in filteredRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
            _modifiedRecords = filteredRecords;
        }
        
        public FishItemData GetRandomItem()
        {
            ApplyFiltersAndModifiers(out var totalWeight);
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