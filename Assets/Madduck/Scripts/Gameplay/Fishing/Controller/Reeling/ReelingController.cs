using System;
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
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
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
        private float _previousAngle;
        private Sign _previousSign = Sign.Zero;
        private bool _changeDirection;
        private bool _passedThreshold;
        private UFloat _accumulatedAngle;
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
            _inputHandler.MouseDelta
                .Subscribe(OnRotate)
                .AddTo(ref disposableBuilder);
            _inputHandler.RightStickDelta
                .Subscribe(OnRotate)
                .AddTo(ref disposableBuilder);
            _model.ReelingPercent
                .Subscribe(x => _hookFactory.Current.SetPositionX(x.AsInversePercentage))
                .AddTo(ref disposableBuilder);
            _model.ReelingPercent
                .Where(x => x == Percentage.Full)
                .Subscribe(_ => OnWinReeling())
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
        
        private void OnRotate(Vector2 delta)
        {
            if (delta == Vector2.zero)
            {
                _commander.OnReelingRelease.Execute(InputType.NonUI);
                return;
            }
            _commander.OnReelingFirstHold.Execute(InputType.NonUI);
            var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            var deltaAngle = Mathf.DeltaAngle(_previousAngle, angle);
            var currentSign = deltaAngle == 0 ? Sign.Zero : (Sign)(int)Mathf.Sign(deltaAngle);
            if (currentSign is Sign.Negative) return; //NOTE: Only allow counter-clockwise, remove this line to allow both directions
            var absDeltaAngle = Mathf.Abs(deltaAngle);
            if (_previousSign == Sign.Zero)
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
                _passedThreshold = false;
                _accumulatedAngle += absDeltaAngle;
                _accumulatedAngle = currentSign != _previousSign ? 0 : _accumulatedAngle;
                if (_accumulatedAngle >= _config.ChangeDirectionThreshold)
                {
                    _passedThreshold = true;
                    _accumulatedAngle = 0f;
                    _changeDirection = false;
                }
            }
            if (_passedThreshold)
            {
                _commander.OnReelingHold.Execute(InputType.NonUI);
            }
            _previousAngle = angle;
            _previousSign = currentSign;
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
            _previousAngle = 0f;
            _previousSign = Sign.Zero;
            _changeDirection = false;
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