using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    public class MainMenuLifetimeScope : LifetimeScope
    {
        [Title("References")]
        [Required, 
         SerializeField] private MainMenuConfig mainMenuConfig;
        [Required,
         SerializeField] private MainMenuView mainMenuView;
        [Required,
         SerializeField] private SettingsPanelConfig settingsPanelConfig;
        [Required,
         SerializeField] private SettingsPanelView settingsPanelView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(mainMenuConfig).AsSelf();
            builder.RegisterEntryPoint<MainMenuManager>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(mainMenuView).AsSelf();
            builder.Register<MainMenuViewModel>(Lifetime.Singleton).AsSelf();
            
            builder.RegisterInstance(settingsPanelConfig).AsSelf();
            builder.RegisterComponent(settingsPanelView).AsSelf();
            builder.Register<SettingsPanelViewModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<MainMenuViewModel>();
            });
        }
    }
}