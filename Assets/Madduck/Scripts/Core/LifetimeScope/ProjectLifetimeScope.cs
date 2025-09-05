using System.Collections.Generic;
using Madduck.Scripts.Audio;
using MessagePipe;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Scripts.Core.LifetimeScope
{
    [ShowOdinSerializedPropertiesInInspector]
    public class ProjectLifetimeScope : VContainer.Unity.LifetimeScope, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("Installers")]
        [HideReferenceObjectPicker]
        [OdinSerialize] private List<IInstaller> installers;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterMessagePipe(options =>
            {
                options.InstanceLifetime = InstanceLifetime.Singleton;
            });
            installers.ForEach(installer => installer.Install(builder));
            builder.RegisterBuildCallback(x => GlobalMessagePipe.SetProvider(x.AsServiceProvider()));
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