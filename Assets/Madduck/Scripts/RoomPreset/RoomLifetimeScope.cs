using System.Collections.Generic;
using Madduck.Scripts.RoomGenerate;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RoomLifetimeScope : LifetimeScope
{
    [SerializeField] private RoomPresetManager roomPresetManager;
    [SerializeField] private List<RoomPreset> roomPresets;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(roomPresets).As<List<RoomPreset>>();
        builder.RegisterComponent(roomPresetManager).AsSelf();;
    }
}
