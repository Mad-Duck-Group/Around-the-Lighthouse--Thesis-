using FMODUnity;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "ReelingConfig", menuName = "Madduck/Fishing/ReelingConfig", order = 4)]
    public class ReelingConfig : ScriptableObject
    {
            [Title("Reeling"),
             HideLabel,
             ShowInInspector] private InspectorPlaceholder _reelingTitle;
        [PropertyTooltip("Max reeling value, when reached the player wins the reeling.")]
        [field: InlineProperty,
                SerializeField] public UFloat MaxReelingValue { get; private set; } = 100f;
        [field: InlineProperty,
                SerializeField] public UFloat RotationThreshold { get; private set; } = 60f;
        [field: InlineProperty,
                SerializeField] public UFloat RotationIdleThreshold { get; private set; } = 5f;
        [field: InlineProperty,
                SerializeField] public UFloat GamepadSensitivity { get; private set; } = 5;
        [field: InlineProperty,
                SerializeField] public UFloat MouseSensitivity { get; private set; } = 1;
        
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference ReelingSfx { get; private set; }
    }
}