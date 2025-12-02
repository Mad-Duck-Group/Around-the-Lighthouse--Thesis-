using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.GameData
{
    [Serializable]
    public class FishCatalogueInstaller : IInstaller
    {
        [Title("Fish Catalogue")]
        [Required, 
         SerializeField] private FishCatalogueConfig fishCatalogueConfig;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(fishCatalogueConfig).AsSelf();
            builder.RegisterEntryPoint<FishCatalogue>(Lifetime.Singleton).AsSelf();
        }
    }
}