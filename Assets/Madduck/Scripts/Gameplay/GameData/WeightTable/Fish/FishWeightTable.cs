using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Fish Probability", menuName = "Madduck/Fish/Fish Probability")]
    public class FishWeightTable : ScriptableObject, IWeightTable<FishWeightRecord>
    {
        [field: OnValueChanged(nameof(CalculateProbabilities)), 
                TableList,
                SerializeField] public List<FishWeightRecord> Records { get; private set; } = new();

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
            return new FishWeightTableInstance(this, null!);
        }
    }
}
