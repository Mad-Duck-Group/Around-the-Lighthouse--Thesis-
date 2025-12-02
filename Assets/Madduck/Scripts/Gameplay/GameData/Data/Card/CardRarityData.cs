using System.Collections.Generic;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "CardRarityData", menuName = "Madduck/Card/CardRarityData", order = 3)]
    [ShowOdinSerializedPropertiesInInspector]
    public class CardRarityData : ScriptableObject, IHasModifier, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("Card Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _cardSettingsTitle;
        [field: SerializeField] public string CardName { get; private set; }
        [field: TextArea(3, 20),
                SerializeField] public string CardDescription { get; private set; }
        [field: PreviewField,
                SerializeField] public Sprite CardIcon { get; private set; }
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