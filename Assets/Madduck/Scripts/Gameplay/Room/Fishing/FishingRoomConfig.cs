using FMODUnity;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Room
{
    [CreateAssetMenu(fileName = "FishingRoomConfig", menuName = "Madduck/Room/FishingRoomConfig")]
    public class FishingRoomConfig : ScriptableObject
    {
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference FishingRoomBGM { get; private set; }
    }
}