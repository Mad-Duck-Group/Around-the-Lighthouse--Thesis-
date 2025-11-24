using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.GameData;
using Madduck.GameData.Bait;
using Madduck.Room;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer;

namespace HasanSadikin.Carousel
{
    public enum Origin {
        TopLeft,
        CenterLeft,
        BottomLeft,
        TopRight,
        CenterRight,
        BottomRight,
        Top,
        Center,
        Bottom
    }

    [RequireComponent(typeof(Image), typeof(Mask))]
    public class CarouselController : MonoBehaviour
    {
        [Header("Carousel Data")]
        private List<BaitItemInstance> BaitList
        {
            get
            {
                if (_playerInventory == null) return new List<BaitItemInstance>();
                return _playerInventory.AllBaits.Values
                    .Where(b => b.CurrentCount > 0) 
                    .ToList();
            }
        }
        [Header("Carousel Item")]
        [SerializeField] protected CarouselItem<BaitItemInstance> _carouselItemPrefab;

        [Header("Carousel Settings")]
        [SerializeField] protected Origin _childOrigin;
        [SerializeField] protected bool _isInfinity = true;
        [SerializeField] protected int _repeat = 2;
        [SerializeField] protected int _indexRepeatOffset = 1;
        public ReactiveProperty<int> CurrentIndex { get; } = new(0);
        public int TotalItems => _carouselItems.Count;
        public bool HasItems => _carouselItems.Count > 0;
        public ReactiveCommand<Unit> OnInitialized { get; } = new();
        public ReactiveCommand<BaitItemInstance> OnItemSelected { get; } = new();
        public ReactiveCommand<BaitItemInstance> OnCurrentItemUpdated { get; } = new();
        public ReactiveCommand<Unit> OnNext { get; } = new();
        public ReactiveCommand<Unit> OnPrev { get; } = new();
        public bool HasConfirmedItem => _confirmedBait != null;
        public PointingBaitConfig PointingBaitConfig => _pointingBaitConfig;
        protected int _currentIndex;
        protected ICarouselItemPositioner _positioner;
        protected List<CarouselItem<BaitItemInstance>> _carouselItems = new List<CarouselItem<BaitItemInstance>>();

        private readonly CompositeDisposable _disposables = new();
        private PointingBaitConfig _pointingBaitConfig;
        private PlayerInventory _playerInventory;
        private BaitItemInstance _confirmedBait;
        private BaitItemInstance _pointingBait;
        private BaitDetailPanel _baitDetailPanel;
        

        [Inject]
        public void Construct(ICarouselItemPositioner positioner,PlayerInventory playerInventory,PointingBaitConfig pointingBaitConfig , BaitDetailPanel baitDetailPanel)
        {
            
            _positioner = positioner;
            Debug.Log(_positioner);
            _playerInventory = playerInventory;
            _pointingBaitConfig = pointingBaitConfig;
            _baitDetailPanel = baitDetailPanel;
            Bind();
            CreateCarouselItems();
            var baitList = BaitList;
            if (HasItems && baitList.Count > 0)
                UpdateData();
            OnInitialized.Execute(Unit.Default);
        }
        
        public void Bind()
        {
            CurrentIndex.Subscribe(_ => UpdateData()).AddTo(_disposables);
        }
        
        protected virtual void OnValidate()
        {
            if (_repeat < 2) _repeat = 3;
            if (!Application.isPlaying) return; 
            UpdateData();
        }

        protected virtual void Start()
        {
            _currentIndex = 0;
            CurrentIndex.Value = 0;
            CreateCarouselItems();
            var baitList = BaitList;
            if (!HasItems || baitList.Count == 0) return;
                UpdateData();

            OnInitialized.Execute(Unit.Default);
        }

        protected virtual void CreateCarouselItems()
        {
            var baitList = BaitList;
            int itemsCount = _isInfinity ? baitList.Count * _repeat : baitList.Count;
            _carouselItems.Capacity = itemsCount;

            for (int i = 0; i < itemsCount; i++)
            {
                var newItem = Instantiate(_carouselItemPrefab, transform);
                newItem.Data = baitList[i % Math.Max(1, baitList.Count)];

                var rect = newItem.transform as RectTransform;
                SetChildOrigin(rect);
                rect.anchoredPosition = new Vector3(0, 0);

                newItem.OnSelected += clicked =>
                {
                    AdjustIndexForClickedItem(clicked);
                    OnItemSelected.Execute(clicked.Data);
                };

                _carouselItems.Add(newItem);
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var item in _carouselItems)
            {
                item.OnSelected -= AdjustIndexForClickedItem;
            }
        } 

