using System;
using System.Collections.Generic;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(menuName = "Madduck/Input/IconDatabase")]
    public class InputIconData : ScriptableObject
    {
        public SerializableDictionary<InputIconType, InputIcon> iconMap;
    }
    [Serializable]
    public class InputIcon
    {
        public Sprite keyboardSprite;
        public Sprite gamepadSprite;
        
        public bool useAnimation;
        [ShowIf("useAnimation")]
        public AnimationClip keyboardAnimation;
        [ShowIf("useAnimation")]
        public AnimationClip gamepadAnimation;
    }
}
