using System.Collections.Generic;
using FMODUnity;
using Madduck.Audio;
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
        //[field: SerializeField] public EventReference BoatSfx { get; private set; }
        [field: SerializeField] public EventReference SeaAmbient { get; private set; }
        [field: InlineProperty, 
                SerializeField] public Percentage BgmChance { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat BgmDelay { get; private set; }
        [field: SerializeField] public List<EventReference> BgmPlaylist { get; private set; } = new();
        [field: SerializeField] public EventReference BossBgm { get; private set; }
    }
}