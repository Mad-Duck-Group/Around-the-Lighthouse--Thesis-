using System;
using Madduck.Scripts.Fishing.Controller.Nibble;
using Madduck.Scripts.Utils.Others;
using R3;
using VContainer;

namespace Madduck.Scripts.Fishing.StateMachine.Nibble
{
    public class NibbleState : FishingState
    {
        private readonly NibbleController _controller;
        private IDisposable _pullHookResultSubscription;
        
        [Inject]
        public NibbleState(
            FishingStateMachine stateMachine,
            NibbleController controller) 
            : base(stateMachine)
        {
            _controller = controller;
        }
        
        public override void Enter()
        {
            base.Enter();
            _controller.SetActive(true);
            _pullHookResultSubscription = Observable.FromEvent<Sign>(
                    h => _controller.OnPullHookResult += h,
                    h => _controller.OnPullHookResult -= h)
                .Subscribe(OnPullHookResult);
        }
        
        public override void Exit()
        {
            base.Exit();
            _pullHookResultSubscription.Dispose();
            _controller.SetActive(false);
            _controller.Reset();
        }
        
        private void OnPullHookResult(Sign result)
        {
            switch (result)
            {
                case Sign.Positive:
                    DebugUtils.Log("Hook pulled while nibbling, transitioning to FishingBoardState");
                    stateMachine.NextState();
                    break;
                case Sign.Negative:
                    DebugUtils.Log("Hook pulled while not nibbling, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    break;
                case Sign.Zero:
                default:
                    DebugUtils.LogError($"Unexpected PullHookResult value: {result}");
                    break;
            }
        }
    }
}