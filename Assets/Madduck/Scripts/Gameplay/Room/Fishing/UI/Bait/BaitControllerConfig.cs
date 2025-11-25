using FMODUnity;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Room
{
    [CreateAssetMenu(fileName = "BaitControllerConfig", menuName = "Madduck/Room/BaitControllerConfig")]
    public class BaitControllerConfig : ScriptableObject
    {
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference CycleBaitSfx { get; private set; }
        
        [Title("Input Instructions"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _inputInstructionsTitle;
        [field: SerializeField] public InputInstruction[] BaitInputInstructions { get; private set; } = {
            new()
            {
                key = "LeftRight",
                description = "Cycle Bait"
            },
            new()
            {
                key = "X",
                description = "Confirm Bait"
            }
        };
    }
}