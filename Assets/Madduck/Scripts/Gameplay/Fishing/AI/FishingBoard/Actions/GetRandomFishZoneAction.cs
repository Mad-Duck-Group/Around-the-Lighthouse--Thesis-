using System;
using System.Collections.Generic;
using Madduck.Fishing.Controller;
using Madduck.Fishing.Shared;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Madduck.Fishing.AI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "GetRandomFishZone", story: "Get random [TargetFishZone] except [FishZone]", category: "Action/Fish", id: "0f45fd91778e4cadd14b7f14ae273fa5")]
    public partial class GetRandomFishZoneAction : Action
    {
        [SerializeReference] public BlackboardVariable<FishZone> TargetFishZone;
        [SerializeReference] public BlackboardVariable<FishZone> FishZone;

        protected override Status OnStart()
        {
            var enumCount = Enum.GetValues(typeof(FishZone)).Length;
            var removeIndex = (int)FishZone.Value;
            List<int> availableFishZones = new List<int>(enumCount - 1);
            for (int i = 0; i < enumCount; i++)
            {
                if (i == removeIndex) continue;
                availableFishZones.Add(i);
            }
            TargetFishZone.Value = (FishZone)availableFishZones[UnityEngine.Random.Range(0, availableFishZones.Count)];
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
}

