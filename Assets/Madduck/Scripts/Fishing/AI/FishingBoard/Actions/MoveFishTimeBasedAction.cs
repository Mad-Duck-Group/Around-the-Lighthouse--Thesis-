using System;
using Madduck.Scripts.Fishing.Controller.FishingBoard;
using Madduck.Scripts.Fishing.DI.FishingBoard;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using VContainer;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveFishTimeBased", story: "Move fish to [TargetPosition] of [FishingBoard] in [Duration] seconds", category: "Action", id: "e3a7fa1ac8e1d556bfdccadd599ea0e0")]
public partial class MoveFishTimeBasedAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;
    [SerializeReference] public BlackboardVariable<FishingBoardLifetimeScope> FishingBoard;
    [SerializeReference] public BlackboardVariable<float> Duration;
    private FishingBoardController _fishingBoardController;

    protected override Status OnStart()
    {
        _fishingBoardController ??= FishingBoard.Value.Container.Resolve<FishingBoardController>();
        _fishingBoardController.MoveFishTimeBased(TargetPosition.Value, Duration.Value);
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

