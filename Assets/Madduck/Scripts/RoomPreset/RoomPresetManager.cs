using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Madduck.Scripts.RoomGenerate
{
    public class RoomPresetManager : MonoBehaviour
    {
        #region Inspactor

        [Title("Debug"),BoxGroup("Debug"),HideLabel,ReadOnly,SerializeField] 
        private  List<RoomPreset> _presets;

        #endregion
        
        #region Inject
        [Inject]
        public void Construct(List<RoomPreset> presets)
        {
            _presets = presets;
        }
        #endregion

        #region lifecycle

        private void OnEnable()
        {
            SpawnRandomRoom(Vector3.zero, Quaternion.identity);
        }

        #endregion

        #region RoomManage

        public RoomPreset SpawnRandomRoom(Vector3 pos, Quaternion rot)
        {
            int index = Random.Range(0, _presets.Count);
            RoomPreset instance = Instantiate(_presets[index], pos, rot);
            
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
