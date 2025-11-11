using System;
using System.Collections.Generic;
using Madduck.Core;
using MessagePack;
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
    }
}