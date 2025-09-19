using System;
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
        [ShowInInspector] private IFishFactory _fishFactory;
        
        public FishingStateMachineDebugData(
            FishingStateMachine stateMachine,
            IFishFactory fishFactory)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _stateMachine = stateMachine;
            _fishFactory = fishFactory;
        }
    }
    
    [ShowOdinSerializedPropertiesInInspector]
    public class FishingStateMachineLifetimeScope : LifetimeScope, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("References")]
        [Required, 
         SerializeField] private HookProjectileFactory hookProjectileFactory;

        [Title("Debug")] 
        [SerializeField] private bool spoofFish;
        [ShowIf(nameof(spoofFish)),
            OdinSerialize] private IFishFactory fishFactoryMock;
        
        
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
            spoofFish = false;
#endif
            if (spoofFish && fishFactoryMock != null)
            { 
                builder.RegisterInstance(fishFactoryMock).As<IFishFactory>();
            }
            else
            {
                builder.Register<FishFactory>(Lifetime.Singleton).As<IFishFactory>();
            }
            builder.RegisterInstance(hookProjectileFactory).AsSelf();
            builder.Register<FishingNoneState>(Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<FishingStateMachine>().AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var noneState = x.Resolve<FishingNoneState>();
                stateMachine.AddState(FishingStateType.None, noneState);
#if UNITY_EDITOR
                var fishItemInstanceFactory = x.Resolve<IFishFactory>();
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