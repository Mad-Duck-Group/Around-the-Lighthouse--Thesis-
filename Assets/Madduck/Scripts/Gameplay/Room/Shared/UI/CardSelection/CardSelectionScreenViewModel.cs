using System;
using Madduck.GameData;
using Madduck.Shared;
using R3;
using VContainer;

namespace Madduck.Room
{
    public class CardSelectionScreenViewModel : IDisposable
    {
        public event Action OnConfirmCardEvent;
        public ReactiveCommand<CardItemInstance> SelectCardCommand { get; private set; } = new();
        public ReactiveCommand ConfirmCardCommand { get; private set; } = new();
        public ReactiveProperty<CardItemInstance> SelectedCard { get; private set; } = new();
        
        private readonly PlayerInventory _playerInventory;
        
        private IDisposable _bindings;

        [Inject]
        public CardSelectionScreenViewModel(
            PlayerInventory playerInventory)
        {
            _playerInventory = playerInventory;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = new DisposableBuilder();
            SelectCardCommand
                .Subscribe(OnSelectCard)
                .AddTo(ref disposableBuilder);
            ConfirmCardCommand
                .Subscribe(_ => OnConfirmCard())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _bindings.Dispose();
        }

        private void OnSelectCard(CardItemInstance cardItemInstance)
        {
            SelectedCard.Value = cardItemInstance;
        }

        private void OnConfirmCard()
        {
            _playerInventory.AddCard(SelectedCard.Value);
            OnConfirmCardEvent?.Invoke();
        }
    }
}