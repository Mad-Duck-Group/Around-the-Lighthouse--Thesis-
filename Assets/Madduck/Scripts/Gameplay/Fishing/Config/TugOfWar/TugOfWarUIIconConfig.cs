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
        public float iconSwitchDelay = 0.1f;
        public Sprite GetIcon(bool isGamepad)
        {
            return isGamepad
                ? gamePadTugOfWarIconSprites[SelectionIcon.Unselected]
                : keyBoardMouseTugOfWarIconSprites[SelectionIcon.Unselected];
        }
        public Sprite GetSelectedIcon(SelectionIcon icon, bool isGamepad)
        {
            return isGamepad
                ? gamePadTugOfWarIconSprites[icon]
                : keyBoardMouseTugOfWarIconSprites[icon];
        }

        
    }
   
}
