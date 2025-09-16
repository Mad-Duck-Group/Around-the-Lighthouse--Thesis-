using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public abstract class ItemInstance<T> where T : ItemData
    {
        [Title("Base References"), 
         HideLabel, 
         ShowInInspector] private InspectorPlaceholder _referencesTitle;
        [field: Required, 
                InlineEditor, 
                SerializeField] public T ItemData { get; private set; }

        protected ItemInstance(T itemData)
        {
            ItemData = itemData;
        }
    }
}