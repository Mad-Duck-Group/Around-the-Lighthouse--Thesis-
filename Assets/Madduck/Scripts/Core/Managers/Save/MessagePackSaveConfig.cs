using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Core
{
    [CreateAssetMenu(fileName = "MessagePackSaveConfig", menuName = "Madduck/Save/MessagePackSaveConfig", order = 0)]
    public class MessagePackSaveConfig : SerializedScriptableObject
    {
        [field: SerializeField] public bool LoadAtStart { get; private set; } = true;
        [field: SerializeField] private List<MessagePackSaveObject> initialSaveObjects = new();
        public IReadOnlyList<MessagePackSaveObject> InitialSaveObjects => initialSaveObjects.AsReadOnly();
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