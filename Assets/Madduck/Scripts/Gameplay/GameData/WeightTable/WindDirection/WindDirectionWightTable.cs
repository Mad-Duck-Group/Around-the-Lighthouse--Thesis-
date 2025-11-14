using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Wind Direction Weight Table", menuName = "Madduck/Weather/Wind Direction Weight Table")]
    public class WindDirectionWeightTable : ScriptableObject, IWeightTable<WindDirectionWeightRecord>
    {
        [field: OnValueChanged(nameof(CalculateProbabilities)), 
                TableList,
                SerializeField] public List<WindDirectionWeightRecord> Records { get; private set; } = new();
        
        [Button("Refresh")]
        public void CalculateProbabilities()
        {
            var totalWeight = Records.Sum(fishProbability => fishProbability.Weight);
            foreach (var fishProbability in Records)
            {
                fishProbability.Probability = Percentage.FromFraction(fishProbability.Weight / totalWeight);
            }
        }
        
        public IWeightTableInstance CreateInstance()
        {
            return new WindDirectionWeightTableInstance(this, null!);
        }
    }
}