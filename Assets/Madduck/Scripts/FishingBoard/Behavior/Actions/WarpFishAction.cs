using System;
using Madduck.Scripts.FishingBoard;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using VContainer;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WarpFish", story: "Warp fish to [TargetPosition] of [FishingBoard]", category: "Action", id: "ce39582446ee37e32af813d5364aaab5")]
public partial class WarpFishAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2> TargetPosition;
    [SerializeReference] public BlackboardVariable<FishingBoardLifetimeScope> FishingBoard;
    private FishingBoardController _fishingBoardController;

    protected override Status OnStart()
    {
        _fishingBoardController ??= FishingBoard.Value.Container.Resolve<FishingBoardController>();
        _fishingBoardController.SetFishPosition(TargetPosition.Value);
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

