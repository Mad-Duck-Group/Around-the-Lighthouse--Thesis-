using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        private float _fatigueTimerProgress;
        private CancellationTokenSource _transitionCts = new();
        private const string ThrowEventName = "After_Throw";

        #endregion

        #region Injection

        [Inject]
        public ReelingController(
            ReelingCommander commander,
            ReelingModel model,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            IGenericFactory<FishItemInstance> fishFactory,
            IFishSpriteFactory fishSpriteFactory,
            [Key(FishingStateType.Reeling)] ITransitionable viewTransition,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
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
            _inputHandler.ThrowHookButton.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(_ => OnReelingFirstHold())
                .AddTo(ref disposableBuilder);
            _inputHandler.ThrowHookButton.IsHeld
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .EveryUpdateWhen(x => x)
                .Subscribe(_ => OnReelingHold())
                .AddTo(ref disposableBuilder);
            _inputHandler.ThrowHookButton.IsUp
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(_ => OnReelingRelease())
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

        private void OnWinReeling()
        {
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
                    var percent = Percentage.Clamp01(Percentage.FromFraction(_fatigueTimerProgress / fatigueDuration));
                    fatigueSlider.SetFishFatigueTimerProgress(percent);
                    if (percent != Percentage.Full) return;
                    _fatigueTimer.Dispose();
                    fatigueSlider.TransitionOut();
                    OnFishRegainConsciousness();
                });
        }
        
        public async UniTask ReturnHook()
        {
            _fishSpriteFactory.Current.Detach();
            await _playerAnimator.Set(PlayerAnimationKey.GotFish, 0, false).WaitUntilEvent(ThrowEventName); 
            await UniTask.WhenAll(
                _hookFactory.Current.Return(),
                _fishSpriteFactory.Current.TransitionOut());
            _fishSpriteFactory.DestroyFishSprite();
            _hookFactory.DestroyHook();
        }
        
        #endregion
        
    }
}