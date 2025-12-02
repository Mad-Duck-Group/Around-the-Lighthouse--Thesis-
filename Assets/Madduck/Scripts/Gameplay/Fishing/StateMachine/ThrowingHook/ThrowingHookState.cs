using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class ThrowingHookState : FishingState
    {
        private readonly ThrowHookController _controller;
        
        [Inject]
        public ThrowingHookState(
            FishingStateMachine stateMachine,
            ThrowHookController controller)
            : base(stateMachine)
        {
            _controller = controller;
        }

        public override async UniTask Enter()
        {
            await base.Enter();
            ThrowHook().Forget();
        }

        private async UniTaskVoid ThrowHook()
        {
            await _controller.ThrowHook();
            DebugUtils.Log("Hook hit water, transitioning to NibbleState");
            stateMachine.NextState();
        }

        public override async UniTask Exit()
        {
            await base.Exit();
            _controller.Reset();
        }
    }
}