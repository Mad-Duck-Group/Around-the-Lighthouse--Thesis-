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
    [NodeDescription(name: "GetRandomPosition", story: "Get random [TargetPosition] on [FishingBoard]", category: "Action/Fish", id: "2a6dffff240bbb7cae1d6936898a7434")]
    public partial class GetRandomPositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;
        [SerializeReference] public BlackboardVariable<FishingBoardLifetimeScope> FishingBoard;
        private FishingBoardController _fishingBoardController;

        protected override Status OnStart()
        {
            _fishingBoardController ??= FishingBoard.Value.Container.Resolve<FishingBoardController>();
            TargetPosition.Value = _fishingBoardController.GetRandomPosition();
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

