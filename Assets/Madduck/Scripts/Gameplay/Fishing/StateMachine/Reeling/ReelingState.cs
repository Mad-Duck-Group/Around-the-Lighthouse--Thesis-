using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class ReelingState : FishingState
    {
        private readonly ReelingController _controller;
        private IDisposable _reelingResultSubscription;
        private bool _shouldReset;
        
        [Inject]
        public ReelingState(
            FishingStateMachine stateMachine,
            ReelingController controller)
            : base(stateMachine)
        {
            _controller = controller;
        }
        
        public override async UniTask Enter()
        {
            await base.Enter();
            await _controller.SetActive(true);
            _reelingResultSubscription = Observable.FromEvent<Sign>(
                    h => _controller.OnReelingResult += h,
                    h => _controller.OnReelingResult -= h)
                .Subscribe(OnReelingResult);
        }
        
        public override async UniTask Exit()
        {
            await base.Exit();
            _reelingResultSubscription.Dispose();
            await _controller.SetActive(false);
            if (_shouldReset) _controller.Reset();
            _shouldReset = false;
        }
        
        private void OnReelingResult(Sign result)
        {
            switch (result)
            {
                case Sign.Positive:
                    DebugUtils.Log("Fish reeled in successfully, transitioning to NoneState");
                    _shouldReset = true;
                    stateMachine.ChangeState(FishingStateType.None);
                    stateMachine.ResetState(FishingStateType.FishingBoard);
                    break;
                case Sign.Negative:
                    DebugUtils.Log("Max fatigue attempt reached, fish escaped, transitioning to NoneState");
                    _shouldReset = true;
                    stateMachine.ChangeState(FishingStateType.None);
                    stateMachine.ResetState(FishingStateType.FishingBoard);
                    break;
                case Sign.Zero:
                    DebugUtils.Log("Fish regained energy, transitioning to FishingBoardState");
                    _shouldReset = false;
                    stateMachine.PreviousState();
                    break;
                default:
                    DebugUtils.LogError($"Unexpected ReelingResult value: {result}");
                    break;
            }
        }
    }
}