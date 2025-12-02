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
    [NodeDescription(name: "MoveFishSpeedBased", story: "Move fish to [TargetPosition] of [FishingBoard] in [Speed] unit/s", category: "Action/Fish", id: "ef23dfc1e606fa72820b77856bfa11b1")]
    public partial class MoveFishSpeedBasedAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;
        [SerializeReference] public BlackboardVariable<FishingStateMachineLifetimeScope> FishingBoard;
        [SerializeReference] public BlackboardVariable<float> Speed;
        private IFishingBoardAIController _fishingBoardAIController;

        protected override Status OnStart()
        {
            _fishingBoardAIController ??= FishingBoard.Value.Container.Resolve<IFishingBoardAIController>();
            _fishingBoardAIController.MoveFishSpeedBased(TargetPosition.Value, Speed.Value);
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

