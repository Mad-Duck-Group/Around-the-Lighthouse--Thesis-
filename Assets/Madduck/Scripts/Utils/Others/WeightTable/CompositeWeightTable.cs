using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.Utils
{
    [Serializable]
    public record CompositeWeightRecord : IWeightRecord
    {
        [OdinSerialize] public object Item { get; private set; }

        [field: MinValue(0f), 
                SerializeField] public UFloat Weight { get; set; } = 1f;
        [field: DisplayAsString(TextAlignment.Center), 
                ShowInInspector] public Percentage Probability { get; internal set; }
        
        public CompositeWeightRecord(object item, UFloat weight)
        {
            Item = item;
            Weight = weight;
        }
    }

    [CreateAssetMenu(fileName = "New Composite Weight Table", menuName = "Madduck/Utils/Composite Weight Table")]
    public class CompositeWeightTable : SerializedScriptableObject
    {
        [field: HideReferenceObjectPicker,
                OdinSerialize] public Dictionary<string, IWeightTable> SubTables { get; } = new();
        
        [ReadOnly, TableList,
         ShowInInspector] private IReadOnlyList<CompositeWeightRecord> _recordDisplay;
        
        public List<CompositeWeightRecord> Records
        {
            get
            {
                var records = SubTables.Values.SelectMany(x => x.Records).ToList();
                var final = records.Select(x => new CompositeWeightRecord(x.Item, x.Weight)).ToList();
                return final;
            }
        }
        
        [Button("Refresh")]
        public void CalculateProbabilities()
        {
            _recordDisplay = Records.ToList();
            var totalWeight = _recordDisplay.Sum(fishProbability => fishProbability.Weight);
            foreach (var fishProbability in _recordDisplay)
            {
                fishProbability.Probability = Percentage.FromFraction(fishProbability.Weight / totalWeight);
            }
        }
    }
}