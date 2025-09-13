using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Scripts.Utils.Others;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer.Unity;

namespace Madduck.Scripts.Fishing.StateMachine
{
    public enum FishingStateType
    {
        None = 0,
        ThrowHook = 1,
        Nibble = 2,
        FishingBoard = 3,
        Reeling = 4,
    }
    
    [Serializable]
    public class FishingStateMachine : Utils.Others.StateMachine, IStartable
    {
        [Title("Debug")]
        [DisplayAsString]
        [ShowInInspector] private FishingStateType _currentStateType = FishingStateType.None;
        [ReadOnly]
        [ShowInInspector] private Dictionary<FishingStateType, FishingState> _states = new();
        [Button("Test Next State")]
        private void TestNextState() => NextState();
        [Button("Test Previous State")]
        private void TestPreviousState() => PreviousState();

        public void Start()
        {
            ChangeState(_currentStateType);
        }
        
        public void AddState(FishingStateType stateType, FishingState state)
        {
            if (!_states.TryAdd(stateType, state))
            {
                DebugUtils.LogWarning($"State {stateType} already exists in FishingStateMachine.");
            }
        }

        public void NextState()
        {
            var nextStateType = _currentStateType + 1;
            var maxStateType = _currentStateType.Max();
            if (nextStateType > maxStateType)
            {
                DebugUtils.LogWarning("Already in the last state, cannot go to next state.");
                return;
            }
            ChangeState(nextStateType);
        }
        
        public void PreviousState()
        {
            var previousStateType = _currentStateType - 1;
            var minStateType = _currentStateType.Min();
            if (previousStateType < minStateType)
            {
                DebugUtils.LogWarning("Already in the first state, cannot go to previous state.");
                return;
            }
            ChangeState(previousStateType);
        }
        
        public void ChangeState(FishingStateType stateType)
        {
            if (_currentStateType == stateType) return;
            if (_states.TryGetValue(stateType, out var nextState))
            {
                _currentStateType = stateType;
                ChangeState(nextState);
            }
            else
            {
                DebugUtils.LogError($"State {stateType} does not exist in FishingStateMachine.");
            }
        }
    }
}