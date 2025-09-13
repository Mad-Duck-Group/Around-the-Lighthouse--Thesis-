using System;
using Cysharp.Threading.Tasks;
using Madduck.Scripts.Audio;
using Madduck.Scripts.Fishing.Config.FishingBoard;
using Madduck.Scripts.Fishing.UI.FishingBoard;
using Madduck.Scripts.Fishing.UI.ThrowHook;
using Madduck.Scripts.Input;
using Madduck.Scripts.Utils.Others;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.Fishing.Controller.FishingBoard
{
    /// <summary>
    /// Controller for the Fishing Board mini-game. Handles non-UI input and AI logic.
    /// </summary>
    public class FishingBoardController : IDisposable
    {
        #region Inspector
        [Title("Debug")] 
        [DisplayAsString] 
        [ShowInInspector] private float _angleDifference;
        [DisplayAsString] 
        [ShowInInspector] private float _pullPercent;
        [DisplayAsString] 
        [ShowInInspector] private Vector2 _fishUnitCirclePosition;
        [DisplayAsString] 
        [ShowInInspector] private Vector2 _hookUnitCirclePosition;
        [DisplayAsString] 
        [ShowInInspector] private FishZone _fishZone;
        [DisplayAsString] 
        [ShowInInspector] private FishZone _hookZone;
        [DisplayAsString] 
        [ShowInInspector] private float _fishPowerMultiplier;
        [DisplayAsString] 
        [ShowInInspector] private float _hookPowerMultiplier;
        #endregion
        
        #region Fields
        public event Action<Sign> OnFishingBoardResult;
        private readonly FishingBoardModel _model;
        private readonly PlayerInputHandler _playerInput;
        private readonly FishingBoardConfig _config;
        private readonly BehaviorGraphAgent _agent;
        private readonly AudioManager _audioManager;
        private readonly ThrowHookProjectileFactory _factory;
        
        private IDisposable _updateSubscription;
        private IDisposable _bindings;
        private AudioReference _fishingLineTensionSfx;
        private Tween _fishPositionTween;
        
        #region Blackboard Variables
        private BlackboardVariable<BlackboardFishZone> _blackBoardFishZone;
        private BlackboardVariable<BlackboardFishZone> _blackBoardHookZone;
        private BlackboardVariable<Vector2> _blackBoardFishUnitCirclePosition;
        private BlackboardVariable<Vector2> _blackBoardHookUnitCirclePosition;
        private BlackboardVariable<float> _blackBoardAngleDifference;
        private BlackboardVariable<float> _blackBoardFatiguePercent;
        #endregion
        #endregion

        #region Injection
        [Inject]
        public FishingBoardController(
            FishingBoardModel model, 
            PlayerInputHandler playerInput,
            FishingBoardConfig config,
            BehaviorGraphAgent agent,
            AudioManager audioManager,
            ThrowHookProjectileFactory factory)
        {
            _model = model;
            _playerInput = playerInput;
            _config = config;
            _agent = agent;
            _audioManager = audioManager;
            _factory = factory;
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
                        _fishUnitCirclePosition = GetUnitCircle(x);
                        _fishZone = GetFishZone(_fishUnitCirclePosition.magnitude);
                        _fishPowerMultiplier = GetPowerMultiplier(_fishUnitCirclePosition);
                    })
                    .AddTo(ref disposableBuilder);
            _model.HookPosition
                    .Subscribe(x =>
                    {
                        FindFishAngle();
                        _hookUnitCirclePosition = GetUnitCircle(x);
                        _hookZone = GetFishZone(_hookUnitCirclePosition.magnitude);
                        _hookPowerMultiplier = GetPowerMultiplier(_hookUnitCirclePosition);
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
            _bindings?.Dispose();
            _model.Dispose();
        }
        #endregion
        
        #region Activation
        public void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
                SetFishPosition(Vector2.zero);
                SetHookPosition(Vector2.zero);
                StartFishingBoard();
            }
            else
            {
                StopFishingBoard();
            }
            _model.IsActive.Value = active;
        }

        public void Reset()
        {
            _model.Reset();
        }
        #endregion

        #region Fishing Board
        /// <summary>
        /// Start the fishing board mini-game.
        /// </summary>
        private void StartFishingBoard()
        {
            ResetFatigueLevel();
            InitializeBehaviorGraph();
            _model.MaxFatigueLevel.Value = _config.MaxFatigueLevel;
            _fishingLineTensionSfx = _audioManager.PlayAudio(_config.FishingLineTensionSfx, Vector3.zero);
            _updateSubscription = Observable.EveryUpdate().Subscribe(_ => Update());
        }

        /// <summary>
        /// Update the fishing board state. Called every frame until the mini-game ends.
        /// </summary>
        private void Update()
        {
            UpdateFatigueLevel();
            UpdateFishingLineDurability();
            UpdateBehaviourGraphVariables();
        }

        /// <summary>
        /// Stop the fishing board mini-game.
        /// </summary>
        private void StopFishingBoard()
        {
            _updateSubscription?.Dispose();
            MoveFishTimeBased(Vector2.zero, 1f);
            PlayTensionSound(_model.FishingLineDurabilityPercent.CurrentValue);
            ShutdownBehaviorGraph();
            _agent.enabled = false;
            ResetFatigueLevel();
            _audioManager.StopAudio(_fishingLineTensionSfx);
        }
        
        /// <summary>
        /// Reset the fatigue level to half of the maximum.
        /// </summary>
        private void ResetFatigueLevel()
        {
            _model.CurrentFatigueLevel.Value = _config.MaxFatigueLevel / 2;
        }

        /// <summary>
        /// Update the fatigue level based on the fishing rod and fish power.
        /// </summary>
        private void UpdateFatigueLevel()
        {
            var fishPower = _model.FishItemInstance.FishItemData.FishBehaviorData.Power;
            var rodPower = _model.FishingRodItemInstance.CurrentPower;
            var fishMultiplier = _fishPowerMultiplier;
            var hookMultiplier = _hookPowerMultiplier;
            var pullPercent = _pullPercent;
            var fatigue = (rodPower * hookMultiplier * pullPercent) - (fishPower * fishMultiplier);
            var currentFatigue = _model.CurrentFatigueLevel.Value;
            currentFatigue += fatigue * Time.deltaTime;
            currentFatigue = Mathf.Clamp(currentFatigue, 0, _config.MaxFatigueLevel);
            _model.CurrentFatigueLevel.Value = currentFatigue;
            if (currentFatigue <= 0)
            {
                LoseFishingBoard().Forget();  
            }
            if (currentFatigue >= _config.MaxFatigueLevel)
            {
                WinFishingBoard();
            }
        }

        /// <summary>
        /// Update the fishing line durability based on the tension from the fish and rod.
        /// </summary>
        private void UpdateFishingLineDurability()
        {
            var currentRod = _model.FishingRodItemInstance;
            var currentFish = _model.FishItemInstance;
            var fishPower = currentFish.FishItemData.FishBehaviorData.Power;
            var rodPower = currentRod.CurrentPower;
            var fishMultiplier = _fishPowerMultiplier;
            var hookMultiplier = _hookPowerMultiplier;
            var fishingLineTension = (rodPower * hookMultiplier) + (fishPower * fishMultiplier);
            var regenFactor = currentRod.CurrentFishingLineRegenFactor;
            var final = regenFactor - fishingLineTension;
            var currentDurability = currentRod.CurrentFishingLineDurability;
            currentDurability += final * Time.deltaTime;
            currentDurability = Mathf.Clamp(currentDurability,
                0, currentRod.BaseStats.FishingLineDurability);
            currentRod.CurrentFishingLineDurability = currentDurability;
            PlayTensionSound(_model.FishingLineDurabilityPercent.CurrentValue);
            if (currentDurability <= 0)
            {
                LoseFishingBoard().Forget();
            }
        }
        
        /// <summary>
        /// Play the fishing line tension sound based on the durability percentage.
        /// </summary>
        /// <param name="durabilityPercent">The current durability percentage of the fishing line.</param>
        private void PlayTensionSound(float durabilityPercent)
        {
            _fishingLineTensionSfx.eventInstance.setParameterByName("Tension", 1 - durabilityPercent);
        }

        /// <summary>
        /// Called when the player loses the fishing board mini-game.
        /// </summary>
        private async UniTaskVoid LoseFishingBoard()
        {
            SetActive(false);
            await _factory.CurrentHook.Return();
            _factory.DestroyHook();
            OnFishingBoardResult?.Invoke(Sign.Negative);
        }

        /// <summary>
        /// Called when the player wins the fishing board mini-game.
        /// </summary>
        private void WinFishingBoard()
        {
            OnFishingBoardResult?.Invoke(Sign.Positive);
        }
        #endregion
        
        #region Input
        /// <summary>
        /// Move the hook based on mouse delta input.
        /// </summary>
        /// <param name="delta">Mouse delta input.</param>
        private void MoveHook(Vector2 delta)
        {
            var hookPosition = _model.HookPosition.Value;
            var redBoard = _model.CircleBoardState[FishZone.Red];
            var mouseDelta = delta * _config.MouseSensitivity;
            var circleCenter = redBoard.Center;
            var hookToCenter = (circleCenter - hookPosition).normalized;
            var inertiaForce = (1 - _model.FishingLineDurabilityPercent.CurrentValue) * _config.Inertia;
            hookPosition += hookToCenter * (inertiaForce * Time.deltaTime);
            hookPosition += mouseDelta * Time.deltaTime;
            _model.HookPosition.Value = ClampPosition(hookPosition);
            //rotate toward the center
            var centerToHook = (circleCenter - hookPosition).normalized;
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
        /// Initialize the behavior graph for fish behavior.
        /// </summary>
        private void InitializeBehaviorGraph()
        {
            _agent.enabled = true;
            _agent.Graph = _model.FishItemInstance.FishItemData.FishBehaviorData.BehaviorGraph;
            _agent.Init();
            _agent.GetVariable("FishZone", out _blackBoardFishZone);
            _agent.GetVariable("HookZone", out _blackBoardHookZone);
            _agent.GetVariable("FishUnitCirclePosition", out _blackBoardFishUnitCirclePosition);
            _agent.GetVariable("HookUnitCirclePosition", out _blackBoardHookUnitCirclePosition);
            _agent.GetVariable("AngleDifference", out _blackBoardAngleDifference);
            _agent.GetVariable("FatiguePercent", out _blackBoardFatiguePercent);
            _agent.Restart();
            _agent.Start();
        }

        /// <summary>
        /// Update the behavior graph variables with the current state of the fishing board.
        /// </summary>
        private void UpdateBehaviourGraphVariables()
        {
            _blackBoardFishZone.Value = (BlackboardFishZone)(int)_fishZone;
            _blackBoardHookZone.Value = (BlackboardFishZone)(int)_hookZone;
            _blackBoardFishUnitCirclePosition.Value = _fishUnitCirclePosition;
            _blackBoardHookUnitCirclePosition.Value = _hookUnitCirclePosition;
            _blackBoardAngleDifference.Value = _angleDifference;
            _blackBoardFatiguePercent.Value = _model.FatigueLevelPercent.CurrentValue;
        }

        /// <summary>
        /// Shutdown the behavior graph when the mini-game ends.
        /// </summary>
        private void ShutdownBehaviorGraph()
        {
            _agent.End();
            _agent.enabled = false;
        }
        
        /// <summary>
        /// Find the angle difference between the fish and the hook relative to the center of the circle board.
        /// </summary>
        private void FindFishAngle()
        {
            Vector2 circleCenter = _model.CircleBoardState[FishZone.Red].Center;
            Vector2 pullDirection = _model.HookPosition.Value - circleCenter;
            Vector2 fishDirection = _model.FishPosition.Value - circleCenter;
            _angleDifference = Vector2.Angle(pullDirection, fishDirection);
            _pullPercent = _angleDifference / 180f;
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
        private float GetPowerMultiplier(Vector2 unitCircle)
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
        private void SetHookPosition(Vector2 unitCircle)
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
            var currentFishPosition = _fishUnitCirclePosition;
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
            var currentFishPosition = _fishUnitCirclePosition;
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
        private Vector2 GetRandomPositionOnFishZone(FishZone fishZone)
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
        public Vector2 GetRandomPositionOnFishZoneBlackboard(BlackboardFishZone blackboardFishZone)
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