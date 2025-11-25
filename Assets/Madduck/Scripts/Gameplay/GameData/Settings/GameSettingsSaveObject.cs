using Madduck.Save;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "GameSettingsSaveObject", menuName = "Madduck/Settings/GameSettingsSaveObject", order = 0)]
    public class GameSettingsSaveObject : MessagePackSaveObject<GameSettingsSaveData>
    {
        
    }
}