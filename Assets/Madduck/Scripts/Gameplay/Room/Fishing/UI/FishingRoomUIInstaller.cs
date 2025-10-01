using System;
using System.Collections.Generic;
using Madduck.Day;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("fishCaughtView")]
        [Required,
         SerializeField] private FishCountView fishCountView;
        [Required,
         SerializeField] private BaitButtonViewFactory baitButtonViewFactory;
        [Required,
         SerializeField] private SerializableDictionary<WeatherType, Sprite> weatherIcons;
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private FishItemPopUpManager fishItemPopUpManager = new();
        [Required,
         SerializeField] private SerializableDictionary<DayRoomKey, Sprite> sprites;
        [Required,
         SerializeField] private RoomTrackFactory roomTrackFactory;
        [Required,
         SerializeField] private RoomTrackView roomTrackView;
        [Required,
         SerializeField] private BoatTrackViewFactory BoatTrackViewFactory;
        
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
            builder.Register<CardRackViewModel>(Lifetime.Singleton);
            
            builder.Register(_ => weatherIcons, Lifetime.Singleton)
                .As<SerializableDictionary<WeatherType, Sprite>>();
            builder.RegisterComponent(weatherHUDView)
                .As<WeatherHUDView>();
            builder.Register<WeatherHUDViewModel>(Lifetime.Singleton);
            builder.RegisterComponent(fishCountView)
                .As<FishCountView>();
            builder.Register<FishCountViewModel>(Lifetime.Singleton);
            builder.Register(_ => sprites, Lifetime.Scoped)
                .As<SerializableDictionary<DayRoomKey, Sprite>>();
            builder.Register(_ => roomTrackFactory, Lifetime.Singleton)
                .As<IGenericFactory<RoomTrackView>>();
            builder.Register(_ => BoatTrackViewFactory, Lifetime.Singleton)
                .As<IGenericFactory<BoatTrackView>>();
            builder.RegisterComponent(roomTrackView)
                .As<RoomTrackView>();
            builder.Register<RoomTrackViewModel>(Lifetime.Singleton);
            builder.Register<RoomTrackColumnViewModel>(Lifetime.Singleton);
            builder.Register(_ => fishItemPopUpManager, Lifetime.Singleton)
                .As<FishItemPopUpManager>();
            builder.Register<ItemPopUpHandler>(Lifetime.Singleton).AsSelf();
            
            builder.Register(_ => baitButtonViewFactory, Lifetime.Singleton)
                .As<IGenericFactory<BaitButtonView>>();
            builder.Register<BaitSelectionViewModel>(Lifetime.Singleton);
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<CardRackViewModel>();
                x.Resolve<BaitSelectionViewModel>();
                x.Resolve<RoomTrackViewModel>();
                x.Resolve<RoomTrackColumnViewModel>();
                x.Resolve<WeatherHUDViewModel>();
                x.Resolve<FishCountViewModel>();
                var popUpHandler = x.Resolve<ItemPopUpHandler>();
#if UNITY_EDITOR
                _fishingRoomUIDebugData = new(popUpHandler);
#endif
            });
        }
    }
}