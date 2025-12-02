using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Fishing.Shared
{
    [Serializable]
    public class BubbleManagerInstaller : IInstaller
    {
        [Required, 
         SerializeField] private BubbleManagerConfig config;
        [HideReferenceObjectPicker, 
         SerializeField] private BubbleFactory bubbleFactory = new();
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config).AsSelf();
            builder.Register(x =>
            {
                x.Inject(bubbleFactory);
                return bubbleFactory;
            }, Lifetime.Singleton)
            .As<IBubbleViewFactory>();
            builder.Register<BubbleManager>(Lifetime.Singleton)
                .AsSelf();
        }
    }
}