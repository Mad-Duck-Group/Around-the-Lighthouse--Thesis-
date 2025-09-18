using System;
using Madduck.Fishing.Controller;
using Madduck.Fishing.DI;
using Madduck.Fishing.Shared;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using VContainer;
using Action = Unity.Behavior.Action;

namespace Madduck.Fishing.AI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "GetRandomPositionOnFishZone", story: "Get random [TargetPosition] on [FishZone] of [FishingBoard]", category: "Action/Fish", id: "025700b9341c509a350c9dff577a9425")]
    public partial class GetRandomPositionOnFishZoneAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;
        [SerializeReference] public BlackboardVariable<FishZone> FishZone;
        [SerializeReference] public BlackboardVariable<FishingBoardLifetimeScope> FishingBoard;
        private FishingBoardController _fishingBoardController;

        protected override Status OnStart()
        {
            _fishingBoardController ??= FishingBoard.Value.Container.Resolve<FishingBoardController>();
            TargetPosition.Value = _fishingBoardController.GetRandomPositionOnFishZone(FishZone.Value);
            return Status.Running;;
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

