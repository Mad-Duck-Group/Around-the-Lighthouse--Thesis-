using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Input;
using R3;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.Shared
{
    public interface IModal
    {
        event Action OnOpen;
        event Action OnClose;
        UniTask Show(CancellationToken cancellationToken = default);
        UniTask Hide(CancellationToken cancellationToken = default);
    }

    public interface IModalManager
    {
        event Action<IModal> OnMadalOpened;
        event Action<IModal> OnModalClosed;
        event Action OnAllModalsClosed;
        int ModalCount { get; }
        void Queue(IModal modal);
    }
    
    [Serializable]
    public class ModalManager : IModalManager
    {
        public event Action<IModal> OnMadalOpened;
        public event Action<IModal> OnModalClosed;
        public event Action OnAllModalsClosed;
        public int ModalCount => _modalQueue.Count;
        [ShowInInspector] private readonly Queue<IModal> _modalQueue = new();
        [ShowInInspector] private IModal _currentModal;
        private readonly IPlayerInputHandler _inputHandler;
        private CancellationTokenSource _modalCts = new();
        private IDisposable _modalHiddenSubscription;

        [Inject]
        public ModalManager(IPlayerInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }

        public void Queue(IModal modal)
        {
            _modalQueue.Enqueue(modal);
            if (_currentModal is null)
                ShowNextModal();
        }
        
        #region Utils

        private void ShowNextModal()
        {
            _modalHiddenSubscription?.Dispose();
            _currentModal = null;
            if (_modalQueue.Count == 0)
            {
                OnAllModalsClosed?.Invoke();
                return;
            }
            _inputHandler.SetActiveInput(false);
            _currentModal = _modalQueue.Dequeue();
            _modalCts.Cancel();
            _modalCts = new CancellationTokenSource();
            _modalHiddenSubscription = Observable.FromEvent(
                    h => _currentModal.OnClose += h,
                    h => _currentModal.OnClose -= h)
                .Subscribe(_ =>
                {
                    OnModalClosed?.Invoke(_currentModal);
                    _inputHandler.SetActiveInput(true);
                    ShowNextModal();
                });
            _currentModal.Show(_modalCts.Token).Forget();
            OnMadalOpened?.Invoke(_currentModal);
        }

        #endregion
    }
}