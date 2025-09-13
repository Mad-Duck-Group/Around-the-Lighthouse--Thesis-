using System;
using System.Collections.Generic;
using Madduck.Scripts.Fishing.StateMachine;
using Madduck.Scripts.Fishing.StateMachine.None;
using Madduck.Scripts.Fishing.UI.ThrowHook;
using MadDuck.Scripts.Items.Data;
using MadDuck.Scripts.Items.Instance;
using Madduck.Scripts.Utils.Editor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Scripts.Fishing.DI.StateMachine
{
    [Serializable]
    public struct FishingStateMachineDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [ShowInInspector] private FishingStateMachine _stateMachine;
        
        public FishingStateMachineDebugData(FishingStateMachine stateMachine)
        {
            ConstantUpdate = false;
            _stateMachine = stateMachine;
        }
    }
    
    [ShowOdinSerializedPropertiesInInspector]
    public class FishingStateMachineLifetimeScope : LifetimeScope, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("References")]
        [InlineEditor]
        [Required]
        [SerializeField] private FishItemData fishItemData;
        [InlineEditor]
        [Required]
        [SerializeField] private FishingRodItemData fishingRodItemData;
        [Required, AssetsOnly]
        [SerializeField] private ThrowHookProjectile throwHookProjectilePrefab;
        [Required]
        [SerializeField] private Transform throwHookSpawnPoint;
        
#if UNITY_EDITOR
        [Title("Debug")]
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            _debugWindow = DebugEditorWindow.Inspect(_fishingStateMachineDebugData, "Fishing State Machine Debug");
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_debugWindow)
            {
                _debugWindow.Close();
            }
        }
        
        private DebugEditorWindow _debugWindow;
        private FishingStateMachineDebugData _fishingStateMachineDebugData;
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(throwHookProjectilePrefab).AsSelf();
            builder.RegisterInstance(throwHookSpawnPoint).Keyed("ProjectileParent").AsSelf();
            builder.Register<ThrowHookProjectileFactory>(Lifetime.Singleton).AsSelf();
            builder.RegisterInstance(new FishItemInstance(fishItemData)).AsSelf();
            builder.RegisterInstance(new FishingRodItemInstance(fishingRodItemData)).AsSelf();
            builder.Register<FishingNoneState>(Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<FishingStateMachine>().AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var stateMachine = x.Resolve<FishingStateMachine>();
                var noneState = x.Resolve<FishingNoneState>();
                stateMachine.AddState(FishingStateType.None, noneState);
#if UNITY_EDITOR
                _fishingStateMachineDebugData = new FishingStateMachineDebugData(stateMachine);
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