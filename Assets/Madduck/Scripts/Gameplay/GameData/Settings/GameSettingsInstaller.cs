using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using VContainer;
using VContainer.Unity;

namespace Madduck.GameData
{
    [Serializable]
    public class GameSettingsInstaller : IInstaller
    {
        [Title("Game Settings"),
            HideLabel,
            ShowInInspector]
        private InspectorPlaceholder _placeholder;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameSettingsManager>()
                .AsSelf();
        }
    }
}