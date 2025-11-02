using System;
using Madduck.GameData;
using Madduck.GameData.Bait;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    public class BaitController : IDisposable ,IStartable
    {
        private readonly PlayerInputHandler _inputHandler;
        private readonly PlayerInventory _playerInventory;
        private readonly ISubscriber<FishingStateEvent> _fishingStateSubscriber;

        private bool _interactable = true;

        private IDisposable _baitBinding;

        [Inject]
        public BaitController(PlayerInputHandler inputHandler,
            PlayerInventory playerInventory,
            ISubscriber<FishingStateEvent> fishingStateSubscriber)
        {
            _inputHandler = inputHandler;
            _playerInventory = playerInventory;
            _fishingStateSubscriber = fishingStateSubscriber;
        }
        
        public void Start()
        {
            Bind();
            DebugUtils.Log("test start");

        }
        private void Bind()
        {
            var builder = Disposable.CreateBuilder();
            _fishingStateSubscriber.Subscribe(OnFishingStateEvent).AddTo(ref builder);
            _baitBinding = _inputHandler.BaitButton.IsDown.Where(x => x)
                .Subscribe(_ => { OpenUI();});
            _baitBinding = _inputHandler.BaitSelectInput.Subscribe(value =>
            {
                // if (value > 0 && _interactable)
                // {
                //     OnCarouselItemUpdated(_playerInventory.GetNextBait());
                // }
                // else if (value < 0&& _interactable)
                // {
                //     OnCarouselItemUpdated(_playerInventory.GetPreviousBait());
                // }
            }).AddTo(ref builder);
            //DebugUtils.Log("test inject");
        }

        private void OpenUI()
        {
            DebugUtils.Log("OpenUI");
        }
        private void OnFishingStateEvent(FishingStateEvent evt)
        {
            _interactable = evt.StateType is FishingStateType.ThrowHook;
        }

        private void OnCarouselItemUpdated(BaitItemInstance bait)
        {
            if (bait == null) return;
            _playerInventory.SetCurrentBait(bait.ItemData.BaitType);
        }

        private void OnCarouselItemSelected(BaitItemInstance bait)
        {
            if (!_interactable || bait == null) return;
            _playerInventory.SetCurrentBait(bait.ItemData.BaitType);
            DebugUtils.Log($"Selected bait: {bait.ItemData.BaitType}");
        }

        public void Dispose()
        {
            _baitBinding?.Dispose();
        }


        
    }
}
