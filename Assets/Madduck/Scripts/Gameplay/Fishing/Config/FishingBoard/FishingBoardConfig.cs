using FMODUnity;
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
        [PropertyTooltip("How fast the hook moves in response to mouse movement.")]
        [field: InlineProperty,
                SerializeField] public UFloat MouseSensitivity { get; private set; } = 1f;
        [PropertyTooltip("Max fatigue level, when reached the fish is caught.")]
        [field: InlineProperty,
                SerializeField] public UFloat MaxFatigueLevel { get; private set; } = 100;
        
        [Title("Audio")]
        [PropertyTooltip("Sound effect played when there is tension on the fishing line.")]
        [field: SerializeField] public EventReference FishingLineTensionSfx { get; private set; }
    }
}