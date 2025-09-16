using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Weather Weight Table", menuName = "Madduck/Weather/Weather Weight Table")]
    public class WeatherWeightTable : ScriptableObject
    {
        [field: OnValueChanged(nameof(CalculateProbabilities)), 
                TableList,
                SerializeField] private List<WeatherWeightRecord> Records { get; set; } = new();
        
        [Button("Refresh")]
        public void CalculateProbabilities()
        {
            var totalWeight = Records.Sum(fishProbability => fishProbability.Weight);
            foreach (var fishProbability in Records)
            {
                fishProbability.Probability = Percentage.FromFraction(fishProbability.Weight / totalWeight);
            }
        }

        public WeatherWeightTableInstance GetInstance()
        {
            return new WeatherWeightTableInstance(new List<WeatherWeightRecord>(Records));
        }
    }
}