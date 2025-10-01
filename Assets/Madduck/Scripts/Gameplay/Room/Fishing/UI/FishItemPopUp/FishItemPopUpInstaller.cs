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
    public record FishItemPopUpDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private ItemPopUpHandler _popUpHandler;
        
        public FishItemPopUpDebugData(ItemPopUpHandler popUpHandler)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _popUpHandler = popUpHandler;
        }
    }
    
    [Serializable]
    public class FishItemPopUpInstaller : IInstaller
    {
        [Title("Fish Item Pop Up")]
        [Required, HideReferenceObjectPicker,
         OdinSerialize] private FishItemPopUpManager fishItemPopUpManager = new();
        
#if UNITY_EDITOR
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_fishItemPopUpDebugData, "Fish Item Pop Up Debug");
        }
        
        private FishItemPopUpDebugData _fishItemPopUpDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.Register(_ => fishItemPopUpManager, Lifetime.Singleton)
                .As<FishItemPopUpManager>();
            builder.Register<ItemPopUpHandler>(Lifetime.Singleton).AsSelf();
            
            builder.RegisterBuildCallback(x =>
            {
                var popUpHandler = x.Resolve<ItemPopUpHandler>();
#if UNITY_EDITOR
                _fishItemPopUpDebugData = new(popUpHandler);
#endif
            });
        }
    }
}