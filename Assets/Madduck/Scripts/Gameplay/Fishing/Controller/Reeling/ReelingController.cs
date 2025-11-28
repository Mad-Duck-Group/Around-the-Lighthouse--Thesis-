using System;
using System.Linq;
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
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
    [Serializable]
    public class ReelingController : IDisposable
    {
        #region Events

        public event Action<Sign> OnReelingResult;

        #endregion

        #region Fields

        private readonly ReelingConfig _config;
        private readonly ReelingCommander _commander;
        private readonly ReelingModel _model;
        private readonly FishingSharedVariable _sharedVariable;
        private readonly InputInstructionManager _inputInstructionManager;
        private readonly IAudioManager _audioManager;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly ITransitionable _viewTransition;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        
        private IDisposable _bindings;
        private IDisposable _fatigueTimer;
        private float _fatigueTimerProgress;
        [ShowInInspector] private bool _reeling;
        [ShowInInspector] private float _previousAngle;
        [ShowInInspector, InlineProperty] private float _accumulatedAngle;
        private CancellationTokenSource _transitionCts = new();
        private AudioReference _reelingAudioReference;

        #endregion

        #region Injection

        [Inject]
        public ReelingController(
            ReelingConfig config,
            ReelingCommander commander,
            ReelingModel model,
            FishingSharedVariable sharedVariable,
            InputInstructionManager inputInstructionManager,
            IAudioManager audioManager,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            IFishSpriteFactory fishSpriteFactory,
            [Key(FishingStateType.Reeling)] ITransitionable viewTransition,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _config = config;
            _hookFactory = hookFactory;
            _inputHandler = inputHandler;
            _sharedVariable = sharedVariable;
            _inputInstructionManager = inputInstructionManager;
            _audioManager = audioManager;
            _commander = commander;
            _model = model;
            _fishSpriteFactory = fishSpriteFactory;
            _viewTransition = viewTransition;
            _playerAnimator = playerAnimator;
        }

        #endregion

        #region Bindings

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            Observable.EveryUpdate(UnityFrameProvider.Update)
                .Where(_ => _inputHandler.CurrentControlScheme == "Mouse & Keyboard")
                .Subscribe(_ =>
                {
                    OnRotate(_inputHandler.MouseUnitCircle.CurrentValue, false);
                })
                .AddTo(ref disposableBuilder);
            Observable.EveryUpdate(UnityFrameProvider.Update)
                .Where(_ => _inputHandler.CurrentControlScheme  == "Gamepad")
                .Subscribe(_ =>
                {
                    OnRotate(_inputHandler.RightStickUnitCircle.CurrentValue, true);
                    
                })
                .AddTo(ref disposableBuilder);
            _model.HookPositionXPercent
                .Subscribe(x => _hookFactory.Current.SetPositionX(x))
                .AddTo(ref disposableBuilder);
            _model.ReelingPercent
                .Where(x => x == Percentage.Full)
                .Subscribe(_ => OnWinReeling())
                .AddTo(ref disposableBuilder);
            Observable.EveryUpdate(UnityFrameProvider.Update)
                .Where(_ => _reeling)
                .Subscribe(_ =>
                {
                    OnReelingHold();
                })
                .AddTo(ref disposableBuilder);
            Observable.Interval(TimeSpan.FromMilliseconds(100f))
               .Subscribe(_ =>
               {
                   OnAngleCheck();
               })
               .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
            _fatigueTimer?.Dispose();
        }

        #endregion
        
        #region Event Handlers

        private void OnReelingFirstHold()
        {
            _commander.OnReelingFirstHold.Execute(InputType.NonUI);
        }

        private void OnReelingHold()
        {
            _commander.OnReelingHold.Execute(InputType.NonUI);
        }

        private void OnReelingRelease()
        {
            _commander.OnReelingRelease.Execute(InputType.NonUI);
        }

        private void OnAngleCheck()
        {
            if (_accumulatedAngle >= (float)_config.RotationThreshold && !_reeling)
            {
                _reeling = true;
                _reelingAudioReference = _audioManager.PlayAudio(_config.ReelingSfx, Vector3.zero);
                OnReelingFirstHold();
            }
            else if (_accumulatedAngle < (float)_config.RotationThreshold && _reeling)
            {
                _reeling = false;
                _audioManager.StopAudio(_reelingAudioReference);
                OnReelingRelease();
            }
            _accumulatedAngle = 0;
        }
        
        private void OnRotate(Vector2 delta, bool gamepad)
        {
            var currentAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            var deltaAngle = Mathf.DeltaAngle(_previousAngle, currentAngle);
            var currentSign = deltaAngle == 0 ? Sign.Zero : (Sign)(int)Mathf.Sign(deltaAngle);
            _previousAngle = currentAngle;
            if (currentSign is not Sign.Positive) return; //NOTE: Only allow counter-clockwise, remove this line to allow both directions
            if (deltaAngle < (float)_config.RotationIdleThreshold) return;
            var absDeltaAngle = Mathf.Abs(deltaAngle);
            var sensitivity = gamepad ? (float)_config.GamepadSensitivity : (float)_config.MouseSensitivity;
            _accumulatedAngle += absDeltaAngle * sensitivity;
        }

        private void OnWinReeling()
        {
            _audioManager.StopAudio(_reelingAudioReference);
            _playerAnimator.Set(PlayerAnimationKey.IdleRod, 0, true);
            _model.Inventory.ChangeCurrentBaitAmount(-1);
            UniTask.WaitForEndOfFrame()
                .ContinueWith(() => OnReelingResult?.Invoke(Sign.Positive)); // Delay to avoid race condition
        }
        
        private void OnFishRegainConsciousness()
        {
            _audioManager.StopAudio(_reelingAudioReference);
            OnReelingResult?.Invoke(Sign.Negative);
        }

        #endregion

        #region Utils

        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            _previousAngle = 0f;
            _accumulatedAngle = 0f;
            var currentFishable = _sharedVariable.CurrentFishable;
            var isFish = currentFishable is FishItemInstance;
            if (active)
            {
                _model.SetFishInstance(currentFishable as FishItemInstance);
                if (!isFish) _model.SetMaxProgress(_hookFactory.Current.CurrentX.AsPercentage);
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _inputInstructionManager.Show(_config.ReelingInputInstructions, stream: 0);
                Bind();
                if (isFish) StartFatigueTimer();
            }
            else
            {
                _reeling = false;
                _commander.Reset();
                _fatigueTimer?.Dispose();
                if (isFish)
                {
                    var fatigueSlider = _fishSpriteFactory.Current.FatigueTimerView;
                    fatigueSlider.TransitionOut().Forget();
                }
                _inputInstructionManager.Show(Array.Empty<InputInstruction>(), stream: 0);
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }
        
        public void Reset()
        {
            _commander.Reset();
            _model.Reset();
        }

        private void StartFatigueTimer()
        {
            var fatigueDuration = _model.FishInstance.CurrentStats.CurrentFatigueDuration;
            var fatigueSlider = _fishSpriteFactory.Current.FatigueTimerView;
            _fatigueTimerProgress = 0f;
            fatigueSlider.TransitionIn();
            _fatigueTimer = Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    _fatigueTimerProgress += Time.deltaTime;
                    var percent = 
                        Percentage.Clamp01(Percentage.FromFraction(_fatigueTimerProgress / fatigueDuration));
                    fatigueSlider.SetFishFatigueTimerProgress(percent.AsInversePercentage);
                    if (percent != Percentage.Full) return;
                    _fatigueTimer.Dispose();
                    fatigueSlider.TransitionOut();
                    OnFishRegainConsciousness();
                });
        }
        
        #endregion
        
    }
}