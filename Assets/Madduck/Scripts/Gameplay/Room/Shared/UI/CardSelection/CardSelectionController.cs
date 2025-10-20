using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    public class CardSelectionController : IDisposable
    {
        public event Action OnCardSelectionClosed;
        
        private readonly CardSelectionScreenViewModel _viewModel;
        private readonly CardWeightTableInstance _cardWeightTableInstance;
        private readonly CardRarityWeightTableInstance _cardRarityWeightTableInstance;
        private readonly IGenericFactory<CardSelectionView> _cardSelectionFactory;
        private readonly ITransitionable _cardSelectionScreen;
        private readonly List<CardSelectionView> _cardSelectionViews = new();
        
        private IDisposable _subscriptions;
        
        [Inject]
        public CardSelectionController(
            CardSelectionScreenViewModel viewModel,
            CardWeightTableInstance cardWeightTableInstance,
            CardRarityWeightTableInstance cardRarityWeightTableInstance,
            IGenericFactory<CardSelectionView> cardSelectionFactory,
            ITransitionable cardSelectionScreen)
        {
            _viewModel = viewModel;
            _cardWeightTableInstance = cardWeightTableInstance;
            _cardRarityWeightTableInstance = cardRarityWeightTableInstance;
            _cardSelectionFactory = cardSelectionFactory;
            _cardSelectionScreen = cardSelectionScreen;
            Subscribe();
        }

        private void Subscribe()
        {
            var disposableBuilder = new DisposableBuilder();
            Observable.FromEvent(
                addHandler: handler => _viewModel.OnConfirmCardEvent += handler,
                removeHandler: handler => _viewModel.OnConfirmCardEvent -= handler)
                .Subscribe(_ => SetActive(false).Forget())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            foreach (var view in _cardSelectionViews)
            {
                Object.Destroy(view);
            }
            _cardSelectionViews.Clear();
        }

        public async UniTask SetActive(bool active)
        {
            if (active)
            {
                await _cardSelectionScreen.TransitionIn();
                SpawnCards();
                await TransitionCards(true);
            }
            else
            {
                await TransitionCards(false);
                await _cardSelectionScreen.TransitionOut();
                OnCardSelectionClosed?.Invoke();
            }
        }

        private void SpawnCards()
        {
            for (var i = 0; i < 3; i++)
            {
                var card = _cardWeightTableInstance.GetRandomItem();
                var rarity = _cardRarityWeightTableInstance.GetRandomItem();
                var cardItemInstance = new CardItemInstance(card);
                cardItemInstance.SetRarity(rarity);
                var view = _cardSelectionFactory.Create();
                view.SetUp(_viewModel);
                view.SetCard(cardItemInstance);
                _cardSelectionViews.Add(view);
            }
        }

        private async UniTask TransitionCards(bool forward)
        {
            foreach (var view in _cardSelectionViews)
            {
                if (forward) await view.TransitionIn();
                else await view.TransitionOut();
            }
        }
    }
}