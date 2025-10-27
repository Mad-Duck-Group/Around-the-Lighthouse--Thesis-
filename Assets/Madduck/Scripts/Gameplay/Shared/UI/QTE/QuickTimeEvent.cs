using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using UnityEngine;

namespace Madduck.Shared
{
    public interface IQuickTimeEvent
    {
        event Action OnSuccess;
        event Action OnFail;
        void StartQuickTimeEvent();
        void CancelQuickTimeEvent(bool success);
        void ChangeInputActiveState(bool active);
        UniTask ChangeViewResult(bool result, CancellationToken cancellationToken = default);
        bool DestroyWhenFinished { get; set; }
        bool ChangeViewResultManually { get; set; }
        IQteElement CurrentElement { get; }
    }
    
    public interface IQteElement : ITransitionable
    {
        UniTask OnSuccess(CancellationToken cancellationToken = default);
        UniTask OnFail(CancellationToken cancellationToken = default);
        void Destroy();
        void SetAsChild(IQteElement child);
    }
}