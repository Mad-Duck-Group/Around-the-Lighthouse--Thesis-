using System;
using Madduck.Scripts.Audio;
using Madduck.Scripts.Fishing.Config.FishingBoard;
using Madduck.Scripts.Fishing.Controller.FishingBoard;
using Madduck.Scripts.Fishing.UI.FishingBoard;
using Madduck.Scripts.Utils.Others;
using R3;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using VContainer.Unity;

namespace Madduck.Scripts.Fishing.StateMachine.FishingBoard
{
    /// <summary>
    /// State of the Fishing Board mini-game.
    /// </summary>
    [Serializable]
    public class FishingBoardState : FishingState
    {
        private readonly FishingBoardController _controller;
        private IDisposable _fishingBoardResultSubscription;
        
        public FishingBoardState(
            FishingStateMachine stateMachine,
            FishingBoardController controller) 
            : base(stateMachine)
        {
            _controller = controller;
        }
        
        public override void Enter()
        {
            base.Enter();
            _controller.SetActive(true);
            _fishingBoardResultSubscription = Observable.FromEvent<Sign>(
                    h => _controller.OnFishingBoardResult += h,
                    h => _controller.OnFishingBoardResult -= h)
                .Subscribe(OnFishingBoardResult);
        }

        public override void Exit()
        {
            base.Exit();
            _fishingBoardResultSubscription.Dispose();
            _controller.SetActive(false);
            _controller.Reset();
        }

        private void OnFishingBoardResult(Sign result)
        {
            switch (result)
            {
                case Sign.Negative:
                    DebugUtils.Log("Fish escaped, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    break;
                case Sign.Positive:
                    DebugUtils.Log("Fish is tired, transitioning to ReelingState");
                    stateMachine.NextState();
                    break;
                case Sign.Zero:
                default:
                    DebugUtils.LogError($"Unexpected FishingBoardResult value: {result}");
                    break;
            }
        }
    }
}