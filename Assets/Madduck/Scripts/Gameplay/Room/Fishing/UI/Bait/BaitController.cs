using System;
using System.Linq;
using HasanSadikin.Carousel;
using Madduck.GameData;
using Madduck.GameData.Bait;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using DisposableBag = R3.DisposableBag;

namespace Madduck.Room
{
    public class BaitController : IDisposable, IStartable
    {
        public ReactiveCommand<BaitItemInstance> OnBaitChanged { get; } = new();
        
        private readonly IPlayerInputHandler _inputHandler;
        private readonly PlayerInventory _playerInventory;
        private readonly ISubscriber<FishingStateEvent> _fishingStateSubscriber;
        private readonly GameObject _uiBeforeTriggerBait;
        private readonly GameObject _uiAfterTriggerBait;
        private readonly CarouselController<LocationData> _carousel;
        
        private BaitItemInstance _pendingBait;
        private bool _interactable = true;
        private IDisposable _bindings;
        private DisposableBag _confirmDisposables;

        [Inject]
        public BaitController(
            IPlayerInputHandler inputHandler,
            PlayerInventory playerInventory,
            ISubscriber<FishingStateEvent> fishingStateSubscriber,
            UIBeforeTriggerBait before,
            UIAfterTriggerBait after,
            CarouselController<LocationData> carousel)
        {
            _inputHandler = inputHandler;
            _playerInventory = playerInventory;
            _fishingStateSubscriber = fishingStateSubscriber;
            _uiBeforeTriggerBait = before.Value;
            _uiAfterTriggerBait = after.Value;
            _carousel = carousel;
        }
        
        public void Start()
        {
            Bind();
        }
        
        private void Bind()
        {
            var builder = Disposable.CreateBuilder();
            _fishingStateSubscriber
                .Subscribe(OnFishingStateEvent)
                .AddTo(ref builder);
            _inputHandler.BaitButton.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .Where(x => x)
                .Subscribe(_ => { SetActive(true);})
                .AddTo(ref builder);
            _inputHandler.BaitButton.IsUpAfterHeld
                .IgnoreFirstValueWhenSubscribe()
                .Where(x => x)
                .Subscribe(_ => { SetActive(false);})
                .AddTo(ref builder);
            _inputHandler.BaitSelectInput
                .IgnoreFirstValueWhenSubscribe()
                .ThrottleFirst(TimeSpan.FromMilliseconds(100))//block spam
                .Where(_ => _interactable)
                .Subscribe(value =>
                {
                    switch (value)
                    {
                        case > 0:
                            OnNextBait();
                            _carousel.Next();
                            break;
                        case < 0:
                            OnPreviousBait();
                            _carousel.Previous();
                            break;
                    }
                })
                .AddTo(ref builder);
            _carousel.OnInitialized
                .Where(_ => _carousel.HasItems)
                .Subscribe(_ =>
                {
                    _confirmDisposables.Dispose();
                    _confirmDisposables.Clear(); // กัน double bind
                    _confirmDisposables = new DisposableBag();

                    _inputHandler.ConfirmBaitButton.IsDown
                        .Where(x => x && _interactable && _uiAfterTriggerBait.activeSelf)
                        .Subscribe(__ =>
                        {
                            _pendingBait ??= _playerInventory.CurrentBaitsView.FirstOrDefault().Value;
                            _playerInventory.SetCurrentBait(_pendingBait.ItemData.BaitType);
                            OnBaitChanged.Execute(_pendingBait);
                        })
                        .AddTo(ref _confirmDisposables);
                })
                .AddTo(ref builder);
            
             // _carousel.OnCurrentItemUpdated
             //     .Where(_ => _uiAfterTriggerBait.activeSelf)
             //     .Subscribe(data =>
             //     {
             //         //
             //     })
             //     .AddTo(ref builder);
             //
             // _carousel.OnItemSelected
             //     .Where(_ => _interactable)
             //     .Subscribe(data =>
             //     {
             //         // DebugUtils.Log($"Selected bait from carousel: {data.sprite}");
             //     })
             //     .AddTo(ref builder);
             _bindings = builder.Build();
        }

        private void SetActive(bool active)
        {
            _uiBeforeTriggerBait.SetActive(!active);
            _uiAfterTriggerBait.SetActive(active);
        }
        private void OnFishingStateEvent(FishingStateEvent evt)
        {
            _interactable = evt.StateType is FishingStateType.ThrowHook;
        }

        private void OnCarouselItemUpdated(BaitItemInstance bait)
        {
            if (bait == null) return;
            _playerInventory.SetCurrentBait(bait.ItemData.BaitType);//เปลี่ยนใหม่
            OnBaitChanged.Execute(bait);
        }
        private void OnNextBait()
        {
            var baseBait = _pendingBait ?? _playerInventory.CurrentBaitView.CurrentValue;
            var baitList = _playerInventory.CurrentBaitsView.Select(x => x.Value).ToList();

            if (baitList.Count == 0) return;

            int currentIndex = baseBait == null ? -1 : baitList.IndexOf(baseBait);
            int nextIndex = (currentIndex + 1) % baitList.Count;

            _pendingBait = baitList[nextIndex];
            DebugUtils.Log($"Pending Next bait: {_pendingBait.ItemData.BaitType}");

        }

        private void OnPreviousBait()
        {
            var baseBait = _pendingBait ?? _playerInventory.CurrentBaitView.CurrentValue;
            var baitList = _playerInventory.CurrentBaitsView.Select(x => x.Value).ToList();

            if (baitList.Count == 0) return;

            int currentIndex = baseBait == null ? 0 : baitList.IndexOf(baseBait);
            int prevIndex = (currentIndex - 1 + baitList.Count) % baitList.Count;

            _pendingBait = baitList[prevIndex];
            DebugUtils.Log($"Pending Previous bait: {_pendingBait.ItemData.BaitType}");

        }
        private void OnCarouselItemSelected(BaitItemInstance bait)
        {
            if (!_interactable || bait == null) return;
            _playerInventory.SetCurrentBait(bait.ItemData.BaitType);
            DebugUtils.Log($"Selected bait: {bait.ItemData.BaitType}");
        }

        public void Dispose()
        {
            _bindings?.Dispose();
        }
    }
    public class UIBeforeTriggerBait
    {
        public GameObject Value { get; }
        public UIBeforeTriggerBait(GameObject value) => Value = value;
    }

    public class UIAfterTriggerBait
    {
        public GameObject Value { get; }
        public UIAfterTriggerBait(GameObject value) => Value = value;
    }
}
