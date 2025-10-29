using Madduck.Fishing.DI;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using PrimeTween;
using Unity.Behavior;
using UnityEngine;
using VContainer;
using Action = System.Action;

namespace Madduck.Fishing.Controller
{
    public interface IFishingBoardAIController
    {
        void InitializeBehaviorGraph();
        void UpdateBehaviourGraphVariables();
        void ShutdownBehaviorGraph();
        void SetFishPosition(Vector2 unitCircle);
        void MoveFishTimeBased(Vector2 unitCircle, float duration);
        void MoveFishSpeedBased(Vector2 unitCircle, float speed);
    }

    public class FishingBoardAIController : IFishingBoardAIController
    {

        #region Fields

        private readonly FishingStateMachineLifetimeScope _lifetimeScope;
        private readonly FishingBoardModel _model;
        private readonly FishingBoardVariables _variables;
        private readonly BehaviorGraphAgent _agent;
        private Tween _fishPositionTween;

        #endregion
        
        #region Blackboard Variables
        private BlackboardVariable<FishZone> _blackBoardFishZone;
        private BlackboardVariable<FishZone> _blackBoardHookZone;
        private BlackboardVariable<Vector2> _blackBoardFishUnitCirclePosition;
        private BlackboardVariable<Vector2> _blackBoardHookUnitCirclePosition;
        private BlackboardVariable<float> _blackBoardAngleDifference;
        private BlackboardVariable<float> _blackBoardFatiguePercent;
        #endregion

        #region Injection

        [Inject]
        public FishingBoardAIController(
            FishingStateMachineLifetimeScope lifetimeScope,
            FishingBoardModel model,
            FishingBoardVariables variables,
            BehaviorGraphAgent agent)
        {
            _lifetimeScope = lifetimeScope;
            _model = model;
            _variables = variables;
            _agent = agent;
        }

        #endregion
        
        #region AI Logic
        /// <summary>
        /// Initialize the behavior graph for fish behavior.
        /// </summary>
        public void InitializeBehaviorGraph()
        {
            _agent.enabled = true;
            _agent.Graph = _model.FishItemInstance.ItemData.BehaviorGraph;
            _agent.Init();
            _agent.GetVariable("FishZone", out _blackBoardFishZone);
            _agent.GetVariable("HookZone", out _blackBoardHookZone);
            _agent.GetVariable("FishUnitCirclePosition", out _blackBoardFishUnitCirclePosition);
            _agent.GetVariable("HookUnitCirclePosition", out _blackBoardHookUnitCirclePosition);
            _agent.GetVariable("AngleDifference", out _blackBoardAngleDifference);
            _agent.GetVariable("FatiguePercent", out _blackBoardFatiguePercent);
            _agent.SetVariableValue("FishingBoard", _lifetimeScope);
            _agent.Restart();
            _agent.Start();
        }

        /// <summary>
        /// Update the behavior graph variables with the current state of the fishing board.
        /// </summary>
        public void UpdateBehaviourGraphVariables()
        {
            _blackBoardFishZone.Value = (FishZone)(int)_variables.FishZone;
            _blackBoardHookZone.Value = (FishZone)(int)_variables.HookZone;
            _blackBoardFishUnitCirclePosition.Value = _variables.FishUnitCirclePosition;
            _blackBoardHookUnitCirclePosition.Value = _variables.HookUnitCirclePosition;
            _blackBoardAngleDifference.Value = _variables.AngleDifference;
            _blackBoardFatiguePercent.Value = _model.FatigueLevelPercent.CurrentValue.AsFraction;
        }

        /// <summary>
        /// Shutdown the behavior graph when the mini-game ends.
        /// </summary>
        public void ShutdownBehaviorGraph()
        {
            _agent.End();
            _agent.enabled = false;
        }
        
        /// <summary>
        /// Set the fish position based on the unit circle position.
        /// </summary>
        /// <param name="unitCircle">Unit circle position.</param>
        public void SetFishPosition(Vector2 unitCircle)
        {
            var circleCenter = _variables.RedBoard.Center;
            //var fishToCenter = (circleCenter - _model.FishPosition.Value).normalized;
            Vector2 position = unitCircle * _variables.RedBoard.Radius;
            _model.FishPosition.Value = position;
            //rotate facing the center
            var centerToFish = (circleCenter - _model.FishPosition.Value).normalized;
            var angle = Mathf.Atan2(centerToFish.y, centerToFish.x) * Mathf.Rad2Deg + 90f;
            _model.FishRotation.Value = Quaternion.Euler(0, 0, angle);
        }
        
        /// <summary>
        /// Move the fish to the target unit circle position over a duration.
        /// </summary>
        /// <param name="unitCircle">Target unit circle position.</param>
        /// <param name="duration">Duration of the movement in seconds.</param>
        public void MoveFishTimeBased(Vector2 unitCircle, float duration)
        {
            if (_fishPositionTween.isAlive) _fishPositionTween.Stop();
            var currentFishPosition = _variables.FishUnitCirclePosition;
            var targetFishPosition = unitCircle;
            _fishPositionTween = Tween.Custom(currentFishPosition, targetFishPosition, duration, 
                SetFishPosition);
        }
        
        /// <summary>
        /// Move the fish to the target unit circle position based on speed (units per second).
        /// </summary>
        /// <param name="unitCircle">Target unit circle position.</param>
        /// <param name="speed">Speed of the movement in units per second.</param>
        public void MoveFishSpeedBased(Vector2 unitCircle, float speed)
        {
            if (_fishPositionTween.isAlive) _fishPositionTween.Stop();
            var currentFishPosition = _variables.FishUnitCirclePosition;
            var targetFishPosition = unitCircle;
            var distance = Vector2.Distance(currentFishPosition, targetFishPosition);
            var duration = distance / speed;
            _fishPositionTween = Tween.Custom(currentFishPosition, targetFishPosition, duration, 
                SetFishPosition);
        }
        #endregion
    }
    
    public class FishingBoardAIControllerMock : IFishingBoardAIController
    {
        private readonly FishingBoardModel _model;
        private readonly FishingBoardVariables _variables;
        
        public FishingBoardAIControllerMock(
            FishingBoardModel model, 
            FishingBoardVariables variables)
        {
            _model = model;
            _variables = variables;
        }

        public void InitializeBehaviorGraph() { }
        
        public void UpdateBehaviourGraphVariables() { }
        public void ShutdownBehaviorGraph() { }
        
        public void SetFishPosition(Vector2 unitCircle)
        {
            var circleCenter = _variables.RedBoard.Center;
            Vector2 position = unitCircle * _variables.RedBoard.Radius;
            _model.FishPosition.Value = position;
            var centerToFish = (circleCenter - _model.FishPosition.Value).normalized;
            var angle = Mathf.Atan2(centerToFish.y, centerToFish.x) * Mathf.Rad2Deg + 90f;
            _model.FishRotation.Value = Quaternion.Euler(0, 0, angle);
        }

        public void MoveFishTimeBased(Vector2 unitCircle, float duration)
        {
            SetFishPosition(unitCircle);
        }

        public void MoveFishSpeedBased(Vector2 unitCircle, float speed)
        {
            SetFishPosition(unitCircle);
        }
    }
}