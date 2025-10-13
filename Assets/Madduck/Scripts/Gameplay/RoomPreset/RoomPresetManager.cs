using System.Collections.Generic;
using Madduck.Day;
using Madduck.WeatherPreset;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Madduck.RoomPreset
{
    public class RoomPresetManager : IStartable
    {
        
        #region Inspector 
        
        [Title("Debug"),
         BoxGroup("Debug"),
         HideLabel, ReadOnly,
         ShowInInspector] private  List<RoomPreset> _presets;

        #endregion
        
        private readonly DayManager _dayManager;
        private readonly  WeatherPresetManager _weatherPresetManager;      
        #region Inject
        [Inject]
        public RoomPresetManager(
            List<RoomPreset> presets,
            DayManager dayManager,WeatherPresetManager weatherPresetManager)
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

        public void SpawnRandomRoom(Vector3 pos, Quaternion rot)
        {
            int index = Random.Range(0, _presets.Count);
            RoomPreset instance = Object.Instantiate(_presets[index], pos, rot);
            
            instance.SetDayPhase(_dayManager.CurrentDayPhase);
            instance.SetDynamicElements(_weatherPresetManager.CurrentWeather);
            instance.ApplySprites();
        }

        #endregion
        

        // #if UNITY_EDITOR
        //         
        //         [Button("Save Current Room As Prefab")]
        //         public void SaveRoomPrefab()
        //         {
        //             foreach (var preset in roomPresets)
        //             {
        //                 if (preset == null) continue;
        //
        //                 string path = $"Assets/Prefabs/Rooms/{preset.presetName}.prefab";
        //                 PrefabUtility.SaveAsPrefabAssetAndConnect(
        //                     preset.gameObject, 
        //                     path, 
        //                     InteractionMode.UserAction
        //                 );
        //                 Debug.Log($"Room prefab saved at {path}");
        //             }
        //         }
        // #endif

        
    }
}
