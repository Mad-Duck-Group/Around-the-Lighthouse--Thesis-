using System;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public abstract class ItemInstance : IDisposable
    {
        [Title("Base References"), 
         HideLabel, 
         ShowInInspector] private InspectorPlaceholder _referencesTitle;
        
        [field: ReadOnly,
                ShowInInspector] public Guid InstanceGuid { get; protected set; }
        [field: ReadOnly,
                ShowInInspector] public uint CurrentCount { get; protected set; }
        
        public ReadOnlyReactiveProperty<uint> CurrentCountView { get; protected set; }
        
        public void ChangeCurrentCount(int change)
        {
            var currentCount = (int)CurrentCount;
            currentCount += change;
            // Prevent underflow by clamping to 0
            if (currentCount < 0) currentCount = 0;
            CurrentCount = (uint)currentCount;
        }

        public virtual void Dispose()
        {
            CurrentCountView?.Dispose();
        }
    }
    
    [Serializable]
    public abstract class ItemInstance<T> : ItemInstance
        where T : ItemData
    {
        [field: InlineEditor, 
                SerializeField] public T ItemData { get; private set; }
        
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