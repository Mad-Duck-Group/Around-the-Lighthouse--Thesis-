using System;
using System.Collections.Generic;
using Madduck.Core;
using Madduck.Day;
using Madduck.GameData;
using Madduck.GameData.Fisherman;
using Madduck.Input;
using Madduck.RoomPreset;
using Madduck.Shared;
using Madduck.Utils;
using Madduck.WeatherPreset;
using MessagePipe;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public record FishingRoomManagerDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private FishingRoomManager _fishingRoomManager;
        [ShowInInspector] private WeatherWeightTableInstance _weatherWeightTable;
        [ShowInInspector] private RoomPresetManager _roomPresetManager;
        [ShowInInspector] private WeatherPresetManager _weatherPresetManager;
        
        public FishingRoomManagerDebugData(FishingRoomManager fishingRoomManager,
            WeatherWeightTableInstance weatherWeightTable,
            RoomPresetManager roomPresetManager ,
            WeatherPresetManager weatherPresetManager
            )
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _fishingRoomManager = fishingRoomManager;
            _roomPresetManager = roomPresetManager;
            _weatherWeightTable = weatherWeightTable;
            _weatherPresetManager = weatherPresetManager;
        }
    }
    
    [ShowOdinSerializedPropertiesInInspector]
    public class FishingRoomManagerLifetimeScope : LifetimeScope, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("References")]
        [Required,
         SerializeField] private FishingRoomConfig fishingRoomConfig;
        [Required,
         SerializeField] private WeatherWeightTable weatherWeightTable;
        [Required,
         SerializeField] private List<RoomPreset.RoomPreset> roomPresets;
        [Required,
         SerializeField] private WeatherPresetConfig weatherPresetConfig;
        [Required,
         SerializeField] private PlayerAnimatorInstaller playerAnimatorInstaller;
        [HideReferenceObjectPicker,
         OdinSerialize] private List<IInstaller> uiInstallers = new();
        
        [Title("Debug")] 
        [SerializeField] private bool spoofWeather;
        [ShowIf(nameof(spoofWeather)),
         OdinSerialize] private IFactory<WeatherItemInstance> weatherFactoryMock;
        [SerializeField] private bool spoofMaxFishCount;
        [ShowIf(nameof(spoofMaxFishCount)),
         OdinSerialize] private IFactory<uint> maxFishCountFactoryMock;
        
#if UNITY_EDITOR
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_fishingRoomManagerDebugData, "Fishing Room Manager Debug");
        }
        
        private FishingRoomManagerDebugData _fishingRoomManagerDebugData;
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
#if !UNITY_EDITOR
            spoofWeather = false;
            spoofMaxFishCount = false;
#endif
            // Weight Tables
            builder.RegisterInstance(weatherWeightTable)
                .As<IWeightTable<WeatherWeightRecord>>();
            builder.Register<WeatherWeightTableInstance>(Lifetime.Singleton)
                .AsSelf();
            
            // Factories
            if (spoofWeather && weatherFactoryMock != null)
            {
                builder.Register(x =>
                    {
                        x.Inject(weatherFactoryMock);
                        return weatherFactoryMock;
                    }, Lifetime.Singleton)
                    .As<IFactory<WeatherItemInstance>>();
            }
            else
            {
                builder.Register<WeatherFactory>(Lifetime.Singleton)
                    .As<IFactory<WeatherItemInstance>>();
            }
            if (spoofMaxFishCount && maxFishCountFactoryMock != null)
            {
                builder.Register(_ => maxFishCountFactoryMock, Lifetime.Singleton)
                    .As<IFactory<uint>>()
                    .Keyed(DIConstants.MaxFishCountFactoryId);
            }
            else
            {
                builder.Register<MaxFishCountFactory>(Lifetime.Singleton)
                    .As<IFactory<uint>>()
                    .Keyed(DIConstants.MaxFishCountFactoryId);
            }
            
            // Room Presets
            builder.RegisterInstance(roomPresets).As<List<RoomPreset.RoomPreset>>();
            builder.RegisterEntryPoint<RoomPresetManager>().AsSelf();
            
            // Weather Presets
            builder.RegisterInstance(weatherPresetConfig).AsSelf();
            builder.Register<WeatherPresetManager>(Lifetime.Singleton).AsSelf();
            
            // Handlers and Managers
            builder.Register<FishingRoomMessageCenter>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<FishingRoomPopUpHandler>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<FishingRoomSecretResetHandler>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<FishingRoomWeatherHandler>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<FishingRoomPauseHandler>(Lifetime.Singleton)
                .AsSelf();
            builder.RegisterInstance(fishingRoomConfig).AsSelf();
            builder.Register<FishingRoomManager>(Lifetime.Singleton)
                .As<IRequestHandler<CanContinueFishingRequest, bool>>()
                .AsSelf();
            
            // UI Installers
            playerAnimatorInstaller?.Install(builder);
            foreach (var uiInstaller in uiInstallers)
            {   
                uiInstaller.Install(builder);
            }
            
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<FishingRoomMessageCenter>();
                x.Resolve<FishingRoomPopUpHandler>();
                x.Resolve<FishingRoomWeatherHandler>();
                x.Resolve<FishingRoomSecretResetHandler>();
                x.Resolve<FishingRoomPauseHandler>();
#if UNITY_EDITOR
                var fishingRoomManager = x.Resolve<FishingRoomManager>();
                var table = x.Resolve<WeatherWeightTableInstance>();
                var roomPresetManager = x.Resolve<RoomPresetManager>();
                var weatherPresetManager = x.Resolve<WeatherPresetManager>();
                _fishingRoomManagerDebugData = new FishingRoomManagerDebugData(fishingRoomManager, table,roomPresetManager,weatherPresetManager);
#endif
            });

        }
        
        #region Serialization
        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData 
        { 
            get => serializationData;
            set => serializationData = value;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
        }
        #endregion
    }
}