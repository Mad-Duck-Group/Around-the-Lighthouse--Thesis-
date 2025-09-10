using System;
using Madduck.Scripts.Utils.Editor;
using Madduck.Scripts.Utils.Others;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.Fishing.StateMachine
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