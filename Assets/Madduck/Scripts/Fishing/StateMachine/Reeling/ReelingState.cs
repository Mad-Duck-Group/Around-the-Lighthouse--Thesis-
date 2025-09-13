using System;
using Madduck.Scripts.Fishing.Controller.Reeling;
using Madduck.Scripts.Utils.Others;
using R3;
using VContainer;

namespace Madduck.Scripts.Fishing.StateMachine.Reeling
{
    public class ReelingState : FishingState
    {
        private readonly ReelingController _controller;
        private IDisposable _reelingResultSubscription;
        
        [Inject]
        public ReelingState(
            FishingStateMachine stateMachine,
            ReelingController controller)
            : base(stateMachine)
        {
            _controller = controller;
        }
        
        public override void Enter()
        {
            base.Enter();
            _controller.SetActive(true);
            _reelingResultSubscription = Observable.FromEvent<Sign>(
                    h => _controller.OnReelingResult += h,
                    h => _controller.OnReelingResult -= h)
                .Subscribe(OnReelingResult);
        }
        
        public override void Exit()
        {
            base.Exit();
            _reelingResultSubscription.Dispose();
            _controller.SetActive(false);
        }
        
        private void OnReelingResult(Sign result)
        {
            switch (result)
            {
                case Sign.Positive:
                    DebugUtils.Log("Fish reeled in successfully, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    _controller.Reset();
                    break;
                case Sign.Negative:
                    DebugUtils.Log("Max fatigue attempt reached, fish escaped, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    _controller.Reset();
                    break;
                case Sign.Zero:
                    DebugUtils.Log("Fish regained energy, transitioning to FishingBoardState");
                    stateMachine.PreviousState();
                    break;
                default:
                    DebugUtils.LogError($"Unexpected ReelingResult value: {result}");
                    break;
            }
        }
    }
}