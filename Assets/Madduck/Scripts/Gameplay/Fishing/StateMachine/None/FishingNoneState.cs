using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Shared;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class FishingNoneState : FishingState
    {
        private readonly BubbleManager _bubbleManager;
        private readonly IRequestHandler<CanContinueFishingRequest, bool> _canContinueFishingRequestHandler;
        private readonly IIdleAnimator _playerAnimator;

        [Inject]
        public FishingNoneState(
            FishingStateMachine stateMachine,
            BubbleManager bubbleManager,
            IRequestHandler<CanContinueFishingRequest, bool> canContinueFishingRequestHandler,
            IIdleAnimator playerAnimator)
            : base(stateMachine)
        {
            _bubbleManager = bubbleManager;
            _canContinueFishingRequestHandler = canContinueFishingRequestHandler;
            _playerAnimator = playerAnimator;
        }

        public override async UniTask Enter()
        {
            await base.Enter();
            _playerAnimator.StartIdle();
            _bubbleManager.ResumeAllBubbles();
            if (_canContinueFishingRequestHandler.Invoke(new CanContinueFishingRequest()))
            {
                DebugUtils.Log("Can continue fishing, going to next state.");
                stateMachine.NextState();
            }
        }
    }
}