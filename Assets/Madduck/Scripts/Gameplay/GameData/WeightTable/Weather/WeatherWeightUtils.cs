using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public record WeatherWeightRecord : IWeightRecord<WeatherType>, IStatModifiable<WeatherWeightRecord>
    {
        [field: Required,
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

    public class WeatherWeightModifier : IWeightModifier<WeatherWeightRecord>
    {
        private readonly List<WeatherModifierData> _modifier;

        public WeatherWeightModifier(List<WeatherModifierData> modifier)
        {
            _modifier = modifier;
        }

        public List<WeatherWeightRecord> Modify(List<WeatherWeightRecord> records)
        {
            return _modifier.ModifyBy(records, data => data.WeatherType, record => record.Item);
        }
    }

    [Serializable]
    public class WeatherWeightTableInstance : IWeightTable<WeatherWeightRecord, WeatherType>
    {
        private List<WeatherWeightRecord> BaseRecords { get; set; }
        public Dictionary<string, IWeightFilter<WeatherWeightRecord>> PersistentFilters { get; private set; }
        public Dictionary<string, IWeightModifier<WeatherWeightRecord>> PersistentModifiers { get; private set; }

        [Title("Debug")] 
        [ReadOnly, TableList,
         ShowInInspector] private List<WeatherWeightRecord> _modifiedRecords = new();
        [Button("Refresh")]
        private void Refresh() => ApplyFiltersAndModifiers(out _);

        public WeatherWeightTableInstance(List<WeatherWeightRecord> baseRecords)
        {
            BaseRecords = baseRecords.Select(x => x.Copy()).ToList();
            PersistentFilters = new Dictionary<string, IWeightFilter<WeatherWeightRecord>>();
            PersistentModifiers = new Dictionary<string, IWeightModifier<WeatherWeightRecord>>();
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
            //update probabilities
            foreach (var record in filteredRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
            _modifiedRecords = filteredRecords;
        }

        public WeatherType GetRandomItem()
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
            return default;
        }
    }
}