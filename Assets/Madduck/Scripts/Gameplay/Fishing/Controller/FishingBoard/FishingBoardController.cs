using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
    /// <summary>
    /// Controller for the Fishing Board mini-game. Handles non-UI input.
    /// </summary>
    public class FishingBoardController : IDisposable
    {
        #region Fields
        public event Action<Sign> OnFishingBoardResult;
        private readonly FishingBoardModel _model;
        private readonly FishingBoardVariables _variables;
        private readonly FishingBoardConfig _config;
        private readonly IAudioManager _audioManager;
        private readonly IPlayerInputHandler _playerInput;
        private readonly IFishingBoardAIController _aiController;
        private readonly IHookFactory _hookFactory;
        private readonly IGenericFactory<FishItemInstance> _fishFactory;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly ITransitionable _viewTransition;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        
        private IDisposable _updateSubscription;
        private IDisposable _bindings;
        private AudioReference _fishingLineTensionSfx;
        private CancellationTokenSource _transitionCts = new();
        private bool _thresholdReached;
        private const string ThrowEventName = "After_Throw";
        #endregion

        #region Injection
        [Inject]
        public FishingBoardController(
            FishingBoardModel model, 
            FishingBoardVariables variables,
            FishingBoardConfig config,
            IAudioManager audioManager,
            IPlayerInputHandler playerInput,
            IFishingBoardAIController aiController,
            IHookFactory hookFactory,
            IGenericFactory<FishItemInstance> fishFactory,
            IFishSpriteFactory fishSpriteFactory,
            [Key(FishingStateType.FishingBoard)] ITransitionable viewTransition,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _model = model;
            _variables = variables;
            _playerInput = playerInput;
            _config = config;
            _hookFactory = hookFactory;
            _audioManager = audioManager;
            _aiController = aiController;
            _fishFactory = fishFactory;
            _fishSpriteFactory = fishSpriteFactory;
            _viewTransition = viewTransition;
            _playerAnimator = playerAnimator;
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
                    _variables.FishUnitCirclePosition = _variables.GetUnitCircle(x);
                    _variables.FishZone = _variables.GetFishZone(_variables.FishUnitCirclePosition.magnitude);
                    _variables.FishPowerMultiplier = _variables.GetPowerMultiplier(_variables.FishUnitCirclePosition);
                })
                .AddTo(ref disposableBuilder);
            _model.HookPosition
                .Subscribe(x =>
                {
                    FindFishAngle();
                    _variables.HookUnitCirclePosition = _variables.GetUnitCircle(x);
                    _variables.HookZone = _variables.GetFishZone(_variables.HookUnitCirclePosition.magnitude);
                    _variables.HookPowerMultiplier = _variables.GetPowerMultiplier(_variables.HookUnitCirclePosition);
                })
                .AddTo(ref disposableBuilder);
            _playerInput.MouseDelta
                .Subscribe(x => MoveHook(x, false))
                .AddTo(ref disposableBuilder);
            _playerInput.LeftStickDelta
                .EveryUpdateWhen(x => x != Vector2.zero)
                .Select(_ => _playerInput.LeftStickDelta.CurrentValue)
                .Subscribe(x => MoveHook(x, true))
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
        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                _aiController.SetFishPosition(Vector2.zero);
                SetHookPosition(Vector2.zero);
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _model.SetFishInstance(_fishFactory.Current);
                Bind();
                StartFishingBoard();
                _playerAnimator.Set(PlayerAnimationKey.Pulling, 0, true);
                _fishSpriteFactory.Current.Animator.Set(FishSpriteAnimationKey.Pulling, 0, true);
            }
            else
            {
                StopFishingBoard();
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
                _aiController.SetFishPosition(Vector2.zero);
                SetHookPosition(Vector2.zero);
            }
        }

        public void Reset()
        {
            _thresholdReached = false;
            _model.Reset();
        }
        
        public void ResetCircleBoardSprite()
        {
            _variables.ResetCircleBoardSprite();
        }
        #endregion

        #region Fishing Board
        /// <summary>
        /// Start the fishing board mini-game.
        /// </summary>
        private void StartFishingBoard()
        {
            ResetFatigueLevel();
            _aiController.InitializeBehaviorGraph();
            _model.MaxFatigueLevel.Value = _config.MaxFatigueLevel;
            _fishingLineTensionSfx = _audioManager.PlayAudio(_config.FishingLineTensionSfx, Vector3.zero);
            _updateSubscription = Observable.EveryUpdate().Subscribe(_ => Update());
        }

        /// <summary>
        /// Update the fishing board state. Called every frame until the mini-game ends.
        /// </summary>
        private void Update()
        {
            UpdateHookToCenter();
            UpdateFatigueLevel();
            UpdateFishingLineDurability();
            _aiController.UpdateBehaviourGraphVariables();
        }

        /// <summary>
        /// Stop the fishing board mini-game.
        /// </summary>
        private void StopFishingBoard()
        {
            _updateSubscription?.Dispose();
            _aiController.MoveFishTimeBased(Vector2.zero, 1f);
            PlayTensionSound(_model.FishingLineDurabilityPercent.CurrentValue);
            _aiController.ShutdownBehaviorGraph();
            _audioManager.StopAudio(_fishingLineTensionSfx);
        }
        
        /// <summary>
        /// Reset the fatigue level to half of the maximum.
        /// </summary>
        private void ResetFatigueLevel()
        {
            _model.CurrentFatigueLevel.Value = 0f;
        }

        /// <summary>
        /// Update the fatigue level based on the fishing rod and fish power.
        /// </summary>
        private void UpdateFatigueLevel()
        {
            var fishPower = (float)_model.FishItemInstance.CurrentStats.CurrentPower;
            var rodPower = (float)_model.FishingRodItemInstance.CurrentStats.CurrentPower;
            var fishResistance = (float)_model.FishItemInstance.CurrentStats.CurrentResistance;
            var rodResistance = (float)_model.FishingRodItemInstance.CurrentStats.CurrentResistance;
            var fishTotalPower = Mathf.Max(1, fishPower - rodResistance);
            var rodTotalPower = Mathf.Max(1, rodPower - fishResistance);
            var fishMultiplier = _variables.FishPowerMultiplier;
            var hookMultiplier = _variables.HookPowerMultiplier;
            var pullPercent = _variables.PullPercent;
            var fatigue = (rodTotalPower * hookMultiplier * pullPercent.AsFraction) - (fishTotalPower * fishMultiplier);
            var currentFatigue = (float)_model.CurrentFatigueLevel.Value;
            if (!_thresholdReached)
            {
                fatigue = Mathf.Max(0, fatigue);
            }
            currentFatigue += fatigue * Time.deltaTime;
            currentFatigue = Mathf.Clamp(currentFatigue, 0, _config.MaxFatigueLevel);
            _model.CurrentFatigueLevel.Value = currentFatigue;
            var fatiguePercent = _model.FatigueLevelPercent.CurrentValue;
            var decayThreshold = _model.FishingRodItemInstance.CurrentStats.CurrentFishingBoardDecayThreshold;
            if (!_thresholdReached && fatiguePercent >= decayThreshold)
            {
                _thresholdReached = true;
            }
            if (_thresholdReached && currentFatigue <= 0)
            {
                LoseFishingBoard();  
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
            var fishPower = (float)currentFish.CurrentStats.CurrentPower;
            var rodPower = (float)currentRod.CurrentStats.CurrentPower;
            var fishMultiplier = _variables.FishPowerMultiplier;
            var hookMultiplier = _variables.HookPowerMultiplier;
            var fishingLineTension = (rodPower * hookMultiplier) + (fishPower * fishMultiplier);
            var regenFactor = (float)currentRod.CurrentStats.CurrentFishingLineRegenFactor;
            var final = regenFactor - fishingLineTension;
            var currentDurability = (float)currentRod.CurrentStats.CurrentFishingLineDurability;
            currentDurability += final * Time.deltaTime;
            currentDurability = Mathf.Clamp(currentDurability,
                0, currentRod.ItemData.FishingLineDurability);
            currentRod.CurrentStats.CurrentFishingLineDurability = currentDurability;
            PlayTensionSound(_model.FishingLineDurabilityPercent.CurrentValue);
            if (currentDurability <= 0)
            {
                LoseFishingBoard();
            }
        }

        private void UpdateHookToCenter()
        {
            var hookPosition = _model.HookPosition.Value;
            var circleCenter = _variables.RedBoard.Center;
            var hookToCenter = (circleCenter - hookPosition).normalized;
            var fishUnitCirclePosition = _variables.FishUnitCirclePosition;
            var hookUnitCirclePosition = _variables.HookUnitCirclePosition;
            // var inertiaForce = _model.FishingLineDurabilityPercent.CurrentValue.AsInverseFraction * (float)_config.Inertia;
            var inertiaForce = Vector2.Distance(fishUnitCirclePosition, hookUnitCirclePosition) / 2f 
                               * (float)_model.FishingRodItemInstance.CurrentStats.CurrentHookToCenterForce;
            hookPosition += hookToCenter * (inertiaForce * Time.deltaTime);
            _model.HookPosition.Value = ClampPosition(hookPosition);
            //rotate toward the center
            var centerToHook = (circleCenter - hookPosition).normalized;
            var angle = Mathf.Atan2(centerToHook.y, centerToHook.x) * Mathf.Rad2Deg + 90f;
            _model.HookRotation.Value = Quaternion.Euler(0, 0, angle);
        }
        
        /// <summary>
        /// Play the fishing line tension sound based on the durability percentage.
        /// </summary>
        /// <param name="durabilityPercent">The current durability percentage of the fishing line.</param>
        private void PlayTensionSound(Percentage durabilityPercent)
        {
            _fishingLineTensionSfx.eventInstance.setParameterByName("Tension", durabilityPercent.AsInverseFraction);
        }

        /// <summary>
        /// Called when the player loses the fishing board mini-game.
        /// </summary>
        private void LoseFishingBoard()
        {
            _model.Inventory.ChangeCurrentBaitAmount(-1);
            OnFishingBoardResult?.Invoke(Sign.Negative);
            _fishSpriteFactory.Current.Animator.Set(FishSpriteAnimationKey.Idle, 0, true);
            _fishSpriteFactory.Current.Detach();
        }

        /// <summary>
        /// Called when the player wins the fishing board mini-game.
        /// </summary>
        private void WinFishingBoard()
        {
            _playerAnimator.Set(PlayerAnimationKey.IdleRod, 0, true);
            _fishSpriteFactory.Current.Animator.Set(FishSpriteAnimationKey.Exhausted, 0, true);
            OnFishingBoardResult?.Invoke(Sign.Positive);
        }

        public async UniTask ReturnHook()
        {
            await _playerAnimator.Set(PlayerAnimationKey.GotFish, 0, false).WaitUntilEvent(ThrowEventName);
            await UniTask.WhenAll(
                _hookFactory.Current.Return(),
                _fishSpriteFactory.Current.TransitionOut());
            _fishSpriteFactory.DestroyFishSprite();
            _hookFactory.DestroyHook();
        }
        #endregion
        
        #region Input

        /// <summary>
        /// Move the hook based on mouse delta input.
        /// </summary>
        /// <param name="delta">Mouse delta input.</param>
        /// <param name="gamepad"></param>
        private void MoveHook(Vector2 delta, bool gamepad)
        {
            var hookPosition = _model.HookPosition.Value;
            var sensitivity = gamepad ? _config.GamepadSensitivity : _config.MouseSensitivity;
            var mouseDelta = delta * sensitivity;
            var circleCenter = _variables.RedBoard.Center;
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
            var centerToPosition = (_variables.RedBoard.Center - target).normalized;
            var maxMagnitude = _variables.RedBoard.Radius * centerToPosition.magnitude;
            return Vector2.ClampMagnitude(target, maxMagnitude);
        }
        #endregion

        #region Utils

        /// <summary>
        /// Set the hook position based on the unit circle position.
        /// </summary>
        /// <param name="unitCircle">Unit circle position.</param>
        private void SetHookPosition(Vector2 unitCircle)
        {
            var circleCenter = _variables.RedBoard.Center;
            var position = unitCircle * _variables.RedBoard.Radius;
            _model.HookPosition.Value = position;
            //rotate facing the center
            var centerToHook = (circleCenter - _model.HookPosition.Value).normalized;
            var angle = Mathf.Atan2(centerToHook.y, centerToHook.x) * Mathf.Rad2Deg + 90f;
            _model.HookRotation.Value = Quaternion.Euler(0, 0, angle);
        }
        
        /// <summary>
        /// Find the angle difference between the fish and the hook relative to the center of the circle board.
        /// </summary>
        private void FindFishAngle()
        {
            Vector2 circleCenter = _variables.RedBoard.Center;
            Vector2 pullDirection = _model.HookPosition.Value - circleCenter;
            Vector2 fishDirection = _model.FishPosition.Value - circleCenter;
            _variables.AngleDifference = Vector2.Angle(pullDirection, fishDirection);
            _variables.PullPercent = Percentage.FromFraction(_variables.AngleDifference / 180f);
        }

        #endregion
    }
}