    using System;
using Madduck.Day;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class RoomTrackViewModel
    {
        public ReadOnlyReactiveProperty<uint> CurrentRoomIndex { get; }
        
        [Inject]
        public RoomTrackViewModel(DayManager dayManager)
        {
            CurrentRoomIndex = dayManager.CurrentRoomIndex.ToReadOnlyReactiveProperty();
        }
    }
}
