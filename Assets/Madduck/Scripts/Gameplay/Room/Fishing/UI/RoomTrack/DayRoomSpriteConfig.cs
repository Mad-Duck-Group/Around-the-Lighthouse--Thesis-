using Madduck.Day;
using Madduck.Utils;
using UnityEngine;

namespace Madduck.Room
{
    public class DayRoomSpriteConfig 
    {
        public SerializableDictionary<DayRoomKey, DayRoomSprite> FutureSprites;
        public SerializableDictionary<DayRoomKey, DayRoomSprite> PastSprites;
    }
}
