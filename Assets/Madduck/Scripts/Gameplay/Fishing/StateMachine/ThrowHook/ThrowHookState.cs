using System;
using Madduck.Fishing.Controller;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class ThrowHookState : FishingState
    {
        private readonly ThrowHookController _controller;
        private IDisposable _hookHitWaterSubscription;
        
        [Inject]
        public ThrowHookState(
            FishingStateMachine stateMachine,
            ThrowHookController controller
            ) : base(stateMachine)
        {
            _controller = controller;
        }

        public override void Enter()
        {
            base.Enter();
            _controller.SetActive(true);
            _hookHitWaterSubscription = Observable.FromEvent(
                    h => _controller.OnHookHitWater += h,
                    h => _controller.OnHookHitWater -= h)
                .Subscribe(_ => OnHookHitWater());
        }
        
        private void OnHookHitWater()
        {
            DebugUtils.Log("Hook hit water, transitioning to NibbleState");
            stateMachine.NextState();
        }

        public override void Exit()
        {
            base.Exit();
            _hookHitWaterSubscription.Dispose();
            _controller.SetActive(false);
            _controller.Reset();
        }
    }
}