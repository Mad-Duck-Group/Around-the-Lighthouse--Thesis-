using System;
using Madduck.Save;
using Sirenix.OdinInspector;
using VContainer;
using VContainer.Unity;

namespace Madduck.GameData
{
    [Serializable]
    public class GameSettingsManager : IPostInitializable
    {
        private readonly MessagePackSaveManager _saveManager;
        
        private GameSettingsSaveObject _gameSettingsSaveObject;
        
        [ShowInInspector] public ControlSettings ControlSettings { get; } = new();
        
        [Inject]
        public GameSettingsManager(MessagePackSaveManager saveManager)
        {
            _saveManager = saveManager;
        }
        
        public void PostInitialize()
        {
            _gameSettingsSaveObject = _saveManager.GetFirstSaveObjectOfType<GameSettingsSaveObject>();
            Load();
        }

        public void Load()
        {
            if (!_gameSettingsSaveObject) return;
            var gameSettingsData = _gameSettingsSaveObject.GetSaveData<GameSettingsSaveData>();
            if (gameSettingsData == null) return;
            ControlSettings.LoadFromSaveData(gameSettingsData.ControlSettings);
        }

        public void Save()
        {
            if (!_gameSettingsSaveObject) return;
            var gameSettingsData = _gameSettingsSaveObject.GetSaveData<GameSettingsSaveData>();
            if (gameSettingsData == null) return;
            ControlSettings.SaveToSaveData(gameSettingsData.ControlSettings);
            _saveManager.Save(_gameSettingsSaveObject);
        }
    }
}