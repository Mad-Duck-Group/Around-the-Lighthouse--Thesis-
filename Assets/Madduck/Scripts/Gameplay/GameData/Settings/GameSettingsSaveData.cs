using System;
using Madduck.Save;
using Madduck.Utils;
using MessagePack;
using Sirenix.OdinInspector;
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
    
    [Serializable]
    public class ControlSettings
    {
        private GameSettingsManagerConfig _gameSettingsManagerConfig;
        [ShowInInspector] public float FishingBoardMouseSensitivity { get; set; } = 500f;
        [ShowInInspector] public float FishingBoardGamepadSensitivity { get; set; } = 500f;

        public Percentage FishingBoardMouseSensitivityPercentage
        {
            get
            {
                var reverseLerp = Mathf.InverseLerp(
                    _gameSettingsManagerConfig.MouseSensitivityRange.x,
                    _gameSettingsManagerConfig.MouseSensitivityRange.y,
                    FishingBoardMouseSensitivity);
                return Percentage.FromFraction(reverseLerp);
            }
        }
        
        public Percentage FishingBoardGamepadSensitivityPercentage
        {
            get
            {
                var reverseLerp = Mathf.InverseLerp(
                    _gameSettingsManagerConfig.GamepadSensitivityRange.x,
                    _gameSettingsManagerConfig.GamepadSensitivityRange.y,
                    FishingBoardGamepadSensitivity);
                return Percentage.FromFraction(reverseLerp);
            }
        }
        
        public void SetUp(GameSettingsManagerConfig gameSettingsManagerConfig)
        {
            _gameSettingsManagerConfig = gameSettingsManagerConfig;
        }
        
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