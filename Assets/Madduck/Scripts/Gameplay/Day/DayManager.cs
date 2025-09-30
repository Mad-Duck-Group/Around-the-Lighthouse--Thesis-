using System;
using System.Collections.Generic;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Day
{
    
    public class DayManager : IMaxFishCountProvider, IDisposable
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

        [field: DisplayAsString,
                ShowInInspector] public List<RoomType> RoomHistory { get; private set; } = new();
        [Button("Next Room")]
        private void NextRoom() => ChangeRoom(1);

        public FishWeightTableInstance FishWeightTable { get; private set; }
        public DayManagerConfig _config { get; private set; }
        private readonly ISubscriber<OutOfFishEvent> _outOfFishEventSubscriber;
        private readonly IPublisher<DayStateChangedEvent> _dayStatePublisher;
        

        
        private IDisposable _subscriptions;
        
        [Inject]
        public DayManager(
            FishWeightTableInstance fishWeightTable, 
            DayManagerConfig config,
            ISubscriber<OutOfFishEvent> outOfFishEventSubscriber,
            IPublisher<DayStateChangedEvent> dayStatePublisher)
        {
            FishWeightTable = fishWeightTable;
            _config = config;
            _outOfFishEventSubscriber = outOfFishEventSubscriber;
            _dayStatePublisher = dayStatePublisher;   
            Subscribe();
            SetDayIndex(0);
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _outOfFishEventSubscriber
                .Subscribe(_ => OnOutOfFish())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        private void OnOutOfFish()
        {
            DebugUtils.Log("Out of fish, moving to next room.");
            ChangeRoom(1);
        }
        private void NotifyStateChanged()
        {
            _dayStatePublisher.Publish(
                new DayStateChangedEvent(
                    CurrentDayIndex,
                    CurrentRoomIndex,
                    RoomHistory[^1],
                    CurrentDayPhase
                )
            );
            
        }
        
        public void Dispose()
        {
            _subscriptions?.Dispose();
        }
        
        #region Day Control
        /// <summary>
        /// Set current day index and reset room index to 0
        /// </summary>
        /// <param name="day"></param>
        public void SetDayIndex(uint day)
        {
            CurrentDayIndex = day;
            RoomHistory.Clear();
            SetRoomIndex(0);
            NotifyStateChanged();

        }
        
        /// <summary>
        /// Change current day index by given value and reset room index to 0
        /// </summary>
        /// <param name="day"></param>
        public void ChangeDayIndex(int day)
        {
            CurrentDayIndex += (uint)day;
            CurrentDayIndex = (uint)Mathf.Clamp(CurrentDayIndex, 0, _config.MaxDayCount - 1);
            RoomHistory.Clear();
            SetRoomIndex(0);
            NotifyStateChanged();

        }
        #endregion
        
        #region Room Control
        /// <summary>
        /// Set current room index
        /// </summary>
        /// <param name="room"></param>
        public void SetRoomIndex(uint room)
        {
            CurrentRoomIndex = room;
            RandomRoom();
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
            RandomRoom();
            SetDayPhase();
            NotifyStateChanged();

        }

        private void RandomRoom()
        {
            //var randomType = EnumUtils.RandomValue<RoomType>();
            RoomHistory.Add(RoomType.Fishing); // For now, only fishing room is available
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
        #endregion
        
        #region Utils

        public uint GetMaxFishCount()
        {
            if (_config.FishCountFormulas.TryGetValue(CurrentDayIndex, out var formula))
            {
                var fishingRoomCount = RoomHistory.FindAll(room => room == RoomType.Fishing).Count;
                if (fishingRoomCount != 0) return formula.Calculate((uint)(fishingRoomCount - 1));
                DebugUtils.LogWarning($"No fishing room visited yet on day {CurrentDayIndex}, using default value 1.");
                return 1;
            }
            DebugUtils.LogWarning($"No fish count formula found for day {CurrentDayIndex}, using default value 1.");
            return 1;
        }
        #endregion
    }
}