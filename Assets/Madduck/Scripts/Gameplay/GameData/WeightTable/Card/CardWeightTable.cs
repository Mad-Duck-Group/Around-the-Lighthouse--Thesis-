using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Card Weight Table", menuName = "Madduck/Card/Card Weight Table")]
    public class CardWeightTable : ScriptableObject, IWeightTable<CardWeightRecord>
    {
        [field: OnValueChanged(nameof(CalculateProbabilities)), 
                TableList,
                SerializeField] public List<CardWeightRecord> Records { get; private set; } = new();

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
            return new CardWeightTableInstance(this, null!);
        }
    }
}