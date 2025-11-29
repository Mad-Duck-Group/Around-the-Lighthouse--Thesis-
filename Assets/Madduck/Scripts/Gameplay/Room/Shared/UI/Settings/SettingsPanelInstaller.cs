using System;
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
         SerializeField] private SettingsPanelConfig settingsPanelConfig;
        [Required,
         SerializeField] private SettingsPanelView settingsPanelView;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(settingsPanelConfig).AsSelf();
            builder.RegisterComponent(settingsPanelView).AsSelf();
            builder.Register<SettingsPanelViewModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<SettingsPanelViewModel>();
            });
        }
    }
}