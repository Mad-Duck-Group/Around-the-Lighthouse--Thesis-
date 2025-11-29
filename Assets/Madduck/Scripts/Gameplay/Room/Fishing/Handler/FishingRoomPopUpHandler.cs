using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using VContainer;
using DisposableBag = R3.DisposableBag;

namespace Madduck.Room
{
    public class FishingRoomPopUpHandler : IDisposable
    {
        private readonly FishingRoomManager _fishingRoomManager;
        private readonly FishCatalogue _fishCatalogue;
        private readonly IModalManager _modalManager;
        private readonly IModal _cardSelectionController;
        private readonly IPopUpFactory<FishableItemPopUpObject> _fishableItemPopUpFactory;
        private readonly IPopUpFactory<NewFishPopUpObject> _newFishPopUpFactory;
        private readonly IPopUpFactory<EndGamePopUpObject> _endGamePopUpFactory;
        
        private IDisposable _subscriptions;
        private DisposableBag _disposables = new();

        [Inject]
        public FishingRoomPopUpHandler(
            FishingRoomManager fishingRoomManager,
            FishCatalogue fishCatalogue,
            IModalManager modalManager,
            IModal cardSelectionController,
            IPopUpFactory<FishableItemPopUpObject> fishableItemPopUpFactory,
            IPopUpFactory<NewFishPopUpObject> newFishPopUpFactory,
            IPopUpFactory<EndGamePopUpObject> endGamePopUpFactory)
        {
            _fishingRoomManager = fishingRoomManager;
            _fishCatalogue = fishCatalogue;
            _modalManager = modalManager;
            _cardSelectionController = cardSelectionController;
            _fishableItemPopUpFactory = fishableItemPopUpFactory;
            _newFishPopUpFactory = newFishPopUpFactory;
            _endGamePopUpFactory = endGamePopUpFactory;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            Observable.FromEvent<FishableCaughtEvent>(
                    h => _fishingRoomManager.OnFishableCaught += h, 
                    h => _fishingRoomManager.OnFishableCaught -= h)
                .Subscribe(OnFishableCaught)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions?.Dispose();
            _disposables.Dispose();
        }

        private void OnFishableCaught(FishableCaughtEvent eventData)
        {
            HandlePopUp(eventData.FishableItemInstances, out var gotBoss);
            if (gotBoss)
            {
                _fishingRoomManager.bossCaught = true;
                Observable.FromEvent(
                        h => _modalManager.OnAllModalsClosed += h,
                        h => _modalManager.OnAllModalsClosed -= h)
                    .Subscribe(_ => _fishingRoomManager.ToMainMenu())
                    .AddTo(ref _disposables);
                return;
            }
            //_modalManager.Queue(_cardSelectionController); //NOTE: Disable for now
            if (_fishingRoomManager.CurrentFishCount.Value != 0) return;
            Observable.FromEvent(
                    h => _modalManager.OnAllModalsClosed += h,
                    h => _modalManager.OnAllModalsClosed -= h)
                .Subscribe(_ => _fishingRoomManager.EndFishingRoom())
                .AddTo(ref _disposables);
        }

        private void HandlePopUp(List<IFishableItemInstance> fishableItemInstances, out bool gotBoss)
        {
            gotBoss = false;
            var unCaughtFishItems = new List<FishItemInstance>();
            var others = new List<IFishableItemInstance>();
            var fishCount = 0;
            foreach (var fishable in fishableItemInstances)
            {
                switch (fishable)
                {
                    case FishItemInstance fishItemInstance:
                        if (fishItemInstance.ItemData.EnemyType is FishEnemyType.Boss)
                        {
                            gotBoss = true;
                            var popUp = _endGamePopUpFactory.Create();
                            popUp.SetPopUpObject(new EndGamePopUpObject());
                            _modalManager.Queue(popUp);
                            return;
                        }
                        var fishGuid = fishItemInstance.ItemData.Guid;
                        if (_fishCatalogue.HasCaught(fishGuid))
                        {
                            others.Add(fishItemInstance);
                        }
                        else
                        {
                            unCaughtFishItems.Add(fishItemInstance);
                        }
                        fishCount++;
                        break;
                    case ResourceItemInstance resourceItemInstance:
                        others.Add(resourceItemInstance);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(fishable));
                }
            }
            foreach (var unCaught in unCaughtFishItems)
            {
                var popUp = _newFishPopUpFactory.Create();
                popUp.SetPopUpObject(new NewFishPopUpObject(unCaught));
                _modalManager.Queue(popUp);
                _fishCatalogue.SetCaught(unCaught.ItemData.Guid);
                _fishCatalogue.Save();
            }
            var chunked = others.Chunk(3).Select(x => x.ToList()).ToList();
            foreach (var chunk in chunked)
            {
                if (chunk.Count <= 0) continue;
                var popUp = _fishableItemPopUpFactory.Create();
                popUp.SetPopUpObject(new FishableItemPopUpObject(chunk));
                _modalManager.Queue(popUp);
            }
            _fishingRoomManager.ChangeFishCount(-fishCount);
        }
    }
}