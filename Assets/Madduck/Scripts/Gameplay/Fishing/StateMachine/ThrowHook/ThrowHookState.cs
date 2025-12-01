using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class ThrowHookState : FishingState
    {
        private readonly ThrowHookController _controller;
        private DisposableBag _subscriptions;
        
        [Inject]
        public ThrowHookState(
            FishingStateMachine stateMachine,
            ThrowHookController controller
            ) : base(stateMachine)
        {
            _controller = controller;
        }

        public override async UniTask Enter()
        {
            await base.Enter();
            _subscriptions = new DisposableBag();
            Observable.FromEvent(
                    h => _controller.OnHookThrown += h,
                    h => _controller.OnHookThrown -= h)
                .Subscribe(_ => OnHookThrown())
                .AddTo(ref _subscriptions);
            Observable.FromEvent(
                    h => _controller.OnThrowHookCanceled += h,
                    h => _controller.OnThrowHookCanceled -= h)
                .Subscribe(_ => OnThrowHookCanceled())
                .AddTo(ref _subscriptions);
        }
        
        public override async UniTask Exit()
        {
            await base.Exit();
            _subscriptions.Dispose();
            _controller.SetActive(false).Forget();
        }
        
        private void OnHookThrown()
        {
            DebugUtils.Log("Hook thrown, transitioning to ThrowingHookState");
            stateMachine.NextState();
        }
        
        private void OnThrowHookCanceled()
        {
            DebugUtils.Log("Throw hook canceled, transitioning to PrepareBaitState");
            stateMachine.PreviousState();
        }
    }
}