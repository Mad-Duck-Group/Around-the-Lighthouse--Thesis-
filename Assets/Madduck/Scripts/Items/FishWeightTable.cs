using System;
using System.Collections.Generic;
using System.Linq;
using MadDuck.Scripts.Items.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.Items
{
    [CreateAssetMenu(fileName = "New Fish Probability", menuName = "Madduck/Fish/Fish Probability")]
    public class FishWeightTable : ScriptableObject
    {
        [field: OnValueChanged(nameof(CalculateProbabilities)), 
                TableList,
                SerializeField] private List<FishWeightRecord> Records { get; set; } = new();
        
        [Button("Refresh")]
        public void CalculateProbabilities()
        {
            var totalWeight = Records.Sum(fishProbability => fishProbability.Weight);
            foreach (var fishProbability in Records)
            {
                fishProbability.Probability = fishProbability.Weight / totalWeight;
            }
        }

        public FishWeightTableInstance GetInstance()
        {
            return new FishWeightTableInstance(new List<FishWeightRecord>(Records));
        }
    }
}
