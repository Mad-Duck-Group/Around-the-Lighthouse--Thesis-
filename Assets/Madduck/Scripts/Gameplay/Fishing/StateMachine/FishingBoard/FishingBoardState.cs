using System;
using Madduck.Fishing.Controller;
using Madduck.Utils;
using R3;

namespace Madduck.Fishing.StateMachine
{
    /// <summary>
    /// State of the Fishing Board mini-game.
    /// </summary>
    [Serializable]
    public class FishingBoardState : FishingState
    {
        private readonly FishingBoardController _controller;
        private IDisposable _fishingBoardResultSubscription;
        
        public FishingBoardState(
            FishingStateMachine stateMachine,
            FishingBoardController controller) 
            : base(stateMachine)
        {
            _controller = controller;
        }
        
        public override void Enter()
        {
            base.Enter();
            _controller.SetActive(true);
            _fishingBoardResultSubscription = Observable.FromEvent<Sign>(
                    h => _controller.OnFishingBoardResult += h,
                    h => _controller.OnFishingBoardResult -= h)
                .Subscribe(OnFishingBoardResult);
        }

        public override void Exit()
        {
            base.Exit();
            _fishingBoardResultSubscription.Dispose();
            _controller.SetActive(false);
            _controller.Reset();
        }

        private void OnFishingBoardResult(Sign result)
        {
            switch (result)
            {
                case Sign.Negative:
                    DebugUtils.Log("Fish escaped, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    break;
                case Sign.Positive:
                    DebugUtils.Log("Fish is tired, transitioning to ReelingState");
                    stateMachine.NextState();
                    break;
                case Sign.Zero:
                default:
                    DebugUtils.LogError($"Unexpected FishingBoardResult value: {result}");
                    break;
            }
        }
    }
}