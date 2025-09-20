using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "CardItemData", menuName = "Madduck/Card/CardItemData", order = 3)]
    [ShowOdinSerializedPropertiesInInspector]
    public class CardItemData : ItemData, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [field: OdinSerialize] public List<BaseModifierData> Modifiers { get; private set; } = new();

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