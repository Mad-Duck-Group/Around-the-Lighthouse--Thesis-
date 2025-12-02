using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Resource Probability", menuName = "Madduck/Resource/Resource Probability")]
    public class ResourceWeightTable : ScriptableObject, IWeightTable<ResourceWeightRecord>
    {
        [field: OnValueChanged(nameof(CalculateProbabilities)), 
                TableList,
                SerializeField] public List<ResourceWeightRecord> Records { get; private set; } = new();

        [Button("Refresh")]
        public void CalculateProbabilities()
        {
            var totalWeight = Records.Sum(probability => probability.Weight);
            foreach (var probability in Records)
            {
                probability.Probability = Percentage.FromFraction(probability.Weight / totalWeight);
            }
        }
        
        public IWeightTableInstance CreateInstance()
        {
            return new ResourceWeightTableInstance(this, null!);
        }
    }
}