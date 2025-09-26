using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room
{
    [ShowOdinSerializedPropertiesInInspector]
    public class CardView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("References")]
        [Required, 
         SerializeField] private Image icon;
        [Required, 
         OdinSerialize] private GeneralTooltipManager tooltipManager;

        private CardItemInstance _card;
        private CancellationTokenSource _tooltipCts = new();
        
        public void SetUp(Canvas tooltipCanvas)
        {
            tooltipManager.TooltipCanvas = tooltipCanvas;
        }

        public void SetCard(CardItemInstance card)
        {
            _card = card;
            icon.sprite = card.ItemData.CardIcon;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _tooltipCts.Cancel();
            _tooltipCts = new();
            var tooltipObject = new GeneralTooltipObject(
                _card.ItemData.CardName, 
                _card.ItemData.CardDescription);
            tooltipManager.ShowTooltip(tooltipObject, _tooltipCts.Token).Forget();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipCts.Cancel();
            _tooltipCts = new();
            tooltipManager.HideTooltip(_tooltipCts.Token).Forget();
        }
        
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