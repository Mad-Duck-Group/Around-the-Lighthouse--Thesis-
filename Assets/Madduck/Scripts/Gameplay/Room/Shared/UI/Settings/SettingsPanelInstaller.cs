using System;
using Madduck.GameData;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class SettingsPanelInstaller : IInstaller
    {
        [Title("Settings Panel")]
        [Required,
         SerializeField] private GameSettingsManagerConfig gameSettingsManagerConfig;
        [Required,
         SerializeField] private SettingsPanelView settingsPanelView;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameSettingsManagerConfig).AsSelf();
            builder.RegisterComponent(settingsPanelView).AsSelf();
            builder.Register<SettingsPanelViewModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<SettingsPanelViewModel>();
            });
        }
    }
}