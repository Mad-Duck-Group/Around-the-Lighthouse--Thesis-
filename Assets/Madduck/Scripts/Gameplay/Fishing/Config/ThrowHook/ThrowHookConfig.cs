using FMODUnity;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "ThrowHookConfig", menuName = "Madduck/Fishing/ThrowHookConfig", order = 1)]
    public class ThrowHookConfig : ScriptableObject
    {
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference ThrowHookSfx { get; private set; }
        [field: SerializeField] public EventReference FishingLineCastSfx { get; private set; }
        [field: SerializeField] public EventReference HookHitWaterSfx { get; private set; }
        
        [Title("Input Instructions"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _inputInstructionsTitle;
        [field: SerializeField] public InputInstruction[] ThrowHookInputInstructions { get; private set; } = {
            new()
            {
                key = "A",
                description = "Throw (Hold)",
            },
        };
    }
}