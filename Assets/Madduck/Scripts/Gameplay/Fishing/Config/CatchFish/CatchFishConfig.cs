using FMODUnity;
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
    }
}