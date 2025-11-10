using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Madduck.Core
{
    [Serializable]
    public class MessagePackSaveManager : IPostInitializable
    {
        private readonly MessagePackSaveConfig _config;
        [ShowInInspector, ReadOnly] private Dictionary<Type, MessagePackSaveObject> _saveObjects = new();
        
        [Button("Test Save All")]
        private void TestSaveAll()
        {
            SaveAll();
        }

        [Button("Test Load All")]
        private void TestLoadAll()
        {
            LoadAll();
        }
        
        [Inject]
        public MessagePackSaveManager(MessagePackSaveConfig config)
        {
            _config = config;
            foreach (var saveObject in _config.InitialSaveObjects)
            {
                RegisterSaveObject(saveObject);
            }
        }
        
        public void PostInitialize()
        {
            if (!_config.LoadAtStart) return;
            LoadAll();
        }
        
        public void RegisterSaveObject<T>(MessagePackSaveObject<T> saveObject)
            where T : MessagePackSaveData
        {
            var type = typeof(T);
            _saveObjects[type] = saveObject;
        }
        
        public void RegisterSaveObject(MessagePackSaveObject saveObject)
        {
            var type = saveObject.GetType();
            _saveObjects[type] = saveObject;
        }
        
        public void UnregisterSaveObject<T>() 
            where T : MessagePackSaveData
        {
            var type = typeof(T);
            _saveObjects.Remove(type);
        }
        
        public void UnregisterSaveObject(Type type)
        {
            _saveObjects.Remove(type);
        }

        public MessagePackSaveObject<T> GetSaveObject<T>() where T : MessagePackSaveData
        {
            var type = typeof(T);
            if (_saveObjects.TryGetValue(type, out var saveObject))
            {
                return saveObject as MessagePackSaveObject<T>;
            }
            Debug.LogError($"Save object of type {type} not found.");
            return null;
        }
        
        public void LoadAll()
        {
            foreach (var saveObject in _saveObjects.Values)
            {
                Load(saveObject);
            }
        }

        public void SaveAll()
        {
            foreach (var saveObject in _saveObjects.Values)
            {
                Save(saveObject);
            }
        }
        
        public void Save<T>() where T : MessagePackSaveData
        {
            var saveObject = GetSaveObject<T>();
            Save(saveObject);
        }

        public void Save(MessagePackSaveObject saveObject)
        {
            if (!saveObject)
                return;
            if (saveObject.SaveSeparately)
            {
                saveObject.Save();
                return;
            }
            var name = saveObject.CurrentSaveSettings.saveFileName;
            ZipAndSave(name, saveObject.SerializeSaveData());
        }
        
        public void Load<T>() where T : MessagePackSaveData
        {
            var saveObject = GetSaveObject<T>();
            Load(saveObject);
        }

        public void Load(MessagePackSaveObject saveObject)
        {
            if (!saveObject)
                return;
            if (saveObject.SaveSeparately)
            {
                saveObject.Load();
                return;
            }
            var name = saveObject.CurrentSaveSettings.saveFileName;
            var data = LoadFromZip(name);
            if (data != null)
            {
                saveObject.LoadFromBytes(data);
            }
        }
        
        private void ZipAndSave(string entryName, byte[] data)
        {
            var zipPath = Path.ChangeExtension(_config.CurrentSaveSettings.GetFullSavePath(), ".sav");

            try
            {
                using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
                {
                    var finalName = Path.ChangeExtension(entryName, ".bin");
                    if (zipArchive.GetEntry(finalName) != null)
                    {
                        zipArchive.GetEntry(finalName)?.Delete();
                    }
                    var entry = zipArchive.CreateEntry(finalName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var writer = new BinaryWriter(entryStream);
                    writer.Write(data);
                }
                Debug.Log($"ZIP file created successfully: {zipPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating ZIP file: {ex.Message}");
                throw;
            }
        }
        
        private byte[] LoadFromZip(string entryName)
        {
            var zipPath = Path.ChangeExtension(_config.CurrentSaveSettings.GetFullSavePath(), ".sav");
            
            if (!File.Exists(zipPath))
                return null;
            
            try
            {
                using var zipArchive = ZipFile.OpenRead(zipPath);
                var finalName = Path.ChangeExtension(entryName, ".bin");
                var entry = zipArchive.GetEntry(finalName);
                if (entry == null)
                    return null;
                using var entryStream = entry.Open();
                using var reader = new BinaryReader(entryStream);
                Debug.Log($"ZIP file loaded successfully: {zipPath}");
                return reader.ReadBytes((int)entry.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading ZIP file: {ex.Message}");
                throw;
            }
        }
    }
}