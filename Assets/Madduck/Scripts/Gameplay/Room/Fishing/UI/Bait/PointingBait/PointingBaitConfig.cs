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
        public float iconSwitchDelay = 0.1f;
        
        public Sprite GetRightSelectedIcon(SelectionIcon icon)
        {
            return pointintRightBaitIconSprites[icon];
        }
        public Sprite GetLeftSelectedIcon(SelectionIcon icon)
        {
            return pointintLeftBaitIconSprites[icon];
        }
    }
    
    
}
