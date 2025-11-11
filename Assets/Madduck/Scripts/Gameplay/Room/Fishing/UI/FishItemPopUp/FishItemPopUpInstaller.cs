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
    public class FishItemPopUpInstaller : IInstaller
    {
        [Title("Fish Item Pop Up")]
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private FishItemPopUpFactory _fishItemPopUpFactory = new();
        
        [Title("New Fish Pop Up")]
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private NewFishPopUpFactory _newFishPopUpFactory = new();
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(x =>
                {
                    x.Inject(_fishItemPopUpFactory);
                    return _fishItemPopUpFactory;
                }, Lifetime.Singleton)
                .As<IPopUpFactory<FishItemPopUpObject>>();
            
            builder.Register(x =>
                {
                    x.Inject(_newFishPopUpFactory);
                    return _newFishPopUpFactory;
                }, Lifetime.Singleton)
                .As<IPopUpFactory<NewFishPopUpObject>>();
        }
    }
}