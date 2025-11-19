using Sirenix.OdinInspector;
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

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(mainMenuConfig).AsSelf();
            builder.RegisterEntryPoint<MainMenuManager>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(mainMenuView).AsSelf();
            builder.Register<MainMenuViewModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<MainMenuViewModel>();
            });
        }
    }
}