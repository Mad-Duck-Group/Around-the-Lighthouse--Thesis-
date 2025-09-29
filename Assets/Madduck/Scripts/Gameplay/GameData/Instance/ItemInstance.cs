using System;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public abstract class ItemInstance<T>
        where T : ItemData
    {
        [Title("Base References"), 
         HideLabel, 
         ShowInInspector] private InspectorPlaceholder _referencesTitle;
        [field: InlineEditor, 
                SerializeField] public T ItemData { get; private set; }
        [field: ReadOnly,
            ShowInInspector] public Guid InstanceGuid { get; private set; }
        [field: ReadOnly,
                ShowInInspector] public uint CurrentCount { get; set; }
        
        public ReadOnlyReactiveProperty<uint> CurrentCountView { get; }

        protected ItemInstance(T itemData, uint count = 1)
        {
            ItemData = itemData;
            InstanceGuid = Guid.NewGuid();
            CurrentCount = count;
            CurrentCountView = Observable
                .EveryValueChanged(this, x => x.CurrentCount)
                .ToReadOnlyReactiveProperty();
        }
    }
}