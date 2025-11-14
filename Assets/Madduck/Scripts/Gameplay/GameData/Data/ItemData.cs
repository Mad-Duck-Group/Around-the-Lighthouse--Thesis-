using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.GameData
{
    public enum ItemType
    {
        Fish,
        FishingRod,
        Fisherman,
        Card,
        Bait,
        Resource,
    }
    [ShowOdinSerializedPropertiesInInspector]
    public abstract class ItemData : ScriptableObject, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("Base Settings"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _baseSettingsTitle;
        [field: OdinSerialize] public Guid Guid { get; private set; } = Guid.NewGuid();
        [field: SerializeField] public ItemType ItemType { get; private set; }
        // [field: SerializeField] public bool IsStackable { get; private set; }
        // [field: ShowIf(nameof(IsStackable)), 
        //         SerializeField] public uint MaxStackSize { get; private set; } = 2;
        
        #region Serialization
        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData 
        { 
            get => serializationData;
            set => serializationData = value;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
        }
        #endregion
    }
}
