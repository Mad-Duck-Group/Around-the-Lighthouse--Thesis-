using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class PrepareBaitState : FishingState
    {
        private readonly ThrowHookController _controller;
        private IDisposable _subscription;
        
        [Inject]
        public PrepareBaitState(
            FishingStateMachine stateMachine,
            ThrowHookController throwHookController) 
            : base(stateMachine)
        {
            _controller = throwHookController;
        }

        public override async UniTask Enter()
        {
            await base.Enter();
            await _controller.SetActive(true);
            _subscription = Observable.FromEvent(
                    h => _controller.OnThrowHookStarted += h,
                    h => _controller.OnThrowHookStarted -= h)
                .Subscribe(_ => OnThrowHookStarted());
        }
        
        public override async UniTask Exit()
        {
            await base.Exit();
            _subscription.Dispose();
        }

        private void OnThrowHookStarted()
        {
            DebugUtils.Log("Throw hook started, transitioning to ThrowHookState");
            stateMachine.NextState();
        }
    }
}