using System;
using System.Collections.Generic;
using Madduck.GameData;
using Madduck.GameData.Bait;
using ObservableCollections;
using R3;
using VContainer;

namespace Madduck.Room
{
    public class BaitSelectionViewModel : IDisposable
    {
        public ISynchronizedView<KeyValuePair<BaitType, BaitItemInstance>, 
            KeyValuePair<BaitType, BaitItemInstance>> CurrentBaitsView { get; }
        public ReadOnlyReactiveProperty<BaitItemInstance> CurrentBaitView { get; }
        public ReactiveCommand<BaitType> SetCurrentBaitCommand { get; } = new();
        
        private readonly PlayerInventory _playerInventory;
        private IDisposable _bindings;

        [Inject]
        public BaitSelectionViewModel(
            PlayerInventory playerInventory)
        {
            _playerInventory = playerInventory;
            CurrentBaitsView = _playerInventory.CurrentBaitsView;
            CurrentBaitView = _playerInventory.CurrentBait.ToReadOnlyReactiveProperty();
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            SetCurrentBaitCommand
                .Subscribe(OnSetCurrentBait)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }

        private void OnSetCurrentBait(BaitType baitType)
        {
            _playerInventory.SetCurrentBait(baitType);
        }
    }
}