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
    [NodeDescription(name: "WarpFish", story: "Warp fish to [TargetPosition] of [FishingBoard]", category: "Action/Fish", id: "ce39582446ee37e32af813d5364aaab5")]
    public partial class WarpFishAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;
        [SerializeReference] public BlackboardVariable<FishingStateMachineLifetimeScope> FishingBoard;
        private IFishingBoardAIController _fishingBoardAIController;

        protected override Status OnStart()
        {
            _fishingBoardAIController ??= FishingBoard.Value.Container.Resolve<IFishingBoardAIController>();
            _fishingBoardAIController.SetFishPosition(TargetPosition.Value);
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

