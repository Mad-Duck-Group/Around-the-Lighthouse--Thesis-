#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.Utils
{
    [Serializable]
    public class CompositeWeightTableInstance
    {
        [ShowInInspector] private readonly Dictionary<string, IWeightTableInstance> _weightTableInstances = new();
        private readonly IModifierSource _modifierSource = null!;
        
        [ReadOnly, TableList,
         ShowInInspector] private IReadOnlyList<CompositeWeightRecord> _recordDisplay = null!;
        
        public List<CompositeWeightRecord> ModifiedRecords
        {
            get
            {
                var records = _weightTableInstances.Values.SelectMany(x => x.ModifiedRecords).ToList();
                var final = records.Select(x => new CompositeWeightRecord(x.Item, x.Weight)).ToList();
                return final;
            }
        }
        
        [Button("Refresh")]
        public void CalculateProbabilities()
        {
            _recordDisplay = ModifiedRecords.ToList();
            var totalWeight = _recordDisplay.Sum(fishProbability => fishProbability.Weight);
            foreach (var fishProbability in _recordDisplay)
            {
                fishProbability.Probability = Percentage.FromFraction(fishProbability.Weight / totalWeight);
            }
        }

        [Inject]
        public CompositeWeightTableInstance(
            CompositeWeightTable compositeWeightTable,
            [Key("ModifierContainer")] IModifierSource modifierSource)
        {
            _modifierSource = modifierSource;
            foreach (var subTable in compositeWeightTable.SubTables)
            {
                var instance = subTable.Value.CreateInstance();
                RegisterInstance(subTable.Key, instance);
            }
        }

        public CompositeWeightTableInstance(
            IDictionary<string, IWeightTableInstance> weightTableInstances)
        {
            _weightTableInstances = new Dictionary<string, IWeightTableInstance>(weightTableInstances);
        }

        public void RegisterInstance(string key, IWeightTableInstance weightTableInstance)
        {
            weightTableInstance.SetModifierSource(_modifierSource);
            _weightTableInstances[key] = weightTableInstance;
        }

        public void UnregisterInstance(string key)
        {
            _weightTableInstances.Remove(key);
        }
        
        public void UnregisterInstance(IWeightTableInstance weightTableInstance)
        {
            var key = _weightTableInstances.FirstOrDefault(x => x.Value == weightTableInstance).Key;
            if (key != null)
            {
                _weightTableInstances.Remove(key);
            }
        }
        
        public bool TryGetInstance(string key, out IWeightTableInstance? weightTableInstance)
        {
            return _weightTableInstances.TryGetValue(key, out weightTableInstance);
        }
        
        public bool TryGetInstance<T>(string key, out T? weightTableInstance)
        {
            weightTableInstance = default;
            if (!_weightTableInstances.TryGetValue(key, out var instance))
            {
                return false;
            }
            if (instance is T typedInstance)
            {
                weightTableInstance = typedInstance;
                return true;
            }
            weightTableInstance = default;
            return false;
        }
        
        public bool TryGetFirstInstanceOfType<T>(out T? weightTableInstance)
        {
            weightTableInstance = default;
            foreach (var instance in _weightTableInstances.Values)
            {
                if (instance is not T typedInstance) continue;
                weightTableInstance = typedInstance;
                return true;
            }
            return false;
        }
        
        public bool TryGetAllInstancesOfType<T>(out T[] weightTableInstances)
        {
            var tempList = new List<T>();
            foreach (var instance in _weightTableInstances.Values)
            {
                if (instance is not T typedInstance) continue;
                tempList.Add(typedInstance);
            }
            weightTableInstances = tempList.ToArray();
            return weightTableInstances.Length > 0;
        }
        
        public void SetKeys(params string[] keys)
        {
            foreach (var weightTableInstance in _weightTableInstances.Values)
            {
                weightTableInstance.SetKeys(keys);
            }
        }

        public bool TryGetRandomItem<T>(out T? randomItem)
        {
            randomItem = default;
            var allRecords = _weightTableInstances.Values.SelectMany(x => x.ModifiedRecords).ToList();
            var totalWeight = allRecords.Sum(x => x.Weight);
            var randomValue = UnityEngine.Random.Range(0f, totalWeight);
            var cumulativeWeight = 0f;
            foreach (var record in allRecords)
            {
                cumulativeWeight += record.Weight;
                if (randomValue > cumulativeWeight) continue;
                if (record.Item is T item)
                {
                    randomItem = item;
                    return true;
                }

                DebugUtils.Log($"The random item is not of type {typeof(T)}");
                return false;
            }

            DebugUtils.LogWarning($"Cannot produce random item for {typeof(T)}");
            return false;
        }
    }
}