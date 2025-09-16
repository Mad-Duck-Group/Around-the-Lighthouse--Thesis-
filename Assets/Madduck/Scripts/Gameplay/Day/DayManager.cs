using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Day
{
    public class DayManager
    {
        [Title("Debug"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugTitle;
        [field: DisplayAsString, 
               ShowInInspector] public uint CurrentDayIndex { get; private set; }
        [field: DisplayAsString, 
               ShowInInspector] public uint CurrentRoomIndex { get; private set; }
        [field: DisplayAsString, 
         ShowInInspector] public DayPhaseType CurrentDayPhase { get; private set; } = DayPhaseType.Day;

        public FishWeightTableInstance FishWeightTable { get; private set; }
        private readonly DayManagerConfig _config;
        
        [Inject]
        public DayManager(
            FishWeightTableInstance fishWeightTable, 
            DayManagerConfig config)
        {
            FishWeightTable = fishWeightTable;
            _config = config;
            SetDayPhase();
        }

        /// <summary>
        /// Set current day phase based on current room index and config day night ratio
        /// </summary>
        private void SetDayPhase()
        {
            var percent = Percentage.FromFraction((float)CurrentRoomIndex / (_config.MaxRoomCount - 1));
            CurrentDayPhase = percent <= _config.DayNightRatio ? DayPhaseType.Day : DayPhaseType.Night;
            FilterFishByDayPhase();
        }
        
        /// <summary>
        /// Filter fish weight table by current day phase
        /// </summary>
        private void FilterFishByDayPhase()
        {
            FishWeightTable.PersistentFilters.Remove("DayPhaseFilter");
            var filter = new FishWeightFilter(record => record.Item.DayPhaseType.HasFlag(CurrentDayPhase));
            FishWeightTable.PersistentFilters.TryAdd("DayPhaseFilter", filter);
        }
        
        /// <summary>
        /// Set current day index and reset room index to 0
        /// </summary>
        /// <param name="day"></param>
        public void SetDayIndex(uint day)
        {
            CurrentDayIndex = day;
            SetRoomIndex(0);
        }
        
        /// <summary>
        /// Change current day index by given value and reset room index to 0
        /// </summary>
        /// <param name="day"></param>
        public void ChangeDayIndex(int day)
        {
            CurrentDayIndex += (uint)day;
            CurrentDayIndex = (uint)Mathf.Clamp(CurrentDayIndex, 0, _config.MaxDayCount - 1);
            SetRoomIndex(0);
        }
        
        /// <summary>
        /// Set current room index
        /// </summary>
        /// <param name="room"></param>
        public void SetRoomIndex(uint room)
        {
            CurrentRoomIndex = room;
            SetDayPhase();
        }
        
        /// <summary>
        /// Change current room index by given value
        /// </summary>
        /// <param name="room"></param>
        public void ChangeRoom(int room)
        {
            CurrentRoomIndex += (uint)room;
            CurrentDayIndex = (uint)Mathf.Clamp(CurrentDayIndex, 0, _config.MaxRoomCount - 1);
            SetDayPhase();
        }
    }
}