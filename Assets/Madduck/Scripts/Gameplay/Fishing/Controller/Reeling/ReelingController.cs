using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly IGenericFactory<FishItemInstance> _fishFactory;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly ITransitionable _viewTransition;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        
        private IDisposable _bindings;
        private IDisposable _fatigueTimer;
        private IDisposable _startSlowMo;
        private float _fatigueTimerProgress;
        [ShowInInspector] private bool _reeling;
        [ShowInInspector] private float _currentAngle;
        [ShowInInspector] private float _previousAngle;
        [ShowInInspector] private Sign _currentSign = Sign.Zero;
        // private Sign _previousSign = Sign.Zero;
        // private bool _changeDirection;
        //private bool _passedThreshold;
        [ShowInInspector, InlineProperty] private float _previousCheckAngle;
        [ShowInInspector, InlineProperty] private float _accumulatedAngle;
        private CancellationTokenSource _transitionCts = new();

        #endregion

        #region Injection

        [Inject]
        public ReelingController(
            ReelingConfig config,
            ReelingCommander commander,
            ReelingModel model,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            IGenericFactory<FishItemInstance> fishFactory,
            IFishSpriteFactory fishSpriteFactory,
            [Key(FishingStateType.Reeling)] ITransitionable viewTransition,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _config = config;
            _hookFactory = hookFactory;
            _inputHandler = inputHandler;
            _commander = commander;
            _model = model;
            _fishFactory = fishFactory;
            _fishSpriteFactory = fishSpriteFactory;
            _viewTransition = viewTransition;
            _playerAnimator = playerAnimator;
        }

        #endregion

        #region Bindings

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            // _inputHandler.Action0Button.IsDown
            //     .IgnoreFirstValueWhenSubscribe()
            //     .Subscribe(_ => OnReelingFirstHold())
            //     .AddTo(ref disposableBuilder);
            // _inputHandler.Action0Button.IsHeld
            //     .IgnoreFirstValueWhenSubscribe()
            //     .DistinctUntilChanged()
            //     .EveryUpdateWhen(x => x)
            //     .Subscribe(_ => OnReelingHold())
            //     .AddTo(ref disposableBuilder);
            // _inputHandler.Action0Button.IsUp
            //     .IgnoreFirstValueWhenSubscribe()
            //     .Subscribe(_ => OnReelingRelease())
            //     .AddTo(ref disposableBuilder);
            _inputHandler.MouseUnitCircle
                //.Where(_ => _reeling)
                .EveryUpdateWhen(x => x != Vector2.zero)
                .Select(_ => _inputHandler.MouseUnitCircle.CurrentValue)
                .DistinctUntilChanged()
                .Subscribe(x => OnRotate(x, false))
                .AddTo(ref disposableBuilder);
            _inputHandler.RightStickDelta
                //.Where(_ => _reeling)
                .EveryUpdateWhen(x => x != Vector2.zero)
                .Select(_ => _inputHandler.RightStickDelta.CurrentValue)
                .DistinctUntilChanged()
                .Subscribe(x => OnRotate(x, true))
                .AddTo(ref disposableBuilder);
            _model.ReelingPercent
                .Subscribe(x => _hookFactory.Current.SetPositionX(x.AsInversePercentage))
                .AddTo(ref disposableBuilder);
            _model.ReelingPercent
                .Where(x => x == Percentage.Full)
                .Subscribe(_ => OnWinReeling())
                .AddTo(ref disposableBuilder);
           Observable.IntervalFrame(2)
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
            _startSlowMo?.Dispose();
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
            var difference = Mathf.DeltaAngle(_previousCheckAngle, _currentAngle);
            _previousCheckAngle = _currentAngle;
            if (_currentSign is not Sign.Positive && _reeling)
            {
                _reeling = false;
                OnReelingRelease();
                return;
            }
            if (difference > (float)_config.RotationIdleThreshold)
            {
                _reeling = true;
                OnReelingFirstHold();
                return;
            }
            if (!_reeling) return;
            _reeling = false;
            OnReelingRelease();
        }
        
        private void OnRotate(Vector2 delta, bool gamepad)
        {
            _currentAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            var deltaAngle = Mathf.DeltaAngle(_previousAngle, _currentAngle);
            _currentSign = deltaAngle == 0 ? Sign.Zero : (Sign)(int)Mathf.Sign(deltaAngle);
            _previousAngle = _currentAngle;
            if (_currentSign is not Sign.Positive) return; //NOTE: Only allow counter-clockwise, remove this line to allow both directions
            var absDeltaAngle = Mathf.Abs(deltaAngle);
            var sensitivity = gamepad ? _config.GamepadSensitivity : _config.MouseSensitivity;
            /*if (_previousSign == Sign.Zero)
            {
                _previousSign = currentSign;
            }
            if (!_changeDirection)
            {
                _changeDirection = currentSign != _previousSign;
                _passedThreshold = true;
            }
            else
            {
                DebugUtils.Log("Test");
                _passedThreshold = false;
                var accumulatedAngle = _accumulatedAngle.Value;
                accumulatedAngle += absDeltaAngle * sensitivity;
                _accumulatedAngle = currentSign != _previousSign ? 0 : accumulatedAngle;
                if (_accumulatedAngle.Value >= _config.RotationThreshold)
                {
                    _passedThreshold = true;
                    _changeDirection = false;
                }
            }
            if (_passedThreshold)
            {
                OnReelingHold();
            }
            _previousAngle = angle;
            _previousSign = currentSign;*/
            var accumulatedAngle = _accumulatedAngle;
            accumulatedAngle += absDeltaAngle * sensitivity;
            _accumulatedAngle = accumulatedAngle;
            if (_accumulatedAngle >= _config.RotationThreshold)
            {
                _accumulatedAngle = 0f;
                OnReelingHold();
            }
        }

        private void OnWinReeling()
        {
            _playerAnimator.Set(PlayerAnimationKey.IdleRod, 0, true);
            _model.Inventory.ChangeCurrentBaitAmount(-1);
            OnReelingResult?.Invoke(Sign.Positive);
        }
        
        private void OnFishRegainConsciousness()
        {
            OnReelingResult?.Invoke(Sign.Negative);
        }

        #endregion

        #region Utils

        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            _currentAngle = 0f;
            _previousCheckAngle = 0f;
            _currentSign = Sign.Zero;
            _previousAngle = 0f;
            // _previousSign = Sign.Zero;
            // _changeDirection = false;
            _accumulatedAngle = 0f;
            if (active)
            {
                _model.SetFishInstance(_fishFactory.Current);
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                Bind();
                StartFatigueTimer();
            }
            else
            {
                _commander.Reset();
                _fatigueTimer?.Dispose();
                var fatigueSlider = _fishSpriteFactory.Current.FatigueTimerView;
                fatigueSlider.TransitionOut().Forget();
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }
        
        public void Reset()
        {
            _model.Reset();
            _commander.Reset();
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