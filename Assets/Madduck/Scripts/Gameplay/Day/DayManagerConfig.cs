using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Day
{
    [CreateAssetMenu(fileName = "New Day Manager Config", menuName = "Madduck/Day/Day Manager Config")]
    public class DayManagerConfig : ScriptableObject
    {
        [field: SerializeField] public uint MaxDayCount { get; private set; } = 7;
        [field: SerializeField] public uint MaxRoomCount { get; private set; } = 6;
        [field: InlineProperty,
            SerializeField] public Percentage DayNightRatio { get; private set; } = Percentage.FromPercentage(50);
    }
}