using MadDuck.Scripts.Utils.Inspectors;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

namespace Madduck.Scripts.Items.Data
{
    [CreateAssetMenu(fileName = "New Fish Behavior Data", menuName = "Madduck/Fish/Fish Behavior Data")]
    public class FishBehaviorData : ScriptableObject
    {
        [Title("References"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _referenceTitle;
        [field: Required, 
                SerializeField] public BehaviorGraph BehaviorGraph { get; private set; }
        
        [Title("Nibble Settings"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _nibbleSettingsTitle;
        [field: SerializeField] public int MaxNibbleAttempts { get; private set; } = 3;
        [field: SerializeField] public Vector2 NibbleIntervalRange { get; private set; } = new(5f, 15f);
        [field: SerializeField] public Vector2 NibbleTimeFrameRange { get; private set; } = new(1f, 3f);
        
        [Title("Fishing Board Settings"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _fishingBoardSettingsTitle;
        [field: SerializeField] public float Power { get; private set; } = 1f;
        [field: SerializeField] public float Resistance { get; private set; } = 1f;

        [Title("Reeling Settings"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _reelingSettingsTitle;
        [field: SerializeField] public float FishWeight { get; private set; }
        [field: SerializeField] public float FatigueDuration { get; private set; } = 10f;
        [field: SerializeField] public int MaxFatigueAttempts { get; private set; } = -1;
    }
}