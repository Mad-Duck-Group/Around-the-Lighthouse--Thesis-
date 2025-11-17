using System;
using Madduck.Shared;
using UnityEngine;

namespace Madduck.Day
{
    [Serializable]
    public struct  DayRoomKey
    {
        public DayPhaseType dayPhase;
        public RoomType room;
        

        public DayRoomKey(DayPhaseType dayPhase, RoomType room )
        {
            this.dayPhase = dayPhase;
            this.room = room;
           
        }
        
    }
}
