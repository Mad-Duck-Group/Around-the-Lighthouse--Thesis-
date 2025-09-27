using System;
using Madduck.GameData;
using Madduck.GameData.Bait;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class BaitButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amount;
        
        private BaitItemInstance _bait;
        private BaitSelectionViewModel _viewModel;
        private IDisposable _bindings;

        public void SetUp(
            BaitSelectionViewModel viewModel,
            BaitItemInstance bait)
        {
            _bait = bait;
            _viewModel = viewModel;
            icon.sprite = bait.ItemData.BaitIcon;
            Bind();
            OnBaitAmountChanged(_bait.CurrentCount);
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
                .Subscribe(x =>
                {
                    var selected = false;
                    if (x is not null)
                    {
                        selected = x.ItemData.BaitType == _bait.ItemData.BaitType;
                    }
                    SetSelected(selected);
                })
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }

        private void OnBaitButtonClicked(BaitType baitType)
        {
            var selected = false;
            var currentBait = _viewModel.CurrentBaitView.CurrentValue;
            if (currentBait is not null)
            {
                var currentType = currentBait.ItemData.BaitType;
                var thisBait = _bait.ItemData.BaitType;
                selected = currentType == thisBait;
            }
            _viewModel.SetCurrentBaitCommand.Execute(selected ? BaitType.None : baitType);
        }
        
        private void OnBaitAmountChanged(uint count)
        {
            amount.text = count.ToString();
            var isActive = count > 0;
            SetInteractable(isActive);
        }

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
            icon.color = interactable ? Color.white : Color.grey;
        }

        public void SetSelected(bool selected)
        {
            icon.color = selected ? Color.red : Color.white;
        }
    }
}