using System;
using System.Collections.Generic;
using Madduck.Day;
using Madduck.RoomPreset.Madduck.Scripts.Gameplay.RoomPreset;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[Serializable]
public record RoomPresetManagerDebugData : IDebugData
{
    [field: SerializeField] public bool ConstantUpdate { get; private set; }
    [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
    
        
    public RoomPresetManagerDebugData(
        RoomPresetManager manager
        )
    {
        ConstantUpdate = false;
        AutoCloseWhenPlayModeEnds = true;
        
    }
}

public class RoomPresetManagerLifetimeScope : LifetimeScope
{
    [SerializeField] private List<RoomPreset> roomPresets;
    
#if UNITY_EDITOR
    [Title("Debug")]
    [HideInEditorMode]
    [Button("Open Debug Window")]
    private void OpenDebugWindow()
    {
        DebugEditorWindow.Inspect(_dayManagerDebugData, "Room Preset Manager Debug");
    }
        
    private RoomPresetManagerDebugData _dayManagerDebugData;
#endif
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(roomPresets).As<List<RoomPreset>>();
        //builder.RegisterComponent(roomPresetManager).AsSelf();
        builder.RegisterBuildCallback(x =>
        {
#if UNITY_EDITOR
            var manager = x.Resolve<RoomPresetManager>();
            _dayManagerDebugData = new RoomPresetManagerDebugData(manager);
#endif
        });
        
    }
}
