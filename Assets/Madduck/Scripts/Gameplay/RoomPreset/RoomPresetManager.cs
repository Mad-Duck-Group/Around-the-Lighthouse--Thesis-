using System.Collections.Generic;
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
         ShowInInspector] private  List<global::Madduck.RoomPreset.RoomPreset> _presets;

        #endregion
        
        #region Inject
        [Inject]
        public RoomPresetManager(List<global::Madduck.RoomPreset.RoomPreset> presets)
        {
            _presets = presets;
        }
        #endregion

        
        #region Lifecycle

        public void Start()
        {
            SpawnRandomRoom(Vector3.zero, Quaternion.identity);
        }

        #endregion

        
        #region RoomManagement

        public global::Madduck.RoomPreset.RoomPreset SpawnRandomRoom(Vector3 pos, Quaternion rot)
        {
            int index = Random.Range(0, _presets.Count);
            global::Madduck.RoomPreset.RoomPreset instance = Object.Instantiate(_presets[index], pos, rot);
            
            instance.ApplySprites();
            return instance;
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
