using System;
using Madduck.Scripts.Fishing.Controller.ThrowHook;
using Madduck.Scripts.Utils.Others;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.Fishing.StateMachine.ThrowHook
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