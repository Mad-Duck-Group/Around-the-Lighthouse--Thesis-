using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    [CreateAssetMenu(fileName = "FishRegistry", menuName = "Madduck/Catalogue/FishRegistry", order = 0)]
    public class FishRegistry : SerializedScriptableObject
    {
        public static FishRegistry Instance { get; private set; }
        
        [Title("Fish Registry")]
        [OdinSerialize] private HashSet<FishItemData> allFishItemData = new();
        public IReadOnlyCollection<FishItemData> AllFishItemData => allFishItemData;
        
#if UNITY_EDITOR
        [HideInEditorMode,
         MenuItem("ATL/Fish Registry")]
        public static void OpenFishRegistryWindow()
        {
            if (!Instance)
            {
                Debug.LogWarning("FishRegistry instance is null. Creating a new one");
                var asset = CreateInstance<FishRegistry>();
                AssetDatabase.CreateAsset(asset, "Assets/Madduck/Resources/FishRegistry.asset");
                AssetDatabase.SaveAssets();
                Instance = asset;
            }
            Sirenix.OdinInspector.Editor.OdinEditorWindow.InspectObject(Instance);
        }
#endif
        
        public static FishItemData GetFishItemDataByGuid(Guid id)
        {
            return Instance.AllFishItemData.FirstOrDefault(fishItemData => fishItemData.Guid.Equals(id));
        }
        
        private void OnEnable()
        {
            Instance = this;
        }
    }
}