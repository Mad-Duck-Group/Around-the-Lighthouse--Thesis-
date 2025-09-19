using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Utils;
using MessagePipe;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class FishingNoneState : FishingState
    {
        private readonly IRequestHandler<CanContinueFishingRequest, bool> _canContinueFishingRequestHandler;

        [Inject]
        public FishingNoneState(
            FishingStateMachine stateMachine,
            IRequestHandler<CanContinueFishingRequest, bool> canContinueFishingRequestHandler)
            : base(stateMachine)
        {
            _canContinueFishingRequestHandler = canContinueFishingRequestHandler;
        }

        public override async UniTask Enter()
        {
            await base.Enter();
            if (_canContinueFishingRequestHandler.Invoke(new CanContinueFishingRequest()))
            {
                DebugUtils.Log("Can continue fishing, going to next state.");
                stateMachine.NextState();
            }
        }
    }
}