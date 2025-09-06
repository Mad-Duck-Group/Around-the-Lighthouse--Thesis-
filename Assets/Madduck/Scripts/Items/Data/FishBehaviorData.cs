using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

namespace Madduck.Scripts.Items.Data
{
    [CreateAssetMenu(fileName = "New Fish Behavior Data", menuName = "Fish/Fish Behavior")]
    public class FishBehaviorData : ScriptableObject
    {
        [Title("References")]
        [field: SerializeField] public BehaviorGraph BehaviorGraph { get; private set; }
        
        [Title("Nibble Settings")]
        [field: SerializeField] 
        public int MaxNibbleAttempts { get; private set; } = 3;
        [field: SerializeField]
        public Vector2 NibbleIntervalRange { get; private set; } = new(5f, 15f);
        [field: SerializeField] 
        public Vector2 NibbleTimeFrameRange { get; private set; } = new(1f, 3f);

        [Title("Reeling Settings")] 
        [field: SerializeField] 
        public float Power { get; private set; } = 1f;
        [field: SerializeField]
        public float FatigueDuration { get; private set; } = 10f;
        [field: SerializeField]
        public int MaxFatigueAttempts { get; private set; } = -1;
    }
}