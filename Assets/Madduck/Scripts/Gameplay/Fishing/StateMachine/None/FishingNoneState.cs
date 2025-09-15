using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class FishingNoneState : FishingState
    {
        [Inject]
        public FishingNoneState(FishingStateMachine stateMachine) : base(stateMachine) { }
    }
}