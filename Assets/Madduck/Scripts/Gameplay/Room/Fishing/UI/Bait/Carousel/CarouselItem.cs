using System;
using UnityEngine;
using UnityEngine.EventSystems;



namespace HasanSadikin.Carousel
{
    public abstract class CarouselItem<T> : MonoBehaviour
    {
        [SerializeField] private T _data;
        public event Action<CarouselItem<T>> OnSelected;

        public RectTransform _rectTransform;
        public T Data 
        { 
            get => _data; 
            set 
            {
                _data = value;
                OnDataUpdated(_data);
            }
        }

        protected virtual void OnValidate()
        {
            if(_rectTransform == null)   _rectTransform = GetComponent<RectTransform>();
        }

        protected virtual void OnDataUpdated(T data) { }

        internal virtual void SetActive(bool value)
        {
            
            if (value) OnActivated();
            else OnDeactivated();
        }
        
        public virtual void SetSelected(bool isSelected) { }
        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }
    }
}