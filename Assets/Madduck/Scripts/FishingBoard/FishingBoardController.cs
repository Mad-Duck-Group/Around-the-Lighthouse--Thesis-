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
    
    /// <summary>
    /// Controller for the Fishing Board mini-game. Handles non-UI input and AI logic.
    /// </summary>
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
        
        #region Fields
        private readonly FishingBoardModel _model;
        private readonly PlayerInputHandler _playerInput;
        private readonly FishingBoardConfig _config;
        private IDisposable _bindings;
        private Tween _fishPositionTween;
        #endregion

        #region Injection
        [Inject]
        public FishingBoardController(
            FishingBoardModel model, 
            PlayerInputHandler playerInput,
            FishingBoardConfig config)
        {
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
            var disposableBuilder = Disposable.CreateBuilder();
            _model.FishPosition
                    .Subscribe(x =>
                    {
                        FindFishAngle();
                        FishUnitCirclePosition = GetUnitCircle(x);
                        FishZone = GetFishZone(FishUnitCirclePosition.magnitude);
                        FishPowerMultiplier = GetPowerMultiplier(FishUnitCirclePosition);
                    })
                    .AddTo(ref disposableBuilder);
            _model.HookPosition
                    .Subscribe(x =>
                    {
                        FindFishAngle();
                        HookUnitCirclePosition = GetUnitCircle(x);
                        HookZone = GetFishZone(HookUnitCirclePosition.magnitude);
                        HookPowerMultiplier = GetPowerMultiplier(HookUnitCirclePosition);
                    })
                    .AddTo(ref disposableBuilder);
            _playerInput.MouseDelta
                    .Subscribe(MoveHook)
                    .AddTo(ref disposableBuilder);
            _playerInput.GamepadHookControl
                    .Subscribe(MoveHook)
                    .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        #endregion
        
        #region Lifecycle
        public void Dispose()
        {
            _bindings.Dispose();
            _model.Dispose();
        }
        #endregion
        
        #region Activation
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
                _model.Reset();
                _bindings?.Dispose();
            }
        }
        #endregion
        
        #region Input
        /// <summary>
        /// Move the hook based on mouse delta input.
        /// </summary>
        /// <param name="delta">Mouse delta input.</param>
        private void MoveHook(Vector2 delta)
        {
            var redBoard = _model.CircleBoardState[FishZone.Red];
            var mouseDelta = delta * _config.MouseSensitivity;
            var circleCenter = redBoard.Center;
            var hookToCenter = (circleCenter - _model.HookPosition.Value).normalized;
            var inertiaForce = (1 - _model.FishingLineDurabilityPercent.CurrentValue) * _config.Inertia;
            var hookPosition = _model.HookPosition.Value;
            hookPosition += hookToCenter * (inertiaForce * Time.deltaTime);
            hookPosition += mouseDelta * Time.deltaTime;
            _model.HookPosition.Value = ClampPosition(hookPosition);
            //rotate toward the center
            var centerToHook = (circleCenter - _model.HookPosition.Value).normalized;
            var angle = Mathf.Atan2(centerToHook.y, centerToHook.x) * Mathf.Rad2Deg + 90f;
            _model.HookRotation.Value = Quaternion.Euler(0, 0, angle);
        }
        
        /// <summary>
        /// Clamp the position of the target within the circle board.
        /// </summary>
        /// <param name="target"></param>
        private Vector2 ClampPosition(Vector2 target)
        {
            var redBoard = _model.CircleBoardState[FishZone.Red];
            var centerToPosition = (redBoard.Center - target).normalized;
            var maxMagnitude = redBoard.Radius * centerToPosition.magnitude;
            return Vector2.ClampMagnitude(target, maxMagnitude);
        }
        #endregion
        
        #region AI Logic
        /// <summary>
        /// Find the angle difference between the fish and the hook relative to the center of the circle board.
        /// </summary>
        private void FindFishAngle()
        {
            Vector2 circleCenter = _model.CircleBoardState[FishZone.Red].Center;
            Vector2 pullDirection = _model.HookPosition.Value - circleCenter;
            Vector2 fishDirection = _model.FishPosition.Value - circleCenter;
            AngleDifference = Vector2.Angle(pullDirection, fishDirection);
            PullPercent = AngleDifference / 180f;
        }

        /// <summary>
        /// Get the fish zone based on the magnitude of the unit circle position.
        /// </summary>
        /// <param name="unitCircleMagnitude">Magnitude of the unit circle position (0 to 1).</param>
        /// <returns>Fish zone.</returns>
        private FishZone GetFishZone(float unitCircleMagnitude)
        {
            var greenThreshold = _model.CircleBoardState[FishZone.Green].Radius / _model.CircleBoardState[FishZone.Red].Radius;
            var yellowThreshold = _model.CircleBoardState[FishZone.Yellow].Radius / _model.CircleBoardState[FishZone.Red].Radius;
            var redThreshold = _model.CircleBoardState[FishZone.Red].Radius / _model.CircleBoardState[FishZone.Red].Radius;
            if (unitCircleMagnitude <= greenThreshold)
            {
                return FishZone.Green;
            }
            if (unitCircleMagnitude <= yellowThreshold)
            {
                return FishZone.Yellow;
            }
            if (unitCircleMagnitude <= redThreshold)
            {
                return FishZone.Red;
            }
            return FishZone.Green;
        }
        
        /// <summary>
        /// Get the unit circle position from the object position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Unit circle position.</returns>
        private Vector2 GetUnitCircle(Vector2 position)
        {
            return position / _model.CircleBoardState[FishZone.Red].Radius;
        }

        /// <summary>
        /// Get the power multiplier based on the unit circle position.
        /// </summary>
        /// <param name="unitCircle">Unit circle position.</param>
        /// <returns>Power multiplier.</returns>
        public float GetPowerMultiplier(Vector2 unitCircle)
        {
            var fishZone = GetFishZone(unitCircle.magnitude);
            var index = (int)fishZone;
            var previousIndex = Mathf.Max(0, index - 1);
            var previousBoard = _model.CircleBoardState[(FishZone)previousIndex];
            var previousThreshold = previousIndex == index ? 0 : previousBoard.Radius / _model.CircleBoardState[FishZone.Red].Radius;
            var currentBoard = _model.CircleBoardState[fishZone];
            var lowerBound = currentBoard.MultiplierRange.x;
            var upperBound = currentBoard.MultiplierRange.y;
            var currentThreshold = currentBoard.Radius / _model.CircleBoardState[FishZone.Red].Radius;
            var relativePercent = (unitCircle.magnitude - previousThreshold) / (currentThreshold - previousThreshold);
            var multiplier = Mathf.Lerp(lowerBound, upperBound, relativePercent);
            return multiplier;
        }
        
        /// <summary>
        /// Set the hook position based on the unit circle position.
        /// </summary>
        /// <param name="unitCircle">Unit circle position.</param>
        public void SetHookPosition(Vector2 unitCircle)
        {
            var redBoard = _model.CircleBoardState[FishZone.Red];
            var circleCenter = redBoard.Center;
            var position = unitCircle * redBoard.Radius;
            _model.HookPosition.Value = position;
            //rotate facing the center
            var centerToHook = (circleCenter - _model.HookPosition.Value).normalized;
            var angle = Mathf.Atan2(centerToHook.y, centerToHook.x) * Mathf.Rad2Deg + 90f;
            _model.HookRotation.Value = Quaternion.Euler(0, 0, angle);
        }
        
        /// <summary>
        /// Set the fish position based on the unit circle position.
        /// </summary>
        /// <param name="unitCircle">Unit circle position.</param>
        public void SetFishPosition(Vector2 unitCircle)
        {
            var redBoard = _model.CircleBoardState[FishZone.Red];
            var circleCenter = redBoard.Center;
            var fishToCenter = (circleCenter - _model.FishPosition.Value).normalized;
            Vector2 position = unitCircle * redBoard.Radius;
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
            var currentFishPosition = FishUnitCirclePosition;
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
            var currentFishPosition = FishUnitCirclePosition;
            var targetFishPosition = unitCircle;
            var distance = Vector2.Distance(currentFishPosition, targetFishPosition);
            var duration = distance / speed;
            _fishPositionTween = Tween.Custom(currentFishPosition, targetFishPosition, duration, 
                SetFishPosition);
        }

        /// <summary>
        /// Get a random position within the unit circle.
        /// </summary>
        /// <returns>Random position within the unit circle.</returns>
        public Vector2 GetRandomPosition()
        {
            var randomPosition = UnityEngine.Random.insideUnitCircle.normalized;
            return randomPosition;
        }

        /// <summary>
        /// Get a random position within the specified fish zone.
        /// </summary>
        /// <param name="fishZone">The fish zone to get the random position from.</param>
        /// <returns>Random position within the specified fish zone.</returns>
        public Vector2 GetRandomPositionOnFishZone(FishZone fishZone)
        {
            var index = (int)fishZone;
            var previousIndex = Mathf.Max(0, index - 1);
            var currentBoard = _model.CircleBoardState[fishZone];
            var previousBoard = _model.CircleBoardState[(FishZone)previousIndex];
            var currentThreshold = currentBoard.Radius / _model.CircleBoardState[FishZone.Red].Radius;
            var previousThreshold = previousIndex == index ? 0 : previousBoard.Radius / _model.CircleBoardState[FishZone.Red].Radius;
            var threshold = UnityEngine.Random.Range(previousThreshold, currentThreshold);
            var randomPosition = UnityEngine.Random.insideUnitCircle.normalized * threshold;
            return randomPosition;
        }
        
        /// <summary>
        /// Get a random position within the specified fish zone by casting from BlackboardFishZone to FishZone.
        /// </summary>
        /// <param name="blackboardFishZone">The fish zone to get the random position from.</param>
        /// <returns>Random position within the specified fish zone.</returns>
        public Vector2 GetRandomPositionOnFishZone(BlackboardFishZone blackboardFishZone)
        {
            return GetRandomPositionOnFishZone((FishZone)blackboardFishZone);
        }
        
        /// <summary>
        /// Get the unit circle position from a target angle in degrees.
        /// </summary>
        /// <param name="angle">Target angle in degrees.</param>
        /// <returns>Unit circle position.</returns>
        public Vector2 GetUnitCircleFromTargetAngle(float angle)
        {
            var radian = angle * Mathf.Deg2Rad;
            var x = Mathf.Cos(radian);
            var y = Mathf.Sin(radian);
            return new Vector2(x, y).normalized;
        }
        
        /// <summary>
        /// Get a random position within the specified unit circle by scaling the unit circle position with a random multiplier between 0 and 1.
        /// </summary>
        /// <param name="unitCircle">The unit circle position to scale.</param>
        /// <returns>Random position within the specified unit circle.</returns>
        public Vector2 GetRandomPositionFromUnitCircle(Vector2 unitCircle)
        {
            var multiplier = UnityEngine.Random.Range(0f, 1f);
            var randomPosition = unitCircle * multiplier;
            return randomPosition;
        }
        #endregion
    }
}