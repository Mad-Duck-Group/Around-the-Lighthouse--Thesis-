using System.Collections.Generic;
using FMODUnity;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "Nibble Config", menuName = "Madduck/Fishing/Nibble Config", order = 2)]
    public class NibbleConfig : ScriptableObject
    {
        [Title("Qte"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _qteTitle;
        [field: SerializeField] public Vector2 QteIntervalRange { get; private set; } = new(3, 8);
        
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference ReelingSfx { get; private set; }
        [field: SerializeField] public EventReference PullHookSfx { get; private set; }
        [field: SerializeField] public EventReference FishBiteSfx { get; private set; }
        [field: SerializeField] public EventReference FishEmergedSfx { get; private set; }
        
        [Title("Input Instructions"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _inputInstructionsTitle;
        [field: SerializeField] public InputInstruction[] CancelInputInstructions { get; private set; } = {
            new()
            {
                key = "B",
                description = "Cancel",
            },
        };
        [field: SerializeField] public InputInstruction[] QteInputInstructions { get; private set; } = {
            new()
            {
                key = "ABXY",
                description = "QTE",
            },
        };
        [field: SerializeField] public InputInstruction[] CatchInputInstructions { get; private set; } = {
            new()
            {
                key = "A",
                description = "Catch",
            },
        };
        
        [Title("Debug"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugTitle;

        [SerializeField] private bool spoofNibbleChance;
        public bool SpoofNibbleChance
        {
            get
            {
#if UNITY_EDITOR
                return spoofNibbleChance;
#else
                return false;
#endif
            }
        }
        [ShowIf(nameof(spoofNibbleChance)), 
         SerializeField] private SerializableDictionary<uint, Percentage> spoofNibbleChances = new();
        public IReadOnlyDictionary<uint, Percentage> SpoofNibbleChances => (Dictionary<uint, Percentage>)spoofNibbleChances;
    }
}