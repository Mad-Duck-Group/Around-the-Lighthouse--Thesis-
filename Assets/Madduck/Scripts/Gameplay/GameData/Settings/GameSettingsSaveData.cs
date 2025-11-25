using System;
using Madduck.Save;
using MessagePack;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    [MessagePackObject]
    public class GameSettingsSaveData : IMessagePackSaveData
    {
        [Key("Version")]
        [field: SerializeField] public string Version { get; set; } = string.Empty;
        
        [Key("ControlSettings")]
        [field: SerializeField] public ControlSettingsSaveData ControlSettings { get; set; } = new();
    }

    [Serializable]
    [MessagePackObject]
    public class ControlSettingsSaveData
    {
        [Key("FishingBoardMouseSensitivity")]
        [field: SerializeField] public float FishingBoardMouseSensitivity { get; set; } = 300f;
        
        [Key("FishingBoardGamepadSensitivity")]
        [field: SerializeField] public float FishingBoardGamepadSensitivity { get; set; } = 300f;
    }
    
    public class ControlSettings
    {
        public float FishingBoardMouseSensitivity { get; private set; } = 500f;
        public float FishingBoardGamepadSensitivity { get; private set; } = 500f;
        
        public void LoadFromSaveData(ControlSettingsSaveData saveData)
        {
            FishingBoardMouseSensitivity = saveData.FishingBoardMouseSensitivity;
            FishingBoardGamepadSensitivity = saveData.FishingBoardGamepadSensitivity;
        }
        
        public void SaveToSaveData(ControlSettingsSaveData saveData)
        {
            saveData ??= new ControlSettingsSaveData();
            saveData.FishingBoardMouseSensitivity = FishingBoardMouseSensitivity;
            saveData.FishingBoardGamepadSensitivity = FishingBoardGamepadSensitivity;
        }
    }
}