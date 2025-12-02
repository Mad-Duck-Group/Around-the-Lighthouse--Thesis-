using FMODUnity;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Shared
{
    [CreateAssetMenu(fileName = "BubbleManagerConfig", menuName = "Madduck/Fishing/Bubble/BubbleManagerConfig")]
    public class BubbleManagerConfig : ScriptableObject
    {
        [Title("Bubble Manager Config"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _title;
        [field: InlineProperty,
                SerializeField] public UFloat BubbleSpawnInterval { get; private set; }

        [field: InlineProperty,
                SerializeField] public UFloat BubbleStayDuration { get; private set; } = 10f;

        [field: SerializeField] public uint BubbleGuaranteeCount { get; private set; } = 1;
        [field: SerializeField] public uint BubbleMaxLimits { get; private set; } = 3;
        [field: SerializeField] public Vector2 BubbleSpawnRange { get; private set; }
        [field: InlineProperty, 
                SerializeField] public float BubbleYOffset { get; private set; }
        [field: SerializeField] public uint RangeSubdivision { get; private set; } = 2;
        
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference BubbleSfx { get; private set; }
    }
}