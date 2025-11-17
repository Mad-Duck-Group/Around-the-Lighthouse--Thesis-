using System;
using Madduck.Shared;
using Madduck.Utils;
using UnityEngine;

namespace Madduck.Room
{
    [Serializable]
    public class PointingBaitConfig 
    {
        public SerializableDictionary<SelectionIcon, Sprite> pointintLeftBaitIconSprites;
        public SerializableDictionary<SelectionIcon, Sprite> pointintRightBaitIconSprites;
    }
}
