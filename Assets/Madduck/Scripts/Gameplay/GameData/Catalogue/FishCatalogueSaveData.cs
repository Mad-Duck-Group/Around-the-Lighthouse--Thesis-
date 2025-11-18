using System;
using System.Collections.Generic;
using Madduck.Core;
using MessagePack;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    [MessagePackObject]
    public class FishCatalogueSaveData : IMessagePackSaveData
    {
        [Key("Version")]
        [field: SerializeField] public string Version { get; set; }
        
        [Key("FishCatalogueEntries")]
        [field: OdinSerialize] public Dictionary<Guid, FishCatalogueEntry> FishCatalogueEntries { get; set; }

        [IgnoreMember] 
        [ShowInInspector] private IReadOnlyDictionary<FishItemData, FishCatalogueEntry> _readableFishCatalogueEntries;

        [Button("Generate Readable Dictionary")]
        public void GenerateReadableDictionary()
        {
            var dict = new Dictionary<FishItemData, FishCatalogueEntry>();
            foreach (var entry in FishCatalogueEntries)
            {
                var fishItemData = FishRegistry.GetFishItemDataByGuid(entry.Key);
                if (fishItemData)
                {
                    dict[fishItemData] = entry.Value;
                }
            }
            _readableFishCatalogueEntries = dict;
        }
    }
}