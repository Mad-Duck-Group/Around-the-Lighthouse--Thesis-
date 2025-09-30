using System;
using System.Collections.Generic;
using Madduck.Day;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class RoomTrackViewModel : IDisposable
    {
        
        public DayStateChangedEvent CurrentState { get; private set; }
        public event Action<DayStateChangedEvent> OnStateChanged; 
        private readonly ISubscriber<DayStateChangedEvent> _dayStateSubscriber;
        private readonly DayManager _dayManager;
        private IDisposable _bindings;
        
        [Inject]
        public RoomTrackViewModel(ISubscriber<DayStateChangedEvent> dayStateSubscriber)
        {
            _dayStateSubscriber = dayStateSubscriber;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _dayStateSubscriber
                .Subscribe(OnDayStateChanged)   
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDayStateChanged(DayStateChangedEvent e)
        {
            Debug.Log("State");
            CurrentState = e;
            OnStateChanged?.Invoke(e);
        }
        
        
        public void Dispose()
        {
            _bindings?.Dispose();
        }
    }
}
