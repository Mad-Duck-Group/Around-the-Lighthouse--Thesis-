using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Core;
using MessagePack;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.GameData
{
    [Serializable]
    [MessagePackObject]
    public record FishCatalogueEntry
    {
        [MessagePack.Key("IsCaught")]
        [field: SerializeField] public bool IsCaught {get; set;}
    }
    
    [Serializable]
    public class FishCatalogue : IPostInitializable
    {
        private readonly FishCatalogueConfig _config;
        private readonly MessagePackSaveManager _saveManager;
        private FishCatalogueSaveObject _fishCatalogueSaveObject;
        private FishCatalogueSaveData _fishCatalogueData;
        
        private Dictionary<Guid, FishCatalogueEntry> _fishCatalogueEntries = new();
        [OdinSerialize] public IReadOnlyDictionary<Guid, FishCatalogueEntry> FishCatalogueEntries => _fishCatalogueEntries;
        
        [Inject]
        public FishCatalogue(
            FishCatalogueConfig config,
            MessagePackSaveManager saveManager)
        {
            _config = config;
            _saveManager = saveManager;
        }

        public void PostInitialize()
        {
            _fishCatalogueSaveObject = _saveManager.GetFirstSaveObjectOfType<FishCatalogueSaveObject>();
            _fishCatalogueData = _fishCatalogueSaveObject.GetSaveData<FishCatalogueSaveData>();
            _fishCatalogueEntries = FishRegistry.Instance.AllFishItemData.ToDictionary(x => x.Guid, _ => new FishCatalogueEntry());
            Load();
        }

        public void Save()
        {
            _fishCatalogueData.FishCatalogueEntries = _fishCatalogueEntries.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _saveManager.Save(_fishCatalogueSaveObject);
        }

        private void Load()
        {
            foreach (var kvp in _fishCatalogueData.FishCatalogueEntries)
            {
                if (_fishCatalogueEntries.ContainsKey(kvp.Key))
                {
                    _fishCatalogueEntries[kvp.Key] = kvp.Value;
                }
            }
        }

        public bool HasCaught(Guid guid)
        {
            return _fishCatalogueEntries.TryGetValue(guid, out var entry) && entry.IsCaught;
        }
        
        public void SetCaught(Guid guid)
        {
            if (!_fishCatalogueEntries.TryGetValue(guid, out var entry)) return;
            entry.IsCaught = true;
            Save();
        }
    }
}