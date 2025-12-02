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
    [NodeDescription(name: "MoveFishTimeBased", story: "Move fish to [TargetPosition] of [FishingBoard] in [Duration] seconds", category: "Action/Fish", id: "e3a7fa1ac8e1d556bfdccadd599ea0e0")]
    public partial class MoveFishTimeBasedAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;
        [SerializeReference] public BlackboardVariable<FishingStateMachineLifetimeScope> FishingBoard;
        [SerializeReference] public BlackboardVariable<float> Duration;
        private IFishingBoardAIController _fishingBoardAIController;

        protected override Status OnStart()
        {
            _fishingBoardAIController ??= FishingBoard.Value.Container.Resolve<IFishingBoardAIController>();
            _fishingBoardAIController.MoveFishTimeBased(TargetPosition.Value, Duration.Value);
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

