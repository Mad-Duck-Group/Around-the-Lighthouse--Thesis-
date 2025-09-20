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
                SerializeField] public UFloat Weight { get; internal set; } = 1f;
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
        private readonly Func<FishWeightRecord, float> _modifier;

        public FishWeightModifier(Func<FishWeightRecord, float> modifier)
        {
            _modifier = modifier;
        }

        public List<FishWeightRecord> Modify(List<FishWeightRecord> records)
        {
            return records.Select(record => record with
            {
                Weight = (float)record.Weight + _modifier(record)
            }).ToList();
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