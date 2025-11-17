using Madduck.Day;
using Madduck.Utils;
using UnityEngine;

namespace Madduck.Room
{
    public class DayRoomSpriteConfig 
    {
        public SerializableDictionary<DayRoomKey, Sprite> FutureSprites;
        public SerializableDictionary<DayRoomKey, Sprite> PastSprites;
    }
}
