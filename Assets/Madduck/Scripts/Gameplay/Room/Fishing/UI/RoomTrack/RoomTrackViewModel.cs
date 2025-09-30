using System;
using Madduck.Day;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class RoomTrackViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<uint> CurrentRoomIndex { get; }
        private readonly DayManager _dayManager;
        private IDisposable _bindings;
        private CompositeDisposable _disposables = new ();
        [Inject]
        public RoomTrackViewModel(DayManager dayManager )
        {
            _dayManager = dayManager; 
            CurrentRoomIndex =_dayManager.CurrentRoomIndex.ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}