        protected virtual void SetChildOrigin(RectTransform rect)
        {
            switch(_childOrigin)
            {
                case Origin.TopLeft:
                    rect.anchorMin = new Vector2(0, 1f);
                    rect.anchorMax = new Vector2(0, 1f);
                    rect.pivot = new Vector2(0, 1f);
                    break;
                case Origin.CenterLeft:
                    rect.anchorMin = new Vector2(0, .5f);
                    rect.anchorMax = new Vector2(0, .5f);
                    rect.pivot = new Vector2(0, .5f);
                    break;
                case Origin.BottomLeft:
                    rect.anchorMin = new Vector2(0, 0f);
                    rect.anchorMax = new Vector2(0, 0f);
                    rect.pivot = new Vector2(0, 0f);
                    break;
                    case Origin.TopRight:
                    rect.anchorMin = new Vector2(1, 1f);
                    rect.anchorMax = new Vector2(1, 1f);
                    rect.pivot = new Vector2(1, 1f);
                    break;
                case Origin.CenterRight:
                    rect.anchorMin = new Vector2(1, .5f);
                    rect.anchorMax = new Vector2(1, .5f);
                    rect.pivot = new Vector2(1, .5f);
                    break;
                case Origin.BottomRight:
                    rect.anchorMin = new Vector2(1, 0f);
                    rect.anchorMax = new Vector2(1, 0f);
                    rect.pivot = new Vector2(1, 0f);
                    break;
                case Origin.Top:
                    rect.anchorMin = new Vector2(.5f, 1f);
                    rect.anchorMax = new Vector2(.5f, 1f);
                    rect.pivot = new Vector2(.5f, 1f);
                    break;
                case Origin.Center:
                    rect.anchorMin = new Vector2(.5f, .5f);
                    rect.anchorMax = new Vector2(.5f, .5f);
                    rect.pivot = new Vector2(.5f, .5f);
                    break;
                case Origin.Bottom:
                    rect.anchorMin = new Vector2(.5f, 0);
                    rect.anchorMax = new Vector2(.5f, 0);
                    rect.pivot = new Vector2(.5f, 0);
                    break;
                default:
                    break;
            }
        }

        protected CarouselItem<BaitItemInstance> GetCarouselItemAt(int index)
        {
            return _carouselItems[GetCarouselIndex(index)];
        }
        public BaitItemInstance GetCurrentBaitVisual()
        {
            return GetCarouselItemAt(_currentIndex).Data;
        }
        protected int GetCarouselIndex(int index)
        {
            var count = _carouselItems.Count;
            if (count == 0) return 0; 
            var mod = index % count;
            return (mod + count) % count;
        }

        protected virtual void UpdateData()
        {
            if (_playerInventory == null) 
                return;
            var baitList = BaitList;
            if (!HasItems || baitList.Count == 0) return;

            for (int i = 0; i < _carouselItems.Count; i++)
            {
                var item = GetCarouselItemAt(i);
                int visualIndex = _isInfinity
                    ? GetCarouselIndex(i - _currentIndex) - baitList.Count * _indexRepeatOffset
                    : i - _currentIndex;

                bool isActive = visualIndex == 0;
                item.SetActive(isActive);
            
                if (isActive)
                {
                    _pointingBait = item.Data;
                    OnCurrentItemUpdated.Execute(item.Data);
                    SetBaitDetailPanel();
                    //UpdatePointing();
                }

                
                MoveItemToPositionAtIndex(item, _isInfinity ? GetCarouselIndex(i - _currentIndex) - baitList.Count * _indexRepeatOffset : i - _currentIndex);
            }
        }

        protected virtual void MoveItemToPositionAtIndex(CarouselItem<BaitItemInstance> item, int index)
        {
            _positioner?.SetPosition(item._rectTransform, index);
        }

        public virtual void Next()
        {
            var list = BaitList;
            if (!_isInfinity && _currentIndex + 1 >= list.Count) return;

            _currentIndex++;
          
            CurrentIndex.Value = _currentIndex;
            OnNext.Execute(Unit.Default);
        }

        public virtual void Previous()
        {
            if (!_isInfinity && _currentIndex - 1 < 0) return;

            _currentIndex--;

            CurrentIndex.Value = _currentIndex;
            OnPrev.Execute(Unit.Default);
        }

        public void ToggleSelection(BaitItemInstance bait)
        {
            var iconState = SelectionIcon.Unselected;
            if (_confirmedBait == bait)
            {
                _confirmedBait = null;
                UpdateIconSelection();
                //UpdatePointing();
                return;
            }
            
            _confirmedBait = bait;
            UpdateIconSelection();
            //UpdatePointing();
        }
        

        private void UpdateIconSelection()
        {
            foreach (var item in _carouselItems)
            {
                bool isSelected = (item.Data == _confirmedBait);
                item.SetSelected(isSelected);
            }
        }
        private void SetBaitDetailPanel()
        {
            _baitDetailPanel.baitNameText.text = _pointingBait.ItemData.BaitName;
            _baitDetailPanel.baitDescriptionText.text = _pointingBait.ItemData.BaitDescription;
        }

        protected virtual void AdjustIndexForClickedItem(CarouselItem<BaitItemInstance> clickedItem)
        {
            var baitList = BaitList;
            var targetItem = GetCarouselItemAt(_currentIndex + baitList.Count);
            int direction = _positioner.IsItemAfter(targetItem._rectTransform, clickedItem._rectTransform) ? -1 : 1;

            while (GetCarouselItemAt(_currentIndex + baitList.Count) != clickedItem)
            {
                _currentIndex += direction;
            }
            UpdateData();
        }
    }
}