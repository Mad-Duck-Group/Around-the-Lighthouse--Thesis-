using System.Collections.Generic;
using Madduck.Day;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [ShowOdinSerializedPropertiesInInspector]
    public class LoadSceneManagerLifetimeScope : LifetimeScope ,ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Title("References")]
        [Required,
         OdinSerialize] private List<IInstaller> uiInstallers = new();
        [Required,
         SerializeField] private LoadingView loadingView;
        
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(loadingView)
                .As<LoadingView>();
            builder.Register<LoadingViewModel>(Lifetime.Singleton);
            
            foreach (var installer in uiInstallers)
            {
                installer.Install(builder);
            }
            builder.RegisterBuildCallback(x =>
            {
                x.Resolve<LoadingViewModel>();
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
