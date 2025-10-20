using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
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
        private ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        
        private IDisposable _bindings;
        private CancellationTokenSource _fatigueTimerCts = new();
        private CancellationTokenSource _transitionCts = new();

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
            ITransitionable viewTransition,
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
            _model.CurrentReelingProgress
                .CombineLatest(_model.MaxReelingProgress, (current, max) => max == 0f
                    ? Percentage.Zero
                    : Percentage.FromFraction(Mathf.Clamp01(current / max)))
                .Do(x => _hookFactory.Current.SetPositionX(x.AsInversePercentage))
                .Where(x => x == Percentage.Full)
                .Subscribe(_ => OnWinReeling())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
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
        
        private void OnLoseReeling()
        {
            _model.Inventory.ChangeCurrentBaitAmount(-1);
            OnReelingResult?.Invoke(Sign.Negative);
        }
        
        private void OnFishRegainConsciousness()
        {
            OnReelingResult?.Invoke(Sign.Zero);
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
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _model.SetFishInstance(_fishFactory.Current);
                Bind();
                StartFatigueTimer().Forget();
            }
            else
            {
                _fatigueTimerCts.Cancel();
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }
        
        public void Reset()
        {
            _model.Reset();
        }

        private async UniTaskVoid StartFatigueTimer()
        {
            var fatigueDuration = _model.FishInstance.ItemData.FatigueDuration;
            _fatigueTimerCts = new CancellationTokenSource();
            await UniTask.WaitForSeconds(fatigueDuration, cancellationToken: _fatigueTimerCts.Token);
            _model.FishInstance.CurrentFatigueCount++;
            var maxFatigueAttempt = _model.FishInstance.ItemData.MaxFatigueAttempts;
            if (_model.FishInstance.CurrentFatigueCount >= maxFatigueAttempt)
            {
                OnLoseReeling();
                return;
            }
            OnFishRegainConsciousness();
        }
        
        public async UniTask ReturnHook(bool reelBack)
        {
            _playerAnimator.Set(reelBack ? PlayerAnimationKey.GotFish : PlayerAnimationKey.LoseFish, 0, false); 
            _fishSpriteFactory.Current.Detach();
            await UniTask.WhenAll(
                _hookFactory.Current.Return(),
                _fishSpriteFactory.Current.TransitionOut());
            _fishSpriteFactory.DestroyFishSprite();
            _hookFactory.DestroyHook();
        }
        

        #endregion
        
    }
}