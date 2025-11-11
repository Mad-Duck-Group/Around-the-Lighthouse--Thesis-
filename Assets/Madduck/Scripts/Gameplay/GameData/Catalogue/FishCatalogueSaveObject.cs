using Madduck.Core;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "FishCatalogueSaveObject", menuName = "Madduck/Catalogue/FishCatalogueSaveObject", order = 0)]
    public class FishCatalogueSaveObject : MessagePackSaveObject<FishCatalogueSaveData>
    {
        
    }
}