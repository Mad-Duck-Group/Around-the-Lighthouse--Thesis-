using System;
using Madduck.Fishing.Controller;
using Madduck.Fishing.DI;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using VContainer;
using Action = Unity.Behavior.Action;

namespace Madduck.Fishing.AI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "GetRandomPosition", story: "Get random [TargetPosition] within a unit circle", category: "Action/Fish", id: "2a6dffff240bbb7cae1d6936898a7434")]
    public partial class GetRandomPositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;

        protected override Status OnStart()
        {
            TargetPosition.Value = FishingBoardUtils.GetRandomPosition();
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

