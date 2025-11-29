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
         SerializeField] private SettingsPanelInstaller settingsPanelInstaller;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(mainMenuConfig).AsSelf();
            builder.RegisterEntryPoint<MainMenuManager>().AsSelf();
            builder.RegisterComponent(mainMenuView).AsSelf();
            builder.Register<MainMenuViewModel>(Lifetime.Singleton).AsSelf();
            settingsPanelInstaller.Install(builder);
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<MainMenuViewModel>();
            });
        }
    }
}