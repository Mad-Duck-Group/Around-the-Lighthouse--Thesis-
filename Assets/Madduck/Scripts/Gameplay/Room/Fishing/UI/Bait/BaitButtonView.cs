using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.GameData.Bait;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Madduck.Room
{
    [ShowOdinSerializedPropertiesInInspector]
    public class BaitButtonView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amount;
        [OdinSerialize] private GeneralTooltipManager tooltipManager;
        
        private BaitItemInstance _bait;
        private BaitSelectionViewModel _viewModel;
        private IDisposable _bindings;
        private CancellationTokenSource _tooltipCts = new();
        
        public void SetUp(
            Canvas tooltipCanvas)
        {
            tooltipManager.TooltipCanvas = tooltipCanvas;
        }

        public void SetBait(
            BaitSelectionViewModel viewModel,
            BaitItemInstance bait)
        {
            _viewModel = viewModel;
            _bait = bait;
            icon.sprite = bait.ItemData.BaitIcon;
            Bind();
            OnBaitAmountChanged(_bait.CurrentCount);
            SetInteractable(true);
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            button
                .OnClickAsObservable()
                .Subscribe(_ => OnBaitButtonClicked(_bait.ItemData.BaitType))
                .AddTo(ref disposableBuilder);
            _bait.CurrentCountView
                .Subscribe(OnBaitAmountChanged)
                .AddTo(ref disposableBuilder);
            _viewModel.CurrentBaitView
                .Subscribe(OnBaitChanged)
                .AddTo(ref disposableBuilder);
            _viewModel.InteractableView
                .Subscribe(SetInteractable)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }

        private void OnBaitButtonClicked(BaitType baitType)
        {
            _viewModel.SetCurrentBaitCommand.Execute(baitType);
        }
        
        private void OnBaitAmountChanged(uint count)
        {
            amount.text = count.ToString();
            if (count == 0) SetSelected(false);
        }

        private void OnBaitChanged(BaitItemInstance newInstance)
        {
            var selected = false;
            if (newInstance is not null)
            {
                selected = newInstance.ItemData.BaitType == _bait.ItemData.BaitType;
            }
            SetSelected(selected);
        }

        private void SetInteractable(bool interactable)
        {
            button.interactable = interactable && _bait.CurrentCount > 0;
        }

        private void SetSelected(bool selected)
        {
            icon.color = selected ? Color.red : Color.white;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _tooltipCts.Cancel();
            _tooltipCts = new();
            var tooltipObject = new GeneralTooltipObject(
                _bait.ItemData.BaitName, 
                _bait.ItemData.BaitDescription);
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

    [Serializable]
    public class BaitButtonViewFactory : IGenericFactory<BaitButtonView>
    {
        [SerializeField] private Transform baitButtonsParent;
        [SerializeField] private Canvas tooltipCanvas;
        [SerializeField] private BaitButtonView baitButtonViewPrefab;
        
        public BaitButtonView Current { get; private set; }
        public BaitButtonView Create()
        {
            Current = UnityEngine.Object.Instantiate(baitButtonViewPrefab, baitButtonsParent);
            Current.SetUp(tooltipCanvas);
            return Current;
        }
    }
}