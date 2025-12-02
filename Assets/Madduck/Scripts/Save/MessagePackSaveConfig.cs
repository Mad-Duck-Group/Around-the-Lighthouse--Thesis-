using System.Collections.Generic;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Save
{
    [CreateAssetMenu(fileName = "MessagePackSaveConfig", menuName = "Madduck/Save/MessagePackSaveConfig", order = 0)]
    public class MessagePackSaveConfig : SerializedScriptableObject
    {
        [field: SerializeField] public bool LoadAtStart { get; private set; } = true;
        [field: SerializeField] private SerializableDictionary<string, MessagePackSaveObject> initialSaveObjects = new();
        public IReadOnlyDictionary<string, MessagePackSaveObject> InitialSaveObjects => (Dictionary<string, MessagePackSaveObject>)initialSaveObjects;
        [SerializeField] private bool debugMode = true;
        [ShowIf(nameof(debugMode)), 
         SerializeField] private SaveSettings debugSaveSettings;
        [HideIf(nameof(debugMode)),
         SerializeField] private SaveSettings releaseSaveSettings;
        
        public SaveSettings CurrentSaveSettings
        {
            get
            { 
#if UNITY_EDITOR
                return debugMode ? debugSaveSettings : releaseSaveSettings; 
#else
                return releaseSaveSettings; 
#endif
            }
        }
    }
}