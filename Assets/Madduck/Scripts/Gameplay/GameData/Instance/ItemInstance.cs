using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public abstract record ItemInstance<T>(T ItemData)
        where T : ItemData
    {
        [Title("Base References"), 
         HideLabel, 
         ShowInInspector] private InspectorPlaceholder _referencesTitle;
        [field: InlineEditor, 
                SerializeField] public T ItemData { get; private set; } = ItemData;
    }
}