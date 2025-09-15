using System;
using System.Collections.Generic;
using System.Linq;
using MadDuck.Scripts.Items.Data;
using Madduck.Scripts.Utils.Others;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.Items
{
    [Serializable]
    public record FishWeightRecord : IWeightRecord<FishItemData>
    {
        [field: Required, 
                SerializeField] public FishItemData Item { get; internal set; }
        [field: MinValue(0f), 
                SerializeField] public float Weight { get; internal set; } = 1f;
        [field: ReadOnly, DisplayAsString, 
                ShowInInspector] public float Probability { get; internal set; }
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
            return records.Where(_predicate).ToList();
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
                Weight = record.Weight + _modifier(record)
            }).ToList();
        }
    }
    
    public class FishWeightTableInstance : IWeightTable<FishWeightRecord, FishItemData>
    {
        private List<FishWeightRecord> BaseRecords { get; set; }
        public Dictionary<string, IWeightFilter<FishWeightRecord>> PersistentFilters { get; private set; }
        public Dictionary<string, IWeightModifier<FishWeightRecord>> PersistentModifiers { get; private set; }

        public FishWeightTableInstance(List<FishWeightRecord> baseRecords)
        {
            BaseRecords = baseRecords;
            PersistentFilters = new Dictionary<string, IWeightFilter<FishWeightRecord>>();
            PersistentModifiers = new Dictionary<string, IWeightModifier<FishWeightRecord>>();
        }
        
        public FishItemData GetRandomItem()
        {
            var filteredRecords = new List<FishWeightRecord>(BaseRecords);
            foreach (var filter in PersistentFilters.Values)
            {
                filteredRecords = filter.Filter(filteredRecords);
            }
            foreach (var modifier in PersistentModifiers.Values)
            {
                filteredRecords = modifier.Modify(filteredRecords);
            }
            var totalWeight = filteredRecords.Sum(record => record.Weight);
            var randomValue = UnityEngine.Random.Range(0f, totalWeight);
            var cumulativeWeight = 0f;
            foreach (var record in filteredRecords)
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