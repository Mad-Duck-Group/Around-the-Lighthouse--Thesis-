using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.Fishing.Config.Reeling
{
    [CreateAssetMenu(fileName = "ReelingConfig", menuName = "Madduck/Fishing/ReelingConfig", order = 4)]
    public class ReelingConfig : ScriptableObject
    {
        [PropertyTooltip("Max reeling value, when reached the player wins the reeling.")]
        [field: SerializeField] public float MaxReelingValue { get; private set; } = 100f;
    }
}