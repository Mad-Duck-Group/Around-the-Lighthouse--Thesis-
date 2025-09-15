using System;
using Madduck.Utils;

namespace Madduck.Fishing.StateMachine
{
    [Serializable]
    public abstract class FishingState : State
    {
        protected readonly FishingStateMachine stateMachine;
        
        public FishingState(FishingStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
    }
}