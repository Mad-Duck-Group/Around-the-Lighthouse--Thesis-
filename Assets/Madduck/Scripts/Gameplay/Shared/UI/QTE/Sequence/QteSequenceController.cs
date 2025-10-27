using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Input;
using Madduck.Utils;
using R3;

namespace Madduck.Shared
{
    public class QteSequenceController : IQuickTimeEvent, IDisposable
    {
        public event Action OnSuccess;
        public event Action OnFail;
        public IQteElement CurrentElement { get; }
        public bool ChangeViewResultManually { get; set; } = false;
        public bool DestroyWhenFinished { get; set; } = true;
        
        private readonly QteSequenceConfigInstance _configInstance;
        private readonly IGenericFactory<IQuickTimeEvent> _qteElementFactory;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IQteElement _view;
        private readonly List<IQuickTimeEvent> _elements = new();
        private readonly CancellationTokenSource _qteCts = new();
        private CancellationTokenSource _elementCts = new();
        
        
        private int _currentElementIndex;
        private DisposableBag _subscription;
       

        public QteSequenceController(
            QteSequenceConfigInstance configInstance,
            IGenericFactory<IQuickTimeEvent> qteElementFactory,
            IPlayerInputHandler inputHandler,
            IQteElement view)
        {
            _configInstance = configInstance;
            _qteElementFactory = qteElementFactory;
            _inputHandler = inputHandler;
            _view = view;
            CurrentElement = view;
        }
        
        public void StartQuickTimeEvent()
        {
            _currentElementIndex = 0;
            for (var i = 0; i < _configInstance.CurrentSequenceLength; i++)
            {
                var element = _qteElementFactory.Create();
                _elements.Add(element);
                _view.SetAsChild(element.CurrentElement);
                element.ChangeInputActiveState(false); 
                element.DestroyWhenFinished = false;
                element.ChangeViewResultManually = true;
            }
            StartQuickTimeEventInternal(_qteCts.Token).Forget();
        }

        private async UniTaskVoid StartQuickTimeEventInternal(CancellationToken cancellationToken)
        {
            await _view.TransitionIn(cancellationToken);
            await UniTask.WaitForSeconds(_configInstance.CurrentStartDelay, cancellationToken: cancellationToken);
            SubscribeElement(_currentElementIndex);
            for (var i = 0; i < _elements.Count; i++)
            {
                if (i > 0)
                {
                    await UniTask.WaitForSeconds(_configInstance.CurrentInterval, cancellationToken: cancellationToken);
                }
                var element = _elements[i];
                element.StartQuickTimeEvent();
            }
        }

        private void SubscribeElement(int index)
        {
;           _subscription.Dispose();
            _subscription.Clear();
            _subscription = new();
            var current = _elements[index];
            if (index > 0) _elements[index - 1].ChangeInputActiveState(false);
            current.ChangeInputActiveState(true);
            _subscription.Add(Observable.FromEvent(
                    h => current.OnFail += h,
                    h => current.OnFail -= h)
                .Subscribe(_ =>
                {
                    current.ChangeViewResult(false, _elementCts.Token);
                    OnElementFail();
                }));
            _subscription.Add(Observable.FromEvent(
                    h => current.OnSuccess += h,
                    h => current.OnSuccess -= h)
                .Subscribe(_ =>
                {
                    current.ChangeViewResult(true, _elementCts.Token);
                    OnElementSuccess();
                }));
        }

        public void CancelQuickTimeEvent(bool success)
        {
            _qteCts.Cancel();
            if (success)
            {
                foreach (var element in _elements)
                {
                    element.CancelQuickTimeEvent(true);
                }
                Success();
            }
            else
            {
                Fail();
            }
        }

        public void Dispose()
        {
            _qteCts.Cancel();
            _elementCts.Cancel();
            _subscription.Dispose();
            foreach (var element in _elements)
            {
                ((IDisposable)element).Dispose();
            }
        }

        private void OnElementSuccess()
        {
            _currentElementIndex++;
            if (_currentElementIndex >= _elements.Count)
            {
                Success();
            }
            else
            {
                UniTask.WaitForEndOfFrame()
                    .ContinueWith(() => SubscribeElement(_currentElementIndex));
            }
        }

        private void OnElementFail()
        {
            Fail();
        }
        
        private void Success()
        {
            _subscription.Dispose();
            if (!ChangeViewResultManually) ChangeViewResult(true).Forget();
            OnSuccess?.Invoke();
        }
        
        private void Fail()
        {
            _qteCts.Cancel();
            _subscription.Dispose();
            foreach (var element in _elements)
            {
                element.CancelQuickTimeEvent(false);
            }
            if (!ChangeViewResultManually) ChangeViewResult(false).Forget();
            OnFail?.Invoke();
        }
        
        public async UniTask ChangeViewResult(bool result, CancellationToken cancellationToken = default)
        {
            _elementCts.Cancel();
            _elementCts = new();
            if (result)
                await _view.OnSuccess(cancellationToken);
            else
                await _view.OnFail(cancellationToken);
            await _view.TransitionOut(cancellationToken);
            if (DestroyWhenFinished) _view.Destroy();
        }
        
        public void ChangeInputActiveState(bool active)
        {
            foreach (var element in _elements)
            {
                element.ChangeInputActiveState(active);
            }
        }

        
    }
}