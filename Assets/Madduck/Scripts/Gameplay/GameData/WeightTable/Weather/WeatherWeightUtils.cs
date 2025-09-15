using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public record WeatherWeightRecord : IWeightRecord<WeatherType>
    {
        [field: Required,
                SerializeField]
        public WeatherType Item { get; internal set; }

        [field: MinValue(0f),
                SerializeField]
        public float Weight { get; internal set; } = 1f;

        [field: ReadOnly, DisplayAsString,
                ShowInInspector]
        public float Probability { get; internal set; }
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
            return records.Where(_predicate).ToList();
        }
    }

    public class WeatherWeightModifier : IWeightModifier<WeatherWeightRecord>
    {
        private readonly Func<WeatherWeightRecord, float> _modifier;

        public WeatherWeightModifier(Func<WeatherWeightRecord, float> modifier)
        {
            _modifier = modifier;
        }

        public List<WeatherWeightRecord> Modify(List<WeatherWeightRecord> records)
        {
            return records.Select(record => record with
            {
                Weight = record.Weight + _modifier(record)
            }).ToList();
        }
    }

    [Serializable]
    public class WeatherWeightTableInstance : IWeightTable<WeatherWeightRecord, WeatherType>
    {
        private List<WeatherWeightRecord> BaseRecords { get; set; }
        public Dictionary<string, IWeightFilter<WeatherWeightRecord>> PersistentFilters { get; private set; }
        public Dictionary<string, IWeightModifier<WeatherWeightRecord>> PersistentModifiers { get; private set; }

        [Title("Debug")] 
        [ReadOnly, 
         ShowInInspector] private List<WeatherWeightRecord> _modifiedRecords = new();

        public WeatherWeightTableInstance(List<WeatherWeightRecord> baseRecords)
        {
            BaseRecords = baseRecords;
            PersistentFilters = new Dictionary<string, IWeightFilter<WeatherWeightRecord>>();
            PersistentModifiers = new Dictionary<string, IWeightModifier<WeatherWeightRecord>>();
        }

        public WeatherType GetRandomItem()
        {
            var filteredRecords = BaseRecords.ToList();
            foreach (var filter in PersistentFilters.Values)
            {
                filteredRecords = filter.Filter(filteredRecords);
            }
            foreach (var modifier in PersistentModifiers.Values)
            {
                filteredRecords = modifier.Modify(filteredRecords);
            }
            _modifiedRecords = filteredRecords;
            var totalWeight = filteredRecords.Sum(record => record.Weight);
            //update probabilities
            foreach (var record in filteredRecords)
            {
                record.Probability = record.Weight / totalWeight;
            }
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
            return default;
        }
    }
}