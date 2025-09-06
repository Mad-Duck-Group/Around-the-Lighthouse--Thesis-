using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.FishingRods
{
    [CreateAssetMenu(fileName = "New Fishing Rod Stats", menuName = "Fishing Rod/Fishing Rod Stats")]
    public class FishingRodStatsData : ScriptableObject
    {
        [Title("Fishing Rod Settings")]
        [field: SerializeField, MinMaxSlider(0f, 50f, true)] 
        public Vector2 ThrowingDistanceRange { get; private set; } = new(1f, 5f);
        [field: SerializeField] 
        public float Power { get; private set; } = 2f;
        [field: SerializeField] 
        public float FishingLineDurability { get; private set; } = 2f;
        [field: SerializeField] 
        public float FishingLineRegenFactor { get; private set; } = 10f;
        [field: SerializeField] 
        public float FishingRodDurability { get; private set; } = 2f;
        [field: SerializeField] 
        public float ReelingSpeed { get; private set; } = 2f;
    }
}
