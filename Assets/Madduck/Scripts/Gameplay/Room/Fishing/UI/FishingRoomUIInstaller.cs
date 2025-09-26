using System;
using System.Collections.Generic;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public record FishingRoomUIDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private ItemPopUpHandler _popUpHandler;
        
        public FishingRoomUIDebugData(ItemPopUpHandler popUpHandler)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _popUpHandler = popUpHandler;
        }
    }
    
    [Serializable]
    public class FishingRoomUIInstaller : IInstaller
    {
        [Title("References")]
        [Required,
            SerializeField] private CardViewFactory cardViewFactory;
        [Required,
         SerializeField] private WeatherHUDView weatherHUDView;
        [Required,
         SerializeField] private FishCaughtView fishCaughtView;
        [Required,
         SerializeField] private SerializableDictionary<WeatherType, Sprite> weatherIcons;
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private FishItemPopUpManager fishItemPopUpManager = new();
        
#if UNITY_EDITOR
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_fishingRoomUIDebugData, "Fishing Room UI Debug");
        }
        
        private FishingRoomUIDebugData _fishingRoomUIDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => cardViewFactory, Lifetime.Singleton)
                .As<IGenericFactory<CardView>>();
            builder.Register<CardRackView>(Lifetime.Singleton);
            
            builder.Register(_ => weatherIcons, Lifetime.Singleton)
                .As<SerializableDictionary<WeatherType, Sprite>>();
            builder.RegisterComponent(weatherHUDView)
                .As<WeatherHUDView>();
            builder.Register<WeatherHUDViewModel>(Lifetime.Singleton);
            builder.Register(_ => fishCaughtView, Lifetime.Singleton)
                .As<FishCaughtView>();
            builder.Register<FishCaughtViewModel>(Lifetime.Singleton);
            
            builder.Register(_ => fishItemPopUpManager, Lifetime.Singleton)
                .As<FishItemPopUpManager>();
            builder.Register<ItemPopUpHandler>(Lifetime.Singleton).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<CardRackView>();
                x.Resolve<WeatherHUDViewModel>();
                x.Resolve<FishCaughtViewModel>();
                var popUpHandler = x.Resolve<ItemPopUpHandler>();
#if UNITY_EDITOR
                _fishingRoomUIDebugData = new(popUpHandler);
#endif
            });
        }
    }
}