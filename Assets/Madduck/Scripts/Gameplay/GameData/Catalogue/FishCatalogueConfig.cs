using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "FishCatalogueConfig", menuName = "Madduck/Catalogue/FishCatalogueConfig", order = 0)]
    public class FishCatalogueConfig : SerializedScriptableObject
    {
        // [OdinSerialize] private HashSet<FishItemData> allFishItems = new();
        // public IReadOnlyCollection<FishItemData> AllFishItems => allFishItems;
    }
}