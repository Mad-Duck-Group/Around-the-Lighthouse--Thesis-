using UnityEngine;

namespace Madduck.Scripts.Fishing.Config.ThrowHook
{
    [CreateAssetMenu(fileName = "ThrowHookConfig", menuName = "Madduck/Fishing/ThrowHookConfig", order = 0)]
    public class ThrowHookConfig : ScriptableObject
    {
        [field: SerializeField] public float ThrowHookMaxValue { get; private set; } = 100f;
        [field: SerializeField] public float ThrowHookSliderSpeed { get; private set; } = 50f;
        [field: SerializeField] public Vector2 ThrowRange { get; private set; } = new(10f, 50f);
    }
}