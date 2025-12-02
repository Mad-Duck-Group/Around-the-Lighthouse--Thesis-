using FMODUnity;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    /// <summary>
    /// Configuration for the Fishing Board mini-game.
    /// </summary>
    [CreateAssetMenu(fileName = "FishingBoardConfig", menuName = "Madduck/Fishing/FishingBoardConfig", order = 3)]
    public class FishingBoardConfig : ScriptableObject
    {
        [Title("Settings")]
        [PropertyTooltip("Max fatigue level, when reached the fish is caught.")]
        [field: InlineProperty,
                SerializeField] public UFloat MaxFatigueLevel { get; private set; } = 100;
        [field: InlineProperty,
                SerializeField] public UFloat MinimumMovingForce { get; private set; } = 250f;
        [field: InlineProperty,
                SerializeField] public Vector2 InertiaRange { get; private set; } = new(100f, 1000f);
        [field: InlineProperty,
                SerializeField] public AnimationCurve InertiaCurve { get; private set; } = AnimationCurve.Linear(0, 0, 1, 1);
        [field: InlineProperty,
                SerializeField] public bool EnableIdleDecayProcessor { get; private set; }
        [field: InlineProperty, ShowIf(nameof(EnableIdleDecayProcessor)),
                SerializeField] public UFloat IdleMagnitudeThreshold { get; private set; } = 0.3f;
        [field: InlineProperty, ShowIf(nameof(EnableIdleDecayProcessor)),
                SerializeField] public UFloat MaxIdleTime { get; private set; } = 2f;
        [field: InlineProperty, ShowIf(nameof(EnableIdleDecayProcessor)),
                SerializeField] public AnimationCurve IdleDecayCurve { get; private set; } = AnimationCurve.Linear(0, 1, 1, 0);
        
        [Title("Audio")]
        [PropertyTooltip("Sound effect played when there is tension on the fishing line.")]
        [field: SerializeField] public EventReference FishingLineTensionSfx { get; private set; }
        
        [Title("Input Instructions"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _inputInstructionsTitle;
        [field: SerializeField] public InputInstruction[] MoveHookInputInstructions { get; private set; } = 
        {
            new()
            {
                key = "Analog Left",
                description = "Move Hook"
            }
        };
    }
}