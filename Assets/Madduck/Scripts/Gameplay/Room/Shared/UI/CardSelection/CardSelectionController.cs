#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    public class CardSelectionController : IModal, IDisposable
    {
        public event Action OnOpen;
        public event Action OnClose;
        
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
                .Subscribe(_ => Hide().Forget())
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

        private void SpawnCards()
        {
            var randomCards = new CardItemData?[3];
            _cardWeightTableInstance.GetRandomUniqueItems(randomCards, true);
            foreach (var card in randomCards)
            {
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
        
        public async UniTask Show(CancellationToken cancellationToken = default)
        {
            await _cardSelectionScreen.TransitionIn(cancellationToken);
            SpawnCards();
            await TransitionCards(true);
            OnOpen?.Invoke();
        }

        public async UniTask Hide(CancellationToken cancellationToken = default)
        {
            await TransitionCards(false);
            await _cardSelectionScreen.TransitionOut(cancellationToken);
            OnClose?.Invoke();
        }
    }
}