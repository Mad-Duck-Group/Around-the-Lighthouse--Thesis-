using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Madduck.Scripts.Utils.Others
{
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [field: SerializeField] public UnityEvent OnHold { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnClick { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnRelease { get; private set; } = new(); 
        [ShowInInspector, ReadOnly] private bool _isHolding;
        [ShowInInspector, ReadOnly] public float HoldDuration { get; private set; }

        private void Start()
        {
            Observable.EveryUpdate()
                .Where(_ => _isHolding)
                .Subscribe(_ =>
                {
                    OnHold.Invoke();
                    HoldDuration += Time.deltaTime;
                })
                .AddTo(this);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _isHolding = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isHolding = false;
            OnRelease.Invoke();
            HoldDuration = 0;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Invoke();
        }
    }
}