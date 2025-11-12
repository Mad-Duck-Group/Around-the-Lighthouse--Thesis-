using System;
using HasanSadikin.Carousel;
using Madduck.Input;
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
         SerializeField] private CarouselController<LocationData> _carouselController;
        [Required,
         SerializeField]private GameObject uiBeforeTriggerBait;
        [Required,
         SerializeField] private GameObject uiAfterTriggerBait;
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => baitButtonViewFactory, Lifetime.Singleton)
                .As<IGenericFactory<BaitButtonView>>();
            builder.Register<BaitSelectionViewModel>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BaitController>(Lifetime.Singleton).AsSelf();
            builder.Register<ICarouselItemPositioner, HorizontalCarouselItemPositioner>(Lifetime.Singleton);
            builder.RegisterComponent(_carouselController);
            builder.RegisterInstance(new UIBeforeTriggerBait(uiBeforeTriggerBait));
            builder.RegisterInstance(new UIAfterTriggerBait(uiAfterTriggerBait));
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<BaitSelectionViewModel>();
                x.Resolve<BaitController>();

            });
        }
    }
}