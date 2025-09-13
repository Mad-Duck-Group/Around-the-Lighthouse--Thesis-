using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.Fishing.Config.ThrowHook
{
    [CreateAssetMenu(fileName = "ThrowHookConfig", menuName = "Madduck/Fishing/ThrowHookConfig", order = 1)]
    public class ThrowHookConfig : ScriptableObject
    {
        [PropertyTooltip("Max throw hook value, throw distance is proportional to this value.")]
        [field: SerializeField] public float ThrowHookMaxValue { get; private set; } = 100f;
        [PropertyTooltip("Speed at which the throw hook slider moves.")]
        [field: SerializeField] public float ThrowHookSliderSpeed { get; private set; } = 50f;
        [PropertyTooltip("Range of the throw distance when the throw hook value is between 0 and max.")]
        [field: SerializeField] public Vector2 ThrowRange { get; private set; } = new(10f, 50f);
    }
}