using System;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class CardSelectionInstaller : IInstaller
    {
        [Title("Card Selection")]
        [Required, 
         SerializeField] private CardSelectionScreenView cardSelectionScreenView;
        [Required,
         SerializeField] private CardSelectionFactory cardSelectionFactory = new();
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(cardSelectionFactory)
                .As<IFactory<CardSelectionView>>();
            builder.RegisterComponent(cardSelectionScreenView)
                .As<ITransitionable>();
            builder.Register<CardSelectionScreenViewModel>(Lifetime.Scoped)
                .AsSelf();
            builder.Register<CardSelectionController>(Lifetime.Scoped)
                .As<IModal>();
        }
    }
}