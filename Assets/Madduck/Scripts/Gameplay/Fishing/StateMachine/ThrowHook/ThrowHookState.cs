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
        private IDisposable _hookThrownSubscription;
        
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
            await _controller.SetActive(true);
            _hookThrownSubscription = Observable.FromEvent(
                    h => _controller.OnHookThrown += h,
                    h => _controller.OnHookThrown -= h)
                .Subscribe(_ => OnHookThrown());
        }
        
        private void OnHookThrown()
        {
            DebugUtils.Log("Hook thrown, transitioning to ThrowingHookState");
            stateMachine.NextState();
        }

        public override async UniTask Exit()
        {
            await base.Exit();
            _hookThrownSubscription.Dispose();
            _controller.SetActive(false).Forget();
        }
    }
}