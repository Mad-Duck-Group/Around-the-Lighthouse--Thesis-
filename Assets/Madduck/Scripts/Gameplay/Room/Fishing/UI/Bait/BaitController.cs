using System;
using System.Linq;
using HasanSadikin.Carousel;
using Madduck.Audio;
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
        private readonly IAudioManager _audioManager;
        private readonly PlayerInventory _playerInventory;
        private readonly ISubscriber<FishingStateEvent> _fishingStateSubscriber;
        private readonly BaitControllerConfig _baitControllerConfig;
        private readonly BaitUITriggerConfig _baitTriggerConfig;
        private readonly CarouselController _carousel;
        private readonly InputInstructionManager _inputInstructionManager;
        private readonly PointingBaitViewModel _pointingBaitViewModel;
        
        private BaitItemInstance _pendingBait;
        private bool _interactable = true;
        private bool _isActive = false;
        private IDisposable _bindings;
        private DisposableBag _confirmDisposables;

        [Inject]
        public BaitController(
            IPlayerInputHandler inputHandler,
            IAudioManager audioManager,
            PlayerInventory playerInventory,
            ISubscriber<FishingStateEvent> fishingStateSubscriber,
            BaitControllerConfig baitControllerConfig,
            BaitUITriggerConfig baitTriggerConfig,
            CarouselController carousel,
            InputInstructionManager inputInstructionManager,
            PointingBaitViewModel pointingBaitViewModel)
        {
            _inputHandler = inputHandler;
            _playerInventory = playerInventory;
            _audioManager = audioManager;
            _fishingStateSubscriber = fishingStateSubscriber;
            _baitControllerConfig = baitControllerConfig;
            _baitTriggerConfig = baitTriggerConfig;
            _carousel = carousel;
            _inputInstructionManager = inputInstructionManager;
            _pointingBaitViewModel = pointingBaitViewModel;
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
                .Subscribe(_ => { SetActive(!_isActive);})
                .AddTo(ref builder);
            // _inputHandler.BaitButton.IsUpAfterHeld
            //     .IgnoreFirstValueWhenSubscribe()
            //     .Where(x => x)
            //     .Subscribe(_ => { SetActive(false);})
            //     .AddTo(ref builder);
            _inputHandler.BaitSelectInput
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(_ => _interactable)
                .Subscribe(value =>
                {
                    _pointingBaitViewModel.UpdateInput(value);
                    if (value != 0) _audioManager.PlayAudioOneShot(_baitControllerConfig.CycleBaitSfx, Vector3.zero);
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
                });
            
            _carousel.OnInitialized
                .Where(_ => _carousel.HasItems)
                .Subscribe(_ =>
                {
                    _confirmDisposables.Dispose();
                    _confirmDisposables.Clear(); // กัน double bind
                    _confirmDisposables = new DisposableBag();

                    _inputHandler.ConfirmBaitButton.IsDown
                        .Where(x => x && _interactable && _baitTriggerConfig.after.activeSelf)
                        .Subscribe(__ =>
                        {
                            var bait = _carousel.GetCurrentBaitVisual();
                            _carousel.ToggleSelection(bait);
                            if (!_carousel.HasConfirmedItem)
                            {
                                _pendingBait = null;
                                _playerInventory.SetCurrentBait(BaitType.None);
                                OnBaitChanged.Execute(null);
                                return;
                            }
                            _pendingBait = bait;
                            _playerInventory.SetCurrentBait(bait.ItemData.BaitType);
                            OnBaitChanged.Execute(bait);
                            
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
            _isActive = active;
            _baitTriggerConfig.before.SetActive(!active);
            _baitTriggerConfig.after.SetActive(active);
            if (active && _interactable)
            {
                _inputInstructionManager.Show(new []
                {
                    new InputInstruction
                    {
                        key = "LeftRight",
                        description = "Cycle Bait"
                    },
                    new InputInstruction
                    {
                        key = "X",
                        description = "Confirm Bait"
                    }
                }, stream: 1);
            }
            else
            {
                _inputInstructionManager.RemoveStream(1);
            }
        }
        private void OnFishingStateEvent(FishingStateEvent evt)
        {
            _interactable = evt.StateType is FishingStateType.ThrowHook;
            switch (_interactable)
            {
                case true when _isActive:
                    _inputInstructionManager.Show(new []
                    {
                        new InputInstruction
                        {
                            key = "Dpad",
                            description = "Cycle Bait"
                        },
                        new InputInstruction
                        {
                            key = "X",
                            description = "Confirm Bait"
                        }
                    }, stream: 1);
                    break;
                case false when _isActive:
                    _inputInstructionManager.RemoveStream(1);
                    break;
            }
        }
        
        private void OnNextBait()
        {
            var baseBait = _pendingBait ?? _playerInventory.CurrentBaitView.CurrentValue;
            var baitList = _playerInventory.CurrentBaitsView.Select(x => x.Value).ToList();

            if (baitList.Count == 0) return;

            int currentIndex = baseBait == null ? -1 : baitList.IndexOf(baseBait);
            int nextIndex = (currentIndex + 1) % baitList.Count;

            _pendingBait = baitList[nextIndex];

        }

        private void OnPreviousBait()
        {
            var baseBait = _pendingBait ?? _playerInventory.CurrentBaitView.CurrentValue;
            var baitList = _playerInventory.CurrentBaitsView.Select(x => x.Value).ToList();

            if (baitList.Count == 0) return;

            int currentIndex = baseBait == null ? 0 : baitList.IndexOf(baseBait);
            int prevIndex = (currentIndex - 1 + baitList.Count) % baitList.Count;

            _pendingBait = baitList[prevIndex];

        }
        public void Dispose()
        {
            _bindings?.Dispose();
        }
    }
    
}
