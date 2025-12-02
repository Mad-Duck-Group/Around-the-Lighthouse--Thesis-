using Madduck.Save;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "FishCatalogueSaveObject", menuName = "Madduck/Catalogue/FishCatalogueSaveObject", order = 0)]
    public class FishCatalogueSaveObject : MessagePackSaveObject<FishCatalogueSaveData>
    {
        public override void Reset()
        {
            base.Reset();
            saveData = new FishCatalogueSaveData
            {
                Version = string.Empty,
                FishCatalogueEntries = new()
            };
        }
    }
}