using System;
using Madduck.Scripts.FishingBoard;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using VContainer;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRandomPosition", story: "Get random [TargetPosition] on [FishingBoard]", category: "Action", id: "2a6dffff240bbb7cae1d6936898a7434")]
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

