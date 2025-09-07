using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.FishingBoard
{
    /// <summary>
    /// Configuration for the Fishing Board mini-game.
    /// </summary>
    [CreateAssetMenu(fileName = "FishingBoardConfig", menuName = "Madduck/FishingBoard/Config")]
    public class FishingBoardConfig : ScriptableObject
    {
        [Title("Settings")]
        [PropertyTooltip("How fast the hook moves in response to mouse movement.")]
        [field: SerializeField] public float MouseSensitivity { get; private set; } = 1f;
        [PropertyTooltip("How quickly the hook returns to the center when there is a tension.")]
        [field: SerializeField] public float Inertia { get; private set; } = 10f;
        [PropertyTooltip("Max fatigue level, when reached the fish is caught.")]
        [field: SerializeField] public float MaxFatigueLevel { get; private set; } = 100;
        
        [Title("Audio")]
        [PropertyTooltip("Sound effect played when there is tension on the fishing line.")]
        [field: SerializeField] public EventReference FishingLineTensionSfx { get; private set; }
    }
}