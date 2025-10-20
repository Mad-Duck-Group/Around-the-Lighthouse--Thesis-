using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Card Rarity Weight Table", menuName = "Madduck/Card/Card Rarity Weight Table")]
    public class CardRarityWeightTable : ScriptableObject, IWeightTable<CardRarityWeightRecord>
    {
        [field: OnValueChanged(nameof(CalculateProbabilities)), 
                TableList,
                SerializeField] public List<CardRarityWeightRecord> Records { get; private set; } = new();
        
        [Button("Refresh")]
        public void CalculateProbabilities()
        {
            var totalWeight = Records.Sum(probability => probability.Weight);
            foreach (var probability in Records)
            {
                probability.Probability = Percentage.FromFraction(probability.Weight / totalWeight);
            }
        }
    }
}