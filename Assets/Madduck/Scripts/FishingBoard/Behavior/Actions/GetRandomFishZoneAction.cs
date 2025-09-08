using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRandomFishZone", story: "Get random [TargetFishZone] except [FishZone]", category: "Action", id: "0f45fd91778e4cadd14b7f14ae273fa5")]
public partial class GetRandomFishZoneAction : Action
{
    [SerializeReference] public BlackboardVariable<BlackboardFishZone> TargetFishZone;
    [SerializeReference] public BlackboardVariable<BlackboardFishZone> FishZone;

    protected override Status OnStart()
    {
        var enumCount = Enum.GetValues(typeof(BlackboardFishZone)).Length;
        var removeIndex = (int)FishZone.Value;
        List<int> availableFishZones = new List<int>(enumCount - 1);
        for (int i = 0; i < enumCount; i++)
        {
            if (i == removeIndex) continue;
            availableFishZones.Add(i);
        }
        TargetFishZone.Value = (BlackboardFishZone)availableFishZones[UnityEngine.Random.Range(0, availableFishZones.Count)];
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

