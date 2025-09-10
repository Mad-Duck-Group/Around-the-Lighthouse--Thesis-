namespace Madduck.Scripts.Utils.Others
{
    public abstract class State
    {
        /// <summary>
        /// Call when entering the state.
        /// </summary>
        public virtual void Enter() { }
        /// <summary>
        /// Call every frame while in the state.
        /// </summary>
        public virtual void Update() { }
        /// <summary>
        /// Call when exiting the state.
        /// </summary>
        public virtual void Exit() { }
        /// <summary>
        /// Reset the state to its initial condition.
        /// </summary>
        public virtual void Reset() { }
        /// <summary>
        /// Complete the state, applying completion logic.
        /// </summary>
        public virtual void Complete() { }
        /// <summary>
        /// Handle failure within the state, applying failure logic.
        /// </summary>
        public virtual void Fail() { }
    }
    
    public abstract class StateMachine
    {
        private State _currentState;

        /// <summary>
        /// Changes the current state of the state machine.
        /// </summary>
        /// <param name="newState">New state to change to.</param>
        protected void ChangeState(State newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }
}
