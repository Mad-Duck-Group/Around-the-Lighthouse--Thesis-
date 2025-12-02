using Madduck.Shared;
using UnityEngine;

namespace Madduck.GameData
{
    public readonly struct DayStateChangedEvent
    {
        public uint DayIndex { get; }
        public uint RoomIndex { get; }
        public RoomType RoomType { get; }
        public DayPhaseType Phase { get; }

        public DayStateChangedEvent(uint dayIndex, uint roomIndex, RoomType roomType, DayPhaseType phase)
        {
            DayIndex = dayIndex;
            RoomIndex = roomIndex;
            RoomType = roomType;
            Phase = phase;
        }
    }
}
