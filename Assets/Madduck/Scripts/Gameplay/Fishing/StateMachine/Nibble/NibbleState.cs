using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class NibbleState : FishingState
    {
        private readonly NibbleController _controller;
        private IDisposable _pullHookResultSubscription;
        private Sign _result;
        
        [Inject]
        public NibbleState(
            FishingStateMachine stateMachine,
            NibbleController controller) 
            : base(stateMachine)
        {
            _controller = controller;
        }
        
        public override async UniTask Enter()
        {
            await base.Enter();
            await _controller.SetActive(true);
            _pullHookResultSubscription = Observable.FromEvent<Sign>(
                    h => _controller.OnPullHookResult += h,
                    h => _controller.OnPullHookResult -= h)
                .Subscribe(OnPullHookResult);
        }
        
        public override async UniTask Exit()
        {
            await base.Exit();
            _pullHookResultSubscription.Dispose();
            await _controller.SetActive(false);
            if (_result is Sign.Negative) await _controller.ReturnHook();
            _controller.Reset();
        }
        
        private void OnPullHookResult(Sign result)
        {
            _result = result;
            switch (result)
            {
                case Sign.Positive:
                    DebugUtils.Log("Got fish, transitioning to FishingBoardState");
                    stateMachine.NextState();
                    break;
                case Sign.Negative:
                    DebugUtils.Log("Hook pulled while not nibbling, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    break;
                case Sign.Zero:
                    DebugUtils.Log("Got trash, transitioning to ReelingState");
                    stateMachine.ChangeState(FishingStateType.Reeling);
                    break;
                default:
                    DebugUtils.LogError($"Unexpected PullHookResult value: {result}");
                    break;
            }
        }
    }
}