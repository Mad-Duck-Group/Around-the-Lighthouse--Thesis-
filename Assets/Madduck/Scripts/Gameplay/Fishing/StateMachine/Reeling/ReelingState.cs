using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class ReelingState : FishingState
    {
        private readonly ReelingController _controller;
        private IDisposable _reelingResultSubscription;
        private Sign _result;
        
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
            if (_result is Sign.Positive)
            {
                _controller.Reset();
            }
        }

        public override void Reset()
        {
            base.Reset();
            _controller.Reset();
        }

        private void OnReelingResult(Sign result)
        {
            _result = result;
            switch (result)
            {
                case Sign.Positive:
                    DebugUtils.Log("Reel in successfully, transition to CatchFishState");
                    stateMachine.ChangeState(FishingStateType.CatchFish);
                    break;
                case Sign.Negative:
                    DebugUtils.Log("Fish regained energy, transitioning to TugOfWarState");
                    stateMachine.ChangeState(FishingStateType.TugOfWar);
                    break;
                case Sign.Zero:
                default:
                    DebugUtils.LogError($"Unexpected ReelingResult value: {result}");
                    break;
            }
        }
    }
}