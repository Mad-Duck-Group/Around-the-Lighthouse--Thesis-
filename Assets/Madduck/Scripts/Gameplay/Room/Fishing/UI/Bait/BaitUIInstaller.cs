using System;
using HasanSadikin.Carousel;
using Madduck.Input;
using Madduck.Room.PointingBait;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class BaitUIInstaller : IInstaller
    {
        [Title("Bait")]
        [Required,
         SerializeField] private BaitButtonViewFactory baitButtonViewFactory;

        [Required,
         SerializeField]private PointingBaitView pointingBaitView;
        
        [Required,
         SerializeField] private PointingBaitConfig pointingBaitConfig;
        [Required,
         SerializeField] private CarouselController _carouselController;
        [Required,
         SerializeField]private BaitUITriggerConfig baitUITriggerConfig;
        [Required,
        SerializeField] private HorizontalCarouselItemPositioner positioner;

        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => baitButtonViewFactory, Lifetime.Singleton)
                .As<IFactory<BaitButtonView>>();
            builder.Register(_ => pointingBaitConfig, Lifetime.Singleton).As<PointingBaitConfig>();
            builder.Register(_ => baitUITriggerConfig, Lifetime.Singleton).As<BaitUITriggerConfig>();
            builder.Register<BaitSelectionViewModel>(Lifetime.Singleton);
            builder.Register<PointingBaitViewModel>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BaitController>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(positioner)
                .As<ICarouselItemPositioner>();
            builder.RegisterComponent(_carouselController);
            builder.RegisterComponent(pointingBaitView).As<PointingBaitView>();
            
            
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<BaitSelectionViewModel>();
                x.Resolve<PointingBaitViewModel>();
                x.Resolve<BaitController>();

            });
        }
    }
}