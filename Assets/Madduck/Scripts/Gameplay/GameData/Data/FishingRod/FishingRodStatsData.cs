using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Fishing Rod Stats", menuName = "Madduck/Fishing Rod/Fishing Rod Stats")]
    public class FishingRodStatsData : ScriptableObject
    {
        [Title("Fishing Rod Settings"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _fishingRodSettingsTitle;
        [field: SerializeField] public float Power { get; private set; } = 1f;
        [field: SerializeField] public float Resistance { get; private set; } = 1f;
        [field: SerializeField] public float FishingLineDurability { get; private set; } = 2f;
        [field: SerializeField] public float FishingLineRegenFactor { get; private set; } = 10f;
        [field: SerializeField] public float ReelingSpeed { get; private set; } = 2f;
    }
}
