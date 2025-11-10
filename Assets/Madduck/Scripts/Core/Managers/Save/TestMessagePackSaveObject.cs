using System;
using System.Collections.Generic;
using System.Dynamic;
using MessagePack;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Core
{
    [MessagePackObject]
    [Serializable]
    public class TestMessagePackSaveData : MessagePackSaveData
    {
        [Key("TestInt")]
        public int testInt;
    }
    
    [Serializable]
    public class TestMessagePackMigrationResolver : ISaveMigrationResolver<TestMessagePackSaveData>
    {
        [field: HideReferenceObjectPicker, 
                SerializeField] public VersionInfo SourceVersion { get; private set; } = new VersionInfo();
        [field: HideReferenceObjectPicker, 
                SerializeField] public VersionInfo TargetVersion { get; private set; } = new VersionInfo();

        [SerializeField] private int additionalTestInt;

        public ExpandoObject Migrate(ExpandoObject expando)
        {
            IDictionary<string, object> dict = expando;
            dict["Version"] = TargetVersion;
            dict["TestInt"] = Convert.ToInt32(dict["TestInt"]) + additionalTestInt;
            return expando;
        }

        public TestMessagePackSaveData Finalize(ExpandoObject expando)
        {
            IDictionary<string, object> dict = expando;
            if (!VersionInfo.TryParse(dict["Version"].ToString(), out var version))
            {
                version = SourceVersion;
                Debug.LogWarning("Failed to parse version during finalization. Using source version as fallback.");
            }
            var final = new TestMessagePackSaveData
            {
                version = version,
                testInt = Convert.ToInt32(dict["TestInt"])
            };
            return final;
        }
    }
    
    [CreateAssetMenu(fileName = "TestMessagePackSaveObject", menuName = "Madduck/Save/TestMessagePackSaveObject", order = 0)]
    public class TestMessagePackSaveObject : MessagePackSaveObject<TestMessagePackSaveData>
    {
        
    }
}