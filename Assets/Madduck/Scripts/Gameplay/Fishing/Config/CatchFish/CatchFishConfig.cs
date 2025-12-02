using FMODUnity;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "CatchFishConfig", menuName = "Madduck/Fishing/CatchFishConfig")]
    public class CatchFishConfig : ScriptableObject
    {
        [Title("Catch Fish"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _catchFishTitle;
        [field: SerializeField] public TweenSettings<float> SlowMoSettings { get; private set; }
        [field: InlineProperty, 
                SerializeField] public Percentage SlowMoThreshold { get; private set; } = Percentage.FromFraction(0.5f);
        
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference PullHookUpSfx { get; private set; }
        [field: SerializeField] public EventReference FishFlopSfx { get; private set; }
        
        [Title("Input Instructions"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _inputInstructionsTitle;
        [field: SerializeField] public InputInstruction[] QteInputInstructions { get; private set; } = {
            new()
            {
                key = "ABXY",
                description = "QTE"
            }
        };
    }
}