using System;
using System.Collections.Generic;
using System.Linq;
using MadDuck.Scripts.Items.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.Items
{
    [Serializable]
    public record FishProbability
    {
        [Required] public FishItemData fishData;
        public float weight = 1;
        [ReadOnly, DisplayAsString] public float probability;
    }
    
    [CreateAssetMenu(fileName = "New Fish Probability", menuName = "Fish/Fish Probability")]
    public class FishProbabilityData : ScriptableObject
    {
        [OnValueChanged(nameof(CalculateProbabilities))]
        [TableList]
        public List<FishProbability> fishProbabilities = new();
        
        [Button("Refresh")]
        public void CalculateProbabilities()
        {
            float totalWeight = fishProbabilities.Sum(fishProbability => fishProbability.weight);
            foreach (var fishProbability in fishProbabilities)
            {
                fishProbability.probability = fishProbability.weight / totalWeight;
            }
        }
    }
}
