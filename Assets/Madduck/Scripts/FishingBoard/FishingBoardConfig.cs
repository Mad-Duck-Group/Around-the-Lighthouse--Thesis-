using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.FishingBoard
{
    [CreateAssetMenu(fileName = "FishingBoardConfig", menuName = "Madduck/FishingBoard/Config")]
    public class FishingBoardConfig : ScriptableObject
    {
        [Title("Settings")]
        [field: SerializeField] public float MouseSensitivity { get; private set; } = 1f;
        [field: SerializeField] public float Inertia { get; private set; } = 10f;
        
        [field: SerializeField] public float MaxFatigueLevel { get; private set; } = 100;
        
        [Title("Audio")]
        [field: SerializeField] public EventReference FishingLineTensionSfx { get; private set; }
    }
}