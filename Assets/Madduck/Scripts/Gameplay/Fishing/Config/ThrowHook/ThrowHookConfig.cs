using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "ThrowHookConfig", menuName = "Madduck/Fishing/ThrowHookConfig", order = 1)]
    public class ThrowHookConfig : ScriptableObject
    {
        [PropertyTooltip("Max throw hook value, throw distance is proportional to this value.")]
        [field: InlineProperty,
            SerializeField] public UFloat ThrowHookMaxValue { get; private set; } = 100f;
        [PropertyTooltip("Speed at which the throw hook slider moves.")]
        [field: InlineProperty,
                SerializeField] public UFloat ThrowHookSliderSpeed { get; private set; } = 50f;
    }
}