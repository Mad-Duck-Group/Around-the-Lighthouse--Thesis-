using System.Collections.Generic;
using Madduck.Day;
using Madduck.WeatherPreset;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Madduck.RoomPreset
{
    public class RoomPresetManager : IStartable
    {
        #region Inspector 
        
        [Title("Debug"),
         BoxGroup("Debug"),
         HideLabel, Sirenix.OdinInspector.ReadOnly,
         ShowInInspector] private  List<RoomPreset> _presets;

        public ReadOnlyReactiveProperty<RoomPreset> CurrentRoomPreset =>
            _currentRoomPreset.Select(x => x).ToReadOnlyReactiveProperty();

        #endregion
        
        private readonly ReactiveProperty<RoomPreset> _currentRoomPreset = new();
        private readonly DayManager _dayManager;
        private readonly WeatherPresetManager _weatherPresetManager;  
        
        #region Inject
        [Inject]
        public RoomPresetManager(
            List<RoomPreset> presets,
            DayManager dayManager,
            WeatherPresetManager weatherPresetManager)
        {
            _presets = presets;
            _dayManager = dayManager;
            _weatherPresetManager = weatherPresetManager;
        }
        #endregion
        
        #region Lifecycle

        public void Start()
        {
            SpawnRandomRoom(Vector3.zero, Quaternion.identity);
        }

        #endregion

        
        #region RoomManagement

        private void SpawnRandomRoom(Vector3 pos, Quaternion rot)
        {
            var instance = Object.Instantiate(_presets.GetRandomElement(), pos, rot);
            instance.SetDayPhase(_dayManager.CurrentDayPhase);
            instance.SetDynamicElements(_weatherPresetManager.CurrentWeather.Value);
            instance.ApplySprites();
            instance.ApplyAnimation();
            _currentRoomPreset.Value = instance;
        }

        #endregion
    }
}
