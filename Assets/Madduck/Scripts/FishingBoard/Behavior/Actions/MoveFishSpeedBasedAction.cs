using System;
using Madduck.Scripts.FishingBoard;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using VContainer;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveFishSpeedBased", story: "Move fish to [TargetPosition] of [FishingBoard] in [Speed] unit/s", category: "Action", id: "ef23dfc1e606fa72820b77856bfa11b1")]
public partial class MoveFishSpeedBasedAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;
    [SerializeReference] public BlackboardVariable<FishingBoardLifetimeScope> FishingBoard;
    [SerializeReference] public BlackboardVariable<float> Speed;
    private FishingBoardController _fishingBoardController;

    protected override Status OnStart()
    {
        _fishingBoardController ??= FishingBoard.Value.Container.Resolve<FishingBoardController>();
        _fishingBoardController.MoveFishSpeedBased(TargetPosition.Value, Speed.Value);
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

