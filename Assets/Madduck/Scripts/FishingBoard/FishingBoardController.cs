using System;
using Madduck.Scripts.FishingBoard.UI.Model;
using Madduck.Scripts.FishingBoard.UI.View;
using Madduck.Scripts.Input;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.FishingBoard
{
    public enum FishZone
    {
        Green,
        Yellow,
        Red
    }
    
    public class FishingBoardController : IDisposable
    {
        #region Inspector
        [Title("Debug")]
        [DisplayAsString]
        [ShowInInspector] public float AngleDifference {get; private set;}
        [DisplayAsString]
        [ShowInInspector] public float PullPercent {get; private set;}
        [DisplayAsString]
        [ShowInInspector] public Vector2 FishUnitCirclePosition {get; private set;}
        [DisplayAsString]
        [ShowInInspector] public Vector2 HookUnitCirclePosition {get; private set;}
        [DisplayAsString]
        [ShowInInspector] public FishZone FishZone { get; private set; }
        [DisplayAsString]
        [ShowInInspector] public FishZone HookZone { get; private set; }
        [DisplayAsString]
        [ShowInInspector] public float FishPowerMultiplier { get; private set; }
        [DisplayAsString]
        [ShowInInspector] public float HookPowerMultiplier { get; private set; }
        #endregion
        
        private readonly FishingBoardView _view;
        private readonly FishingBoardModel _model;
        private readonly PlayerInputHandler _playerInput;
        private readonly FishingBoardConfig _config;
        private IDisposable _bindings;
        private Tween _fishPositionTween;

        #region Injection
        [Inject]
        public FishingBoardController(FishingBoardView view, 
            FishingBoardModel model, 
            PlayerInputHandler playerInput,
            FishingBoardConfig config)
        {
            _view = view;
            _model = model;
            _playerInput = playerInput;
            _config = config;
            Bind();
        }
        #endregion
        
        #region Bindings
        private void Bind()
        {
            _bindings?.Dispose();
            var fishPositionBinding =
                _model.FishPosition
                    .Subscribe(x =>
                    {
                        FindFishAngle();
                        FishUnitCirclePosition = GetUnitCircle(x);
                        FishZone = GetFishZone(FishUnitCirclePosition.magnitude);
                        FishPowerMultiplier = GetPowerMultiplier(FishUnitCirclePosition);
                    });
            var hookPositionBinding =
                _model.HookPosition
                    .Subscribe(x =>
                    {
                        FindFishAngle();
                        HookUnitCirclePosition = GetUnitCircle(x);
                        HookZone = GetFishZone(HookUnitCirclePosition.magnitude);
                        HookPowerMultiplier = GetPowerMultiplier(HookUnitCirclePosition);
                    });
            var mouseDeltaBinding =
                _playerInput.MouseDelta
                    .Subscribe(MoveHook);
            _bindings = Disposable.Combine(fishPositionBinding, hookPositionBinding, mouseDeltaBinding);
        }
        #endregion
        
        #region Lifecycle
        public void SetActive(bool active)
        {
            if (active)
            {
                Bind();
                _model.IsActive.Value = true;
                SetFishPosition(Vector2.zero);
                SetHookPosition(Vector2.zero);
            }
            else
            {
                _model.IsActive.Value = false;
                _bindings?.Dispose();
            }
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
            _model.Dispose();
        }
        #endregion
        
        #region Updates
        private void FindFishAngle()
        {
            Vector2 circleCenter = _view.CircleBoards[FishZone.Red].Circle.localPosition;
            Vector2 pullDirection = _model.HookPosition.Value - circleCenter;
            Vector2 fishDirection = _model.FishPosition.Value - circleCenter;
            AngleDifference = Vector2.Angle(pullDirection, fishDirection);
            PullPercent = AngleDifference / 180f;
        }
        
        private void MoveHook(Vector2 delta)
        {
            var redBoard = _view.CircleBoards[FishZone.Red];
            var mouseDelta = delta * _config.MouseSensitivity;
            var circleCenter = (Vector2)redBoard.Circle.localPosition;
            var hookToCenter = (circleCenter - _model.HookPosition.Value).normalized;
            var inertiaForce = (1 - _model.FishingLineDurabilityPercent.CurrentValue) * _config.Inertia;
            _model.HookPosition.Value += hookToCenter * (inertiaForce * Time.deltaTime);
            _model.HookPosition.Value += new Vector2(mouseDelta.x, mouseDelta.y) * Time.deltaTime;
            //rotate toward the center
            var centerToHook = (circleCenter - _model.HookPosition.Value).normalized;
            var angle = Mathf.Atan2(centerToHook.y, centerToHook.x) * Mathf.Rad2Deg + 90f;
            _model.HookRotation.Value = Quaternion.Euler(0, 0, angle);
        }
        #endregion
        
        #region Utils

        private FishZone GetFishZone(float magnitude)
        {
            var greenThreshold = _view.CircleBoards[FishZone.Green].Radius / _view.CircleBoards[FishZone.Red].Radius;
            var yellowThreshold = _view.CircleBoards[FishZone.Yellow].Radius / _view.CircleBoards[FishZone.Red].Radius;
            var redThreshold = _view.CircleBoards[FishZone.Red].Radius / _view.CircleBoards[FishZone.Red].Radius;
            if (magnitude <= greenThreshold)
            {
                return FishZone.Green;
            }
            if (magnitude<= yellowThreshold)
            {
                return FishZone.Yellow;
            }
            if (magnitude <= redThreshold)
            {
                return FishZone.Red;
            }
            return FishZone.Green;
        }
        
        private Vector2 GetUnitCircle(Vector2 position)
        {
            return position / _view.CircleBoards[FishZone.Red].Radius;
        }

        public float GetPowerMultiplier(Vector2 unitCircle)
        {
            var fishZone = GetFishZone(unitCircle.magnitude);
            var index = (int)fishZone;
            var previousIndex = Mathf.Max(0, index - 1);
            var previousBoard = _view.CircleBoards[(FishZone)previousIndex];
            var previousThreshold = previousIndex == index ? 0 : previousBoard.Radius / _view.CircleBoards[FishZone.Red].Radius;
            var currentBoard = _view.CircleBoards[fishZone];
            var lowerBound = currentBoard.MultiplierRange.x;
            var upperBound = currentBoard.MultiplierRange.y;
            var currentThreshold = currentBoard.Radius / _view.CircleBoards[FishZone.Red].Radius;
            var relativePercent = (unitCircle.magnitude - previousThreshold) / (currentThreshold - previousThreshold);
            var multiplier = Mathf.Lerp(lowerBound, upperBound, relativePercent);
            return multiplier;
        }
        
        public void SetHookPosition(Vector2 unitCircle)
        {
            var redBoard = _view.CircleBoards[FishZone.Red];
            var circleCenter = (Vector2)redBoard.Circle.localPosition;
            Vector2 position = unitCircle * redBoard.Radius;
            _model.HookPosition.Value = position;
            //rotate facing the center
            var centerToHook = (circleCenter - _model.HookPosition.Value).normalized;
            var angle = Mathf.Atan2(centerToHook.y, centerToHook.x) * Mathf.Rad2Deg + 90f;
            _model.HookRotation.Value = Quaternion.Euler(0, 0, angle);
        }
        
        public void SetFishPosition(Vector2 unitCircle)
        {
            var redBoard = _view.CircleBoards[FishZone.Red];
            var circleCenter = (Vector2)redBoard.Circle.localPosition;
            var fishToCenter = (circleCenter - _model.FishPosition.Value).normalized;
            Vector2 position = unitCircle * redBoard.Radius;
            _model.FishPosition.Value = position;
            //rotate facing the center
            var centerToFish = (circleCenter - _model.FishPosition.Value).normalized;
            var angle = Mathf.Atan2(centerToFish.y, centerToFish.x) * Mathf.Rad2Deg + 90f;
            _model.FishRotation.Value = Quaternion.Euler(0, 0, angle);
        }
        
        public void MoveFishTimeBased(Vector2 unitCircle, float duration)
        {
            if (_fishPositionTween.isAlive) _fishPositionTween.Stop();
            var currentFishPosition = FishUnitCirclePosition;
            var targetFishPosition = unitCircle;
            _fishPositionTween = Tween.Custom(currentFishPosition, targetFishPosition, duration, 
                SetFishPosition);
        }
        
        public void MoveFishSpeedBased(Vector2 unitCircle, float speed)
        {
            if (_fishPositionTween.isAlive) _fishPositionTween.Stop();
            var currentFishPosition = FishUnitCirclePosition;
            var targetFishPosition = unitCircle;
            var distance = Vector2.Distance(currentFishPosition, targetFishPosition);
            var duration = distance / speed;
            _fishPositionTween = Tween.Custom(currentFishPosition, targetFishPosition, duration, 
                SetFishPosition);
        }

        public Vector2 GetRandomPosition()
        {
            var randomPosition = UnityEngine.Random.insideUnitCircle.normalized;
            return randomPosition;
        }

        public Vector2 GetRandomPositionOnFishZone(FishZone fishZone)
        {
            var index = (int)fishZone;
            var previousIndex = Mathf.Max(0, index - 1);
            var currentBoard = _view.CircleBoards[fishZone];
            var previousBoard = _view.CircleBoards[(FishZone)previousIndex];
            var currentThreshold = currentBoard.Radius / _view.CircleBoards[FishZone.Red].Radius;
            var previousThreshold = previousIndex == index ? 0 : previousBoard.Radius / _view.CircleBoards[FishZone.Red].Radius;
            var threshold = UnityEngine.Random.Range(previousThreshold, currentThreshold);
            var randomPosition = UnityEngine.Random.insideUnitCircle.normalized * threshold;
            return randomPosition;
        }
        
        public Vector2 GetRandomPositionOnFishZone(int fishZone)
        {
            return GetRandomPositionOnFishZone((FishZone)fishZone);
        }
        
        public Vector2 GetUnitCircleFromTargetAngle(float angle)
        {
            var radian = angle * Mathf.Deg2Rad;
            var x = Mathf.Cos(radian);
            var y = Mathf.Sin(radian);
            return new Vector2(x, y).normalized;
        }
        
        public Vector2 GetRandomPositionFromUnitCircle(Vector2 unitCircle)
        {
            var multiplier = UnityEngine.Random.Range(0f, 1f);
            var randomPosition = unitCircle * multiplier;
            return randomPosition;
        }
        #endregion
    }
}