using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Madduck.GameData;
using Madduck.GameData.Bait;
using Madduck.Utils;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class BaitSelectionView : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Transform baitButtonsParent;
        [SerializeField] private BaitButtonView baitButtonViewPrefab;
        
        private BaitSelectionViewModel _viewModel;
        private IDisposable _bindings;
        private readonly Dictionary<BaitType, BaitButtonView> _baitButtonViews = new();

        [Inject]
        public void SetUp(BaitSelectionViewModel viewModel)
        {
            _viewModel = viewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CurrentBaitsView
                .ObserveChanged()
                .Subscribe(OnBaitChanged)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
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
                        Destroy(view.gameObject);
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
                baitButtonView = Instantiate(baitButtonViewPrefab, baitButtonsParent);
                _baitButtonViews.TryAdd(newItem.Key, baitButtonView);
                baitButtonView.SetUp(_viewModel, newItem.Value);
            }

            void OnRemove()
            {
                if (oldItem.Value is null) return;
                if (_baitButtonViews.Remove(oldItem.Key, out baitButtonView))
                    Destroy(baitButtonView.gameObject);
            }
        }
    }
}