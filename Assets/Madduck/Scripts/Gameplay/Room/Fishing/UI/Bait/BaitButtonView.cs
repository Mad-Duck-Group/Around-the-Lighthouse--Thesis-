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
        ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        #region Inspector

        [Title("References")]
        [Required, 
         SerializeField] private Image icon;
        [Required, 
         SerializeField] private TMP_Text amount;
        [Required,
            SerializeField] private RectTransform baitViewTransform;
        [Required,
         OdinSerialize] private GeneralTooltipManager tooltipManager;

        #endregion

        #region Fields

        private BaitItemInstance _bait;
        private BaitSelectionViewModel _viewModel;
        private IDisposable _bindings;
        private CancellationTokenSource _tooltipCts = new();

        #endregion

        #region Injection

        public void SetUp(
            Canvas tooltipCanvas,
            Transform tooltipParent)
        {
            tooltipManager.TooltipCanvas = tooltipCanvas;
            tooltipManager.TooltipParent = tooltipParent;
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
            
        }

        #endregion

        #region Bindings

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _bait.CurrentCountView
                .Subscribe(OnBaitAmountChanged)
                .AddTo(ref disposableBuilder);
            _viewModel.CurrentBaitView
                .Subscribe(OnBaitChanged)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }

        #endregion

        #region Events

        private void OnBaitButtonClicked(BaitType baitType)
        {
            _viewModel.SetCurrentBaitCommand.Execute(baitType);
        }
        
        private void OnBaitAmountChanged(uint count)
        {
            amount.text = count.ToString();
            
        }

        private void OnBaitChanged(BaitItemInstance newInstance)
        {
            var selected = false;
            if (newInstance is not null)
            {
                selected = newInstance.ItemData.BaitType == _bait.ItemData.BaitType;
            }
            
        }
        
        #endregion
        
        
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