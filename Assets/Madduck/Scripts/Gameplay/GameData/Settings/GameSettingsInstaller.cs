using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.GameData
{
    [Serializable]
    public record GameSettingsManagerDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private GameSettingsManager _manager;
        
        public GameSettingsManagerDebugData(
            GameSettingsManager manager)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _manager = manager;
        }
    }
    
    [Serializable]
    public class GameSettingsInstaller : IInstaller
    {
        [Title("Game Settings"),
            HideLabel,
            ShowInInspector]
        private InspectorPlaceholder _placeholder;
        [Required, 
         SerializeField] private GameSettingsManagerConfig gameSettingsManagerConfig;
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_gameSettingsManagerDebugData, "Game Settings Manager Debug");
        }
        
        private GameSettingsManagerDebugData _gameSettingsManagerDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameSettingsManagerConfig).AsSelf();
            builder.RegisterEntryPoint<GameSettingsManager>()
                .AsSelf();
            
#if UNITY_EDITOR
            builder.RegisterBuildCallback(x =>
            {
                _gameSettingsManagerDebugData = new GameSettingsManagerDebugData(
                    x.Resolve<GameSettingsManager>());
            });
#endif
        }
    }
}