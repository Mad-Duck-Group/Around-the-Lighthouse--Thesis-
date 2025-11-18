using System;
using Madduck.Shared;
using Madduck.Utils;
using UnityEngine;

namespace Madduck.Fishing
{
    [Serializable]
    public class TugOfWarUIIconConfig 
    {
        public SerializableDictionary<SelectionIcon, Sprite> keyBoardMouseTugOfWarIconSprites;
        public SerializableDictionary<SelectionIcon, Sprite> gamePadTugOfWarIconSprites;
        public Sprite GetIcon(SelectionIcon icon, bool isGamepad)
        {
            return isGamepad
                ? gamePadTugOfWarIconSprites[icon]
                : keyBoardMouseTugOfWarIconSprites[icon];
        }
    }
   
}
