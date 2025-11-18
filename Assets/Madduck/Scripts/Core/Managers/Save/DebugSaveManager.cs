using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Madduck.Core
{
    [CreateAssetMenu(fileName = "DebugSaveManager", menuName = "Madduck/Core/DebugSaveManager", order = 0)]
    [ShowOdinSerializedPropertiesInInspector]
    public class DebugSaveManager : SerializedScriptableObject
    {
        public static DebugSaveManager Instance { get; private set; }
        
        [SerializeField] private MessagePackSaveConfig saveConfig;
        [OdinSerialize] private MessagePackSaveManager _saveManager;

        public MessagePackSaveManager SaveManager
        {
            get
            {
                if (_saveManager == null)
                {
                    InitializeSaveManager();
                }
                return _saveManager;
            }
        }

        [Button("Initialize Save Manager")]
        public void InitializeSaveManager()
        {
            if (!saveConfig)
            {
                Debug.LogError("Save Config is not assigned.");
                return;
            }
            _saveManager = new MessagePackSaveManager(saveConfig);
            Debug.Log("Save Manager initialized.");
        }
        
#if UNITY_EDITOR
        [MenuItem("ATL/Save Manager")]
        public static void OpenFishRegistryWindow()
        {
            if (!Instance)
            {
                Debug.LogWarning("FishRegistry instance is null. Creating a new one");
                var asset = CreateInstance<DebugSaveManager>();
                AssetDatabase.CreateAsset(asset, "Assets/Madduck/Resources/DebugSaveManager.asset");
                AssetDatabase.SaveAssets();
                Instance = asset;
            }
            Sirenix.OdinInspector.Editor.OdinEditorWindow.InspectObject(Instance);
        }
#endif
        
        private void OnEnable()
        {
            Instance = this;
        }
        
    }
}