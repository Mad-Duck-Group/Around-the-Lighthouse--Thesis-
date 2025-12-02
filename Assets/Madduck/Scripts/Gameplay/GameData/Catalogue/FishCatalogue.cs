using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Core;
using Madduck.Save;
using MessagePack;
using MessagePipe;
using R3;
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
    public class FishCatalogue : IPostInitializable, IDisposable
    {
        private readonly FishCatalogueConfig _config;
        private readonly MessagePackSaveManager _saveManager;
        private readonly ISubscriber<FishingRoomStartedEvent> _fishingRoomStartedEventPublisher;
        private FishCatalogueSaveObject _fishCatalogueSaveObject;
        private FishCatalogueSaveData _fishCatalogueData;
        
        private Dictionary<Guid, FishCatalogueEntry> _fishCatalogueEntries = new();
        [OdinSerialize] public IReadOnlyDictionary<Guid, FishCatalogueEntry> FishCatalogueEntries => _fishCatalogueEntries;
        private IDisposable _disposables;
        
        [Inject]
        public FishCatalogue(
            FishCatalogueConfig config,
            MessagePackSaveManager saveManager,
            ISubscriber<FishingRoomStartedEvent> fishingRoomStartedEventPublisher)
        {
            _config = config;
            _saveManager = saveManager;
            _fishingRoomStartedEventPublisher = fishingRoomStartedEventPublisher;
            Subscribe();
        }

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishingRoomStartedEventPublisher
                .Subscribe(_ =>
                {
                    Load();
                })
                .AddTo(ref disposableBuilder);
            _disposables = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void PostInitialize()
        {
            _fishCatalogueSaveObject = _saveManager.GetFirstSaveObjectOfType<FishCatalogueSaveObject>();
            _fishCatalogueData = _fishCatalogueSaveObject.GetSaveData<FishCatalogueSaveData>();
            _fishCatalogueEntries = _config.FishRegistry.AllFishItemData.ToDictionary(x => x.Guid, _ => new FishCatalogueEntry());
            Load();
        }

        public void Save()
        {
            _fishCatalogueData.FishCatalogueEntries = _fishCatalogueEntries.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _saveManager.Save(_fishCatalogueSaveObject);
        }

        public void Load()
        {
            foreach (var kvp in _fishCatalogueData.FishCatalogueEntries)
            {
                if (_fishCatalogueEntries.ContainsKey(kvp.Key))
                {
                    _fishCatalogueEntries[kvp.Key] = kvp.Value;
                }
            }
        }

        public void Reset()
        {
            foreach (var entry in _fishCatalogueEntries.Values)
            {
                entry.IsCaught = false;
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