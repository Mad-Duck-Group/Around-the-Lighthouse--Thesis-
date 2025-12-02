using System.Collections.Generic;
using FMODUnity;
using Madduck.Day;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Room
{
    [CreateAssetMenu(fileName = "RoomTrackConfig", menuName = "Madduck/Room/Room Track Config")]
    public class RoomTrackConfig : ScriptableObject
    {
        [Title("References"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _reference;
        [SerializeField] private SerializableDictionary<DayRoomKey, DayRoomSprite> futureSprites;
        public IReadOnlyDictionary<DayRoomKey, DayRoomSprite> FutureSprites => (Dictionary<DayRoomKey, DayRoomSprite>)futureSprites;
        [SerializeField] private SerializableDictionary<DayRoomKey, DayRoomSprite> pastSprites;
        public IReadOnlyDictionary<DayRoomKey, DayRoomSprite> PastSprites => (Dictionary<DayRoomKey, DayRoomSprite>)pastSprites;
        
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audio;
        [field: SerializeField] public EventReference BoatEngineSound { get; private set; }
    }
}