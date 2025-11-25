using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Input;
using Madduck.Utils;
using R3;
using UnityEngine;
using UnityEngine.UI;

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
        private readonly IFactory<IQuickTimeEvent> _qteElementFactory;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IQteElement _view;
        private readonly List<IQuickTimeEvent> _quickTimeEvents = new();
        private readonly CancellationTokenSource _qteSequenceCts = new();
        private CancellationTokenSource _qteCts = new();
        private UniTask _lastQteTask = UniTask.CompletedTask;
        
        
        private int _currentQteIndex;
        private bool _transitionedIn;
        private DisposableBag _subscription;
       

        public QteSequenceController(
            QteSequenceConfigInstance configInstance,
            IFactory<IQuickTimeEvent> qteElementFactory,
            IPlayerInputHandler inputHandler,
            IQteElement view)
        {
            _configInstance = configInstance;
            _qteElementFactory = qteElementFactory;
            _inputHandler = inputHandler;
            _view = view;
            CurrentElement = view;
        }
        
        public async UniTask TransitionInElement(CancellationToken cancellationToken = default)
        {
            if (_transitionedIn) return;
            _transitionedIn = true;
            await _view.TransitionIn(cancellationToken);
        }
        
        public void StartQuickTimeEvent()
        {
            StartQuickTimeEventInternal(_qteSequenceCts.Token).Forget();
        }

        private async UniTaskVoid StartQuickTimeEventInternal(CancellationToken cancellationToken)
        {
            _currentQteIndex = 0;
            for (var i = 0; i < _configInstance.CurrentSequenceLength; i++)
            {
                var qte = _qteElementFactory.Create();
                _quickTimeEvents.Add(qte);
                _view.SetAsChild(qte.CurrentElement);
                qte.ChangeInputActiveState(false); 
                qte.DestroyWhenFinished = false;
                qte.ChangeViewResultManually = true;
            } 
            if (!_transitionedIn)
                await TransitionInElement(cancellationToken);
            await UniTask.WaitForSeconds(_configInstance.CurrentStartDelay, cancellationToken: cancellationToken);
            ActivateElement(cancellationToken).Forget();
            for (var i = 0; i < _quickTimeEvents.Count; i++)
            {
                if (i > 0)
                {
                    await UniTask.WaitForSeconds(_configInstance.CurrentInterval, cancellationToken: cancellationToken);
                }
                var qte = _quickTimeEvents[i];
                await qte.TransitionInElement(cancellationToken);
            }
        }

        private async UniTaskVoid ActivateElement(CancellationToken cancellationToken)
        {
            await UniTask.WaitForSeconds(_configInstance.CurrentActivationDelay, cancellationToken: cancellationToken);
            SubscribeElement(_currentQteIndex);
            for (var i = 0; i < _quickTimeEvents.Count; i++)
            {
                if (i > 0)
                {
                    await UniTask.WaitForSeconds(_configInstance.CurrentInterval, cancellationToken: cancellationToken);
                }
                var qte = _quickTimeEvents[i];
                qte.StartQuickTimeEvent();
            }
        }

        private void SubscribeElement(int index)
        {
;           _subscription.Dispose();
            _subscription.Clear();
            _subscription = new();
            var current = _quickTimeEvents[index];
            if (index > 0) _quickTimeEvents[index - 1].ChangeInputActiveState(false);
            current.ChangeInputActiveState(true);
            _subscription.Add(Observable.FromEvent(
                    h => current.OnFail += h,
                    h => current.OnFail -= h)
                .Subscribe(_ =>
                {
                    _lastQteTask = current.ChangeViewResult(false, _qteCts.Token);
                    OnElementFail();
                }));
            _subscription.Add(Observable.FromEvent(
                    h => current.OnSuccess += h,
                    h => current.OnSuccess -= h)
                .Subscribe(_ =>
                {
                    _lastQteTask = current.ChangeViewResult(true, _qteCts.Token);
                    OnElementSuccess();
                }));
        }

        public void CancelQuickTimeEvent(bool success)
        {
            _qteSequenceCts.Cancel();
            if (success)
            {
                foreach (var qte in _quickTimeEvents)
                {
                    qte.CancelQuickTimeEvent(true);
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
            _qteSequenceCts.Cancel();
            _qteCts.Cancel();
            _subscription.Dispose();
            foreach (var qte in _quickTimeEvents)
            {
                ((IDisposable)qte).Dispose();
            }
        }

        private void OnElementSuccess()
        {
            _currentQteIndex++;
            if (_currentQteIndex >= _quickTimeEvents.Count)
            {
                Success();
            }
            else
            {
                UniTask.WaitForEndOfFrame()
                    .ContinueWith(() => SubscribeElement(_currentQteIndex));
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
            _qteSequenceCts.Cancel();
            _subscription.Dispose();
            foreach (var qte in _quickTimeEvents)
            {
                qte.CancelQuickTimeEvent(false);
            }
            if (!ChangeViewResultManually) ChangeViewResult(false).Forget();
            OnFail?.Invoke();
        }
        
        public async UniTask ChangeViewResult(bool result, CancellationToken cancellationToken = default)
        {
            await _lastQteTask;
            _qteCts.Cancel();
            _qteCts = new();
            if (result)
                await _view.OnSuccess(cancellationToken);
            else
                await _view.OnFail(cancellationToken);
            await _view.TransitionOut(cancellationToken);
            if (DestroyWhenFinished) _view.Destroy();
        }
        
        public void ChangeInputActiveState(bool active)
        {
            foreach (var qte in _quickTimeEvents)
            {
                qte.ChangeInputActiveState(active);
            }
        }

        
    }
}