using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;

namespace Madduck.Utils
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
        void Queue(IModal modal);
    }
    
    [Serializable]
    public class ModalManager : IModalManager
    {
        [ShowInInspector] private readonly Queue<IModal> _modalQueue = new();
        [ShowInInspector] private IModal _currentModal;
        private CancellationTokenSource _modalCts = new();
        private IDisposable _modalHiddenSubscription;
        
        public void Queue(IModal modal)
        {
            _modalQueue.Enqueue(modal);
            if (_modalQueue.Count == 1)
                ShowNextModal();
        }
        
        #region Utils

        private void ShowNextModal()
        {
            _modalHiddenSubscription?.Dispose();
            _currentModal = null;
            if (_modalQueue.Count == 0)
            {
                return;
            }
            _currentModal = _modalQueue.Dequeue();
            _modalCts.Cancel();
            _modalCts = new CancellationTokenSource();
            _modalHiddenSubscription = Observable.FromEvent(
                    h => _currentModal.OnClose += h,
                    h => _currentModal.OnClose -= h)
                .Subscribe(_ =>
                {
                    ShowNextModal();
                });
            _currentModal.Show(_modalCts.Token).Forget();
        }

        #endregion
    }
}