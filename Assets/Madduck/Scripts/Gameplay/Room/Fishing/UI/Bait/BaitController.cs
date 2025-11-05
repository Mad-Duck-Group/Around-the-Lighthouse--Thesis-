using System;
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

namespace Madduck.Room
{
    public class BaitController : IDisposable ,IStartable
    {
        public ReactiveCommand<BaitItemInstance> OnBaitChanged { get; } = new();
        
        private readonly PlayerInputHandler _inputHandler;
        private readonly PlayerInventory _playerInventory;
        private readonly ISubscriber<FishingStateEvent> _fishingStateSubscriber;
        private readonly GameObject _uiBeforeTriggerBait;
        private readonly GameObject _uiAfterTriggerBait;
        private readonly CarouselController<LocationData> _carousel;
        
        private bool _interactable = true;

        private IDisposable _bindings;
        private readonly CompositeDisposable _confirmDisposables = new();


        [Inject]
        public BaitController(PlayerInputHandler inputHandler,
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
            DebugUtils.Log("test start");

        }
        private void Bind()
        {
            var builder = Disposable.CreateBuilder();
            _fishingStateSubscriber
                .Subscribe(OnFishingStateEvent)
                .AddTo(ref builder);
            _inputHandler.BaitButton.IsDown
                .Where(x => x)
                .Subscribe(_ => { ToggleBaitUI();})
                .AddTo(ref builder);
            _inputHandler.BaitSelectInput
                .Where(_ => _interactable)
                .ThrottleFirst(TimeSpan.FromMilliseconds(200))//block spam
                .Subscribe(value =>
            {
                if (value > 0 && _interactable)
                {
                    OnNextBait();
                    _carousel.Next();
                }
                else if (value < 0&& _interactable)
                {
                    OnPreviousBait();
                    _carousel.Previous();
                }
            })
                .AddTo(ref builder);
            _carousel.OnInitialized
                .Where(_ => _carousel.HasItems)
                .Subscribe(_ =>
                {
                    _confirmDisposables.Clear(); // กัน double bind

                    _inputHandler.ConfirmBaitButton.IsDown
                        .Where(x => x && _interactable && _uiAfterTriggerBait.activeSelf)
                        .Subscribe(__ => _carousel.Select())
                        .AddTo(_confirmDisposables);
                })
                .AddTo(ref builder);
            
             _carousel.OnCurrentItemUpdated
                 .Where(_ => _uiAfterTriggerBait.activeSelf)
                 .Subscribe(data =>
                 {
                     DebugUtils.Log($"Current bait: {data.sprite}");
                 })
                 .AddTo(ref builder);
            
             _carousel.OnItemSelected
                 .Where(_ => _interactable)
                 .Subscribe(data =>
                 {
                     DebugUtils.Log($"Selected bait from carousel: {data.sprite}");
                 })
                 .AddTo(ref builder);
             _bindings = builder.Build();
            DebugUtils.Log("test inject");
        }

        private void ToggleBaitUI()
        {
            if (_uiBeforeTriggerBait.activeSelf)
            {
                _uiBeforeTriggerBait.SetActive(false);
                _uiAfterTriggerBait.SetActive(true);
                DebugUtils.Log("CloseUI");
            }
            else
            {
                _uiAfterTriggerBait.SetActive(false);
                _uiBeforeTriggerBait.SetActive(true);
                DebugUtils.Log("OpenUI");
            }
        }
        private void OnFishingStateEvent(FishingStateEvent evt)
        {
            _interactable = evt.StateType is FishingStateType.ThrowHook;
        }

        private void OnCarouselItemUpdated(BaitItemInstance bait)
        {
            if (bait == null) return;
            _playerInventory.SetCurrentBait(bait.ItemData.BaitType);
            OnBaitChanged.Execute(bait);
        }
        private void OnNextBait()
        {
            var next = _playerInventory.GetNextBait();
            if (next != null)
            {
                OnCarouselItemUpdated(next);
                DebugUtils.Log($"Next bait: {next.ItemData.BaitType}");
            }
        }

        private void OnPreviousBait()
        {
            var prev = _playerInventory.GetPreviousBait();
            if (prev != null)
            {
                OnCarouselItemUpdated(prev);
                DebugUtils.Log($"Previous bait: {prev.ItemData.BaitType}");
            }
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
