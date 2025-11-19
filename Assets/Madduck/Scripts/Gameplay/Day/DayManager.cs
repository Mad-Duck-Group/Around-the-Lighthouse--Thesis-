using System;
using System.Collections.Generic;
using Madduck.Core;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Madduck.Day
{
    
    public class DayManager : IMaxFishCountProvider, IDisposable
    {
        #region Inspector

        [Title("Debug"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugTitle;

        [field: DisplayAsString,
                ShowInInspector]public ReactiveProperty<uint> CurrentDayIndex { get; private set; } = new(0);
        [field: DisplayAsString, 
                ShowInInspector] public ReactiveProperty<uint> CurrentRoomIndex { get; private set; } = new(0);
        [field: DisplayAsString, 
                ShowInInspector] public DayPhaseType CurrentDayPhase { get; private set; } = DayPhaseType.Day;

        [field: DisplayAsString,
                ShowInInspector] public List<RoomType> RoomHistory { get; private set; } = new();

        [Button("Next Room")]
        private void NextRoom() => OnOutOfFish();

        [Button("Next Day")]
        private void NextDay()
        {
            ChangeDayIndex(1);
            _loadSceneManager.LoadScene(SceneType.Gameplay, LoadSceneMode.Single, true).Forget();
        }

        #endregion

        #region Fields

        private readonly CompositeWeightTableInstance _fishableWeightTable;
        private readonly DayManagerConfig _config;
        private readonly ISubscriber<FishingRoomEndedEvent> _outOfFishEventSubscriber;
        private readonly IPublisher<DayStateChangedEvent> _dayStatePublisher;
        private readonly LoadSceneManager _loadSceneManager;
        private IDisposable _subscriptions;

        #endregion

        #region Injection

        [Inject]
        public DayManager(
            [Key(ModifierKeys.FishableKey)] CompositeWeightTableInstance fishableWeightTable,
            DayManagerConfig config,
            LoadSceneManager loadSceneManager,
            ISubscriber<FishingRoomEndedEvent> outOfFishEventSubscriber)
        {
            _fishableWeightTable = fishableWeightTable;
            _config = config;
            _outOfFishEventSubscriber = outOfFishEventSubscriber;
            _loadSceneManager = loadSceneManager;
            Subscribe();
            SetDayIndex(0);
        }

        #endregion

        #region Subscription

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _outOfFishEventSubscriber
                .Subscribe(_ => OnOutOfFish())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions?.Dispose();
        }

        #endregion

        #region Events

        private void OnOutOfFish()
        {
            DebugUtils.Log("Out of fish, moving to next room.");
            ChangeRoom(1);
            if (CurrentRoomIndex.CurrentValue >= _config.MaxRoomCount)
            {
                ChangeDayIndex(1);
            }
            
            _loadSceneManager.LoadScene(SceneType.Gameplay, LoadSceneMode.Single, true).Forget();
        }

        #endregion
        
        #region Day Control
        /// <summary>
        /// Set current day index and reset room index to 0
        /// </summary>
        /// <param name="day"></param>
        public void SetDayIndex(uint day)
        {
            CurrentDayIndex.Value = day;
            RoomHistory.Clear();
            SetRoomIndex(0);

        }
        
        /// <summary>
        /// Change current day index by given value and reset room index to 0
        /// </summary>
        /// <param name="day"></param>
        public void ChangeDayIndex(int day)
        {
            CurrentDayIndex.Value += (uint)day;
            CurrentDayIndex.Value = (uint)Mathf.Clamp(CurrentDayIndex.Value, 0, _config.MaxDayCount - 1);
            RoomHistory.Clear();
            SetRoomIndex(0);

        }
        #endregion
        
        #region Room Control
        /// <summary>
        /// Set current room index
        /// </summary>
        /// <param name="room"></param>
        public void SetRoomIndex(uint room)
        {
            CurrentRoomIndex.Value = room;
            RandomRoom();
            SetDayPhase();
        }
        
        /// <summary>
        /// Change current room index by given value
        /// </summary>
        /// <param name="room"></param>
        public void ChangeRoom(int room)
        {
            
            CurrentRoomIndex.Value += (uint)room;
            CurrentDayIndex.Value = (uint)Mathf.Clamp(CurrentDayIndex.Value, 0, _config.MaxRoomCount - 1);
            RandomRoom();
            SetDayPhase();
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
            var percent = Percentage.FromFraction((float)CurrentRoomIndex.Value / (_config.MaxRoomCount - 2)); //ignore last room
            CurrentDayPhase = percent <= _config.DayNightRatio ? DayPhaseType.Day : DayPhaseType.Night;
            FilterFishByDayPhase();
        }
        
        /// <summary>
        /// Filter fish weight table by current day phase
        /// </summary>
        private void FilterFishByDayPhase()
        {
            if (!_fishableWeightTable.TryGetFirstInstanceOfType<FishWeightTableInstance>(out var fishWeightTableInstance)) return;
            if (fishWeightTableInstance == null) return;
            fishWeightTableInstance.PersistentFilters.Remove("DayPhaseFilter");
            var filter = new FishWeightFilter(record => record.Item.DayPhaseType.HasFlag(CurrentDayPhase));
            fishWeightTableInstance.PersistentFilters.TryAdd("DayPhaseFilter", filter);
        }
        #endregion
        
        #region Utils

        public uint GetMaxFishCount()
        {
            if (_config.FishCountFormulas.TryGetValue(CurrentDayIndex.Value, out var formula))
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
    
    
    [Serializable]
    public class DayModifierContextProvider : IModifierContextProvider
    {
        [SerializeField] private int everyDay = 1;
        private DayManager _dayManager;

        public void Inject(IObjectResolver objectResolver)
        {
            objectResolver.TryResolve(out _dayManager);
        }
        
        public bool TryGetEvaluationParameter(ModifierValueType modifierValueType, out float parameter)
        {
            parameter = 0;
            if (_dayManager == null) return false;
            if (everyDay <= 0) return false;
            switch (modifierValueType)
            {
                case ModifierValueType.Constant:
                case ModifierValueType.Curve:
                case ModifierValueType.Step:
                    return false;
                case ModifierValueType.Incremental:
                    parameter = Mathf.Floor(_dayManager.CurrentDayIndex.Value / (float)everyDay);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifierValueType), modifierValueType, null);
            }
            return true;
        }
    }
    
    [Serializable]
    public class RoomModifierContextProvider : IModifierContextProvider
    {
        [SerializeField] private int everyRoom = 1;
        private DayManager _dayManager;

        public void Inject(IObjectResolver objectResolver)
        {
            objectResolver.TryResolve(out _dayManager);
        }
        
        public bool TryGetEvaluationParameter(ModifierValueType modifierValueType, out float parameter)
        {
            parameter = 0;
            if (_dayManager == null) return false;
            if (everyRoom <= 0) return false;
            switch (modifierValueType)
            {
                case ModifierValueType.Constant:
                case ModifierValueType.Curve:
                case ModifierValueType.Step:
                    return false;
                case ModifierValueType.Incremental:
                    parameter = Mathf.Floor(_dayManager.CurrentRoomIndex.Value / (float)everyRoom);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifierValueType), modifierValueType, null);
            }
            return true;
        }
    }
}