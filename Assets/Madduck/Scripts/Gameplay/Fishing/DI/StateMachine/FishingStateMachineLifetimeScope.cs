using System;
using System.Collections.Generic;
using Madduck.Fishing.Shared;
using Madduck.Fishing.StateMachine;
using Madduck.Fishing.UI;
using Madduck.GameData;
using Madduck.GameData.Fisherman;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Madduck.Fishing.DI
{
    [Serializable]
    public record FishingStateMachineDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        
        [ShowInInspector] private FishingStateMachine _stateMachine;
        [ShowInInspector] private IFactory<IFishableItemInstance> _fishableFactory;
        
        public FishingStateMachineDebugData(
            FishingStateMachine stateMachine,
            IFactory<IFishableItemInstance> fishableFactory)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _stateMachine = stateMachine;
            _fishableFactory = fishableFactory;
        }
    }
    
    [ShowOdinSerializedPropertiesInInspector]
    public class FishingStateMachineLifetimeScope : LifetimeScope, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("References")] 
        [HideReferenceObjectPicker, 
         OdinSerialize] private List<IInstaller> fishingStateInstallers = new();
        [HideReferenceObjectPicker, 
         OdinSerialize] private BubbleManagerInstaller bubbleManagerInstaller = new();
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private HookProjectileFactory hookProjectileFactory = new();
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private FishSpriteFactory fishSpriteFactory = new();
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private FishEyesFactory fishEyesFactory = new();

        [Title("Debug")] 
        [SerializeField] private bool spoofFishable;
        [ShowIf(nameof(spoofFishable)),
            OdinSerialize] private FishableFactoryMock fishableFactoryMock;
        
        
#if UNITY_EDITOR
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_fishingStateMachineDebugData, "Fishing State Machine Debug");
        }
        
        private FishingStateMachineDebugData _fishingStateMachineDebugData;
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
#if !UNITY_EDITOR
            spoofFishable = false;
#endif
            if (spoofFishable && fishableFactoryMock != null)
            { 
                builder.Register(_ => fishableFactoryMock, Lifetime.Singleton)
                    .As<IFactory<IFishableItemInstance>>();
            }
            else
            {
                builder.Register<FishableFactory>(Lifetime.Singleton)
                    .As<IFactory<IFishableItemInstance>>();
            }

            builder.Register<FishingSharedVariable>(Lifetime.Singleton).AsSelf();
            builder.Register(x =>
            {
                x.Inject(hookProjectileFactory);
                return hookProjectileFactory;
            }, Lifetime.Singleton).As<IHookFactory>();
            builder.Register(_ => fishSpriteFactory, Lifetime.Singleton).As<IFishSpriteFactory>();
            builder.Register(_ => fishEyesFactory, Lifetime.Singleton).As<IFishEyesFactory>();
            builder.Register<FishingNoneState>(Lifetime.Scoped).AsSelf();
            builder.Register<FishingStateMachine>(Lifetime.Singleton).AsSelf();
            bubbleManagerInstaller.Install(builder);
            fishingStateInstallers.ForEach(x => x.Install(builder));
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var noneState = x.Resolve<FishingNoneState>();
                stateMachine.AddState(FishingStateType.None, noneState);
#if UNITY_EDITOR
                var fishItemInstanceFactory = x.Resolve<IFactory<IFishableItemInstance>>();
                _fishingStateMachineDebugData = new FishingStateMachineDebugData(stateMachine, fishItemInstanceFactory);
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