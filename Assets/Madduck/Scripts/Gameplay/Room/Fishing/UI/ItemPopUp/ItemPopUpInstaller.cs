using System;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class ItemPopUpInstaller : IInstaller
    {
        [Title("Fish Item Pop Up")]
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private FishableItemPopUpFactory _fishableItemPopUpFactory = new();
        
        [Title("New Fish Pop Up")]
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private NewFishPopUpFactory _newFishPopUpFactory = new();
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(x =>
                {
                    x.Inject(_fishableItemPopUpFactory);
                    return _fishableItemPopUpFactory;
                }, Lifetime.Singleton)
                .As<IPopUpFactory<FishableItemPopUpObject>>();
            
            builder.Register(x =>
                {
                    x.Inject(_newFishPopUpFactory);
                    return _newFishPopUpFactory;
                }, Lifetime.Singleton)
                .As<IPopUpFactory<NewFishPopUpObject>>();
        }
    }
}