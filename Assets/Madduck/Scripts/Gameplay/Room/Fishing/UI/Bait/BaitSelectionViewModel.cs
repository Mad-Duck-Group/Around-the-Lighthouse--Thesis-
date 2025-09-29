using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Madduck.GameData;
using Madduck.GameData.Bait;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    public class BaitSelectionViewModel : IDisposable
    {
        #region Properties

        public ReadOnlyReactiveProperty<bool> InteractableView { get; }
        public ReadOnlyReactiveProperty<BaitItemInstance> CurrentBaitView { get; }
        public ReactiveCommand<BaitType> SetCurrentBaitCommand { get; } = new();

        #endregion

        #region Fields

        private readonly PlayerInventory _playerInventory;
        private readonly IGenericFactory<BaitButtonView> _baitButtonViewFactory;
        private readonly ISubscriber<FishingStateEvent> _fishingStateEventSubscriber;
        private readonly Dictionary<BaitType, BaitButtonView> _baitButtonViews = new();
        private readonly ReactiveProperty<bool> _interactable = new(true);
        private IDisposable _bindings;

        #endregion

        #region Injection

        [Inject]
        public BaitSelectionViewModel(
            PlayerInventory playerInventory,
            IGenericFactory<BaitButtonView> baitButtonViewFactory,
            ISubscriber<FishingStateEvent> fishingStateEventSubscriber)
        {
            _playerInventory = playerInventory;
            _baitButtonViewFactory = baitButtonViewFactory;
            _fishingStateEventSubscriber = fishingStateEventSubscriber;
            InteractableView = _interactable.ToReadOnlyReactiveProperty();
            CurrentBaitView = _playerInventory.CurrentBaitView.ToReadOnlyReactiveProperty();
            Bind();
        }

        #endregion

        #region Binding

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _playerInventory.CurrentBaitsView
                .ObserveChanged()
                .Subscribe(OnBaitChanged)
                .AddTo(ref disposableBuilder);
            SetCurrentBaitCommand
                .Subscribe(OnSetCurrentBait)
                .AddTo(ref disposableBuilder);
            _fishingStateEventSubscriber
                .Subscribe(OnFishingStateEvent)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }

        #endregion

        #region Events

        private void OnFishingStateEvent(FishingStateEvent eventData)
        {
            _interactable.Value = eventData.StateType is FishingStateType.ThrowHook;
        }

        private void OnSetCurrentBait(BaitType baitType)
        {
            var selected = false;
            var currentBait = _playerInventory.CurrentBaitView.CurrentValue;
            if (currentBait is not null)
            {
                var currentType = currentBait.ItemData.BaitType;
                selected = currentType == baitType;
            }
            _playerInventory.SetCurrentBait(selected ? BaitType.None : baitType);
        }
        
        private void OnBaitChanged(
            ViewChangedEvent<KeyValuePair<BaitType, BaitItemInstance>, KeyValuePair<BaitType, BaitItemInstance>>
                viewChangedEvent)
        {
            var newItem = viewChangedEvent.NewItem.View;
            var oldItem = viewChangedEvent.OldItem.View;
            BaitButtonView baitButtonView;
            switch (viewChangedEvent.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnAdd();
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Ignore
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnRemove();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnRemove();
                    OnAdd();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var view in _baitButtonViews.Values)
                    {
                        Object.Destroy(view.gameObject);
                    }
                    _baitButtonViews.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return;

            void OnAdd()
            {
                if (newItem.Value is null) return;
                baitButtonView = _baitButtonViewFactory.Create();
                _baitButtonViews.TryAdd(newItem.Key, baitButtonView);
                baitButtonView.SetBait(this, newItem.Value);
            }

            void OnRemove()
            {
                if (oldItem.Value is null) return;
                if (_baitButtonViews.Remove(oldItem.Key, out baitButtonView))
                    Object.Destroy(baitButtonView.gameObject);
            }
        }

        #endregion
    }
}