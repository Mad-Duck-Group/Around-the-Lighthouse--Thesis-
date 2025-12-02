using FMODUnity;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "TugOfWarConfig", menuName = "Madduck/Fishing/TugOfWarConfig", order = 5)]
    public class TugOfWarConfig : ScriptableObject
    {
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference FishAngrySfx { get; private set; }
        
        [Title("Input Instructions"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _inputInstructionsTitle;
        [field: SerializeField] public InputInstruction[] TugInputInstructions { get; private set; } = {
            new()
            {
                key = "ABXY",
                description = "Tug (Spam)"
            }
        };
    }
}