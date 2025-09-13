using VContainer;

namespace Madduck.Scripts.Fishing.StateMachine.None
{
    public class FishingNoneState : FishingState
    {
        [Inject]
        public FishingNoneState(FishingStateMachine stateMachine) : base(stateMachine) { }
    }
}