using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Madduck.Utils;
using MessagePack;
using MessagePack.Resolvers;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.Core
{
    [MessagePackObject]
    [Union(0, typeof(TestMessagePackSaveData))]
    [Serializable]
    public abstract class MessagePackSaveData
    {
        [Key("Version")] 
        public VersionInfo version;
    }
    
    public interface ISaveMigrationResolver<out T>
        where T : MessagePackSaveData
    {
        VersionInfo SourceVersion { get; }
        VersionInfo TargetVersion { get; }
        ExpandoObject Migrate(ExpandoObject fullResolve);
        T Finalize(ExpandoObject expando);
    }

    public abstract class MessagePackSaveObject : SerializedScriptableObject
    {
        [Title("Settings")] 
        [InfoBox("Settings mark in red are destructive after release.\n" +
                 "Changing these settings will make existing saves incompatible!\n" +
                 "Make sure they are definite before release.", InfoMessageType.Warning),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _warning;
        [field: GUIColor("red"),
            SerializeField] public bool SaveSeparately { get; private set; } = false;
        [GUIColor("red"), 
         SerializeField] protected bool saveAsJson = false;
        [SerializeField] protected bool debugMode = true;
        [ShowIf(nameof(debugMode)),
         SerializeField] protected SaveSettings debugSaveSettings;
        [HideIf(nameof(debugMode)), GUIColor("red"),
         SerializeField] protected SaveSettings releaseSaveSettings;
        
        public abstract T GetSaveData<T>() where T : MessagePackSaveData;

        public abstract byte[] SerializeSaveData();
        
        public abstract void Save();
        
        public abstract void Load();
        
        public abstract void LoadFromBytes(byte[] bytes);
        
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
    
    public abstract class MessagePackSaveObject<T> : MessagePackSaveObject
        where T : MessagePackSaveData
    {
        [Title("Save Management")]
        [OdinSerialize] protected T saveData;
        [OdinSerialize] protected List<ISaveMigrationResolver<T>> migrationResolvers = new();

        public override TDerived GetSaveData<TDerived>()
        {
            if (saveData is TDerived derivedData)
            {
                return derivedData;
            }
            throw new InvalidCastException($"Cannot cast save data of type {typeof(T)} to {typeof(TDerived)}.");
        }

        [Title("Debug"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugTitle;
        [ButtonGroup("Save&Load")]
        [Button("Test Save", ButtonSizes.Large)]
        protected virtual void TestSave()
        {
            SaveInternal();
        }
        
        [ButtonGroup("Save&Load")]
        [Button("Test Load", ButtonSizes.Large)]
        protected virtual void TestLoad()
        {
            LoadInternal();
        }
        
        [ButtonGroup("JSON")]
        [Button("Export JSON", ButtonSizes.Large)]
        protected void ExportJson()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.SaveFilePanel("Export Save Data to JSON", "", 
                $"{CurrentSaveSettings.saveFileName}_export", "json");
            if (string.IsNullOrEmpty(path)) return;
#else
            var path = Path.Combine(Application.persistentDataPath,
                $"{CurrentSaveSettings.saveFileName}_export.json");
#endif
            var json = MessagePackSerializer.ConvertToJson(
                MessagePackSerializer.Serialize(saveData, ContractlessStandardResolver.Options));
            var beautifiedJson = JsonUtils.BeautifyJson(json);
            var jsonPath = Path.ChangeExtension(path, "json");
            File.WriteAllText(jsonPath, beautifiedJson);
            Debug.Log($"Dumped save data to JSON at: {jsonPath}");
        }
        
        [ButtonGroup("JSON")]
        [Button("Import JSON", ButtonSizes.Large)]
        protected void ImportJson()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Select JSON Save File", "", "json");
            if (string.IsNullOrEmpty(path)) return;
            var json = File.ReadAllText(path);
            var bytes = MessagePackSerializer.ConvertFromJson(json);
            LoadFromBytes(bytes);
#else
            Debug.LogWarning("ImportJson is only available in the Unity Editor.");
#endif
        }
        
        public override byte[] SerializeSaveData()
        {
            return MessagePackSerializer.Serialize(saveData, ContractlessStandardResolver.Options);
        }

        public override void Save() 
        {
            if (!SaveSeparately)
            {
                Debug.LogError("SaveSeparately is false. Save operation must be handled by Save Manager.");
                return;
            } 
            SaveInternal();
        }

        private void SaveInternal()
        {
            var bytes = SerializeSaveData();
            WriteToFile(bytes);
        }
        
        public override void Load()
        {
            if (!SaveSeparately)
            {
                Debug.LogError("SaveSeparately is false. Load operation must be handled by Save Manager.");
                return;
            } 
            LoadInternal();
        }
        
        private void LoadInternal()
        {
            var bytes = ReadFromFile();
            if (bytes == null)
            {
                Debug.LogError("No save file found to load.");
                return;
            }
            LoadFromBytes(bytes);
        }
        
        public override void LoadFromBytes(byte[] bytes)
        {
            if (TryMigrateSave(bytes, out var migratedSave))
            {
                saveData = migratedSave;
                Debug.Log("Save data loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to migrate save data. Load aborted.");
            }
        }

        protected virtual bool TryMigrateSave(byte[] bytes, out T migratedSave)
        {
            migratedSave = MessagePackSerializer.Deserialize<T>(bytes, ContractlessStandardResolver.Options);
            if (migratedSave.version == saveData.version)
            {
                Debug.Log("Save version matches current version. No migration needed.");
                return true;
            }
            Debug.Log($"Save version ({migratedSave.version}) is different from current version ({saveData.version}). Attempting migration.");
            if (!TryFindShortestMigrationPath(migratedSave.version, saveData.version, out var path))
            {
                Debug.LogWarning($"No migration path found from version {migratedSave.version} to {saveData.version}. Migration aborted.");
                return false;
            }
            var expando = MessagePackSerializer.Deserialize<ExpandoObject>(bytes, ExpandoObjectResolver.Options);
            foreach (var resolver in path)
            {
                expando = resolver.Migrate(expando);
                Debug.Log($"Migration successful from version {resolver.SourceVersion} to {resolver.TargetVersion}.");
            }
            Debug.Log("All migrations completed.");
            migratedSave = path.Last().Finalize(expando);
            return true;
        }

        private bool TryFindShortestMigrationPath(VersionInfo sourceVersion, VersionInfo targetVersion, out List<ISaveMigrationResolver<T>> path)
        {
            path = new List<ISaveMigrationResolver<T>>();
            if (migrationResolvers == null)
                throw new ArgumentNullException(nameof(migrationResolvers));

            if (sourceVersion == null)
                throw new ArgumentNullException(nameof(sourceVersion));

            if (targetVersion == null)
                throw new ArgumentNullException(nameof(targetVersion));

            if (sourceVersion == targetVersion)
                return false; // No migration needed

            // Build adjacency list
            var graph = migrationResolvers
                .GroupBy(r => r.SourceVersion)
                .ToDictionary(g => g.Key, g => g.ToList());

            // BFS setup
            var queue = new Queue<MigrationPath>();
            var visited = new Dictionary<VersionInfo, MigrationPath>();

            var initialPath = new MigrationPath(sourceVersion, new List<ISaveMigrationResolver<T>>());
            queue.Enqueue(initialPath);
            visited[sourceVersion] = initialPath;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (!graph.TryGetValue(current.Version, out var value1))
                    continue;

                foreach (var resolver in value1)
                {
                    VersionInfo nextVersion = resolver.TargetVersion;

                    // Skip if we've found a better path to this version already
                    if (visited.ContainsKey(nextVersion) &&
                        visited[nextVersion].Path.Count <= current.Path.Count + 1)
                    {
                        continue;
                    }

                    var newPath = new List<ISaveMigrationResolver<T>>(current.Path) { resolver };
                    var newMigrationPath = new MigrationPath(nextVersion, newPath);

                    visited[nextVersion] = newMigrationPath;

                    if (nextVersion == targetVersion)
                    {
                        // We found a path, but continue to check if there's a shorter one
                        continue;
                    }

                    queue.Enqueue(newMigrationPath);
                }
            }

            path = visited.TryGetValue(targetVersion, out var value) ? value.Path : null;
            return path != null;
        }

        protected virtual void WriteToFile(byte[] bytes)
        {
            var fullPath = CurrentSaveSettings.GetFullSavePath();
            if (saveAsJson) 
            {
                var json = MessagePackSerializer.ConvertToJson(bytes);
                var beautifiedJson = JsonUtils.BeautifyJson(json);
                File.WriteAllText(fullPath, beautifiedJson);
                return;
            }
            File.WriteAllBytes(fullPath, bytes);
        }
        
        protected virtual byte[] ReadFromFile()
        {
            var fullPath = CurrentSaveSettings.GetFullSavePath();
            if (!saveAsJson) return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
            if (!File.Exists(fullPath)) return null;
            var json = File.ReadAllText(fullPath);
            return MessagePackSerializer.ConvertFromJson(json);
        }
        
        // Helper class to track paths during BFS
        private class MigrationPath
        {
            public VersionInfo Version { get; }
            public List<ISaveMigrationResolver<T>> Path { get; }
    
            public MigrationPath(VersionInfo version, List<ISaveMigrationResolver<T>> path)
            {
                Version = version;
                Path = path;
            }
        }
    }
}