using System;

namespace Madduck.Shared
{
    public interface IQuickTimeEvent
    {
        event Action OnSuccess;
        event Action OnFail;
        void StartQuickTimeEvent();
        void CancelQuickTimeEvent(bool success);
    }
}