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
    public class TugOfWarController : IDisposable
    {
        public event Action<Sign> OnTugOfWarResult;
        
        private readonly TugOfWarConfig _config;
        private readonly TugOfWarModel _model;
        private readonly ReelingModel _reelingModel;
        private readonly FishingSharedVariable _sharedVariable;
        private readonly IAudioManager _audioManager;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        private readonly ITransitionable _viewTransition;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        
        private IDisposable _bindings;
        private IDisposable _decayingTimer;
        private CancellationTokenSource _transitionCts = new();
        private bool _thresholdReached;
        private bool _inputActive;
        private const string ThrowEventName = "After_Throw";
        
        [Inject]
        public TugOfWarController(
            TugOfWarConfig config,
            TugOfWarModel model,
            ReelingModel reelingModel,
            FishingSharedVariable sharedVariable,
            IAudioManager audioManager,
            IFishSpriteFactory fishSpriteFactory,
            ISpineAnimator<PlayerAnimationKey> playerAnimator,
            [Key(FishingStateType.TugOfWar)] ITransitionable viewTransition,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory)
        {
            _config = config;
            _model = model;
            _audioManager = audioManager;
            _sharedVariable = sharedVariable;
            _reelingModel = reelingModel;
            _fishSpriteFactory = fishSpriteFactory;
            _playerAnimator = playerAnimator;
            _viewTransition = viewTransition;
            _inputHandler = inputHandler;
            _hookFactory = hookFactory;
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.JerkBaitButton.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x && _inputActive)
                .Subscribe(_ => OnTugButtonDown())
                .AddTo(ref disposableBuilder);
            _inputHandler.JerkBaitButton.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Subscribe(isDown => _model.IsTugButtonDown.Value = isDown)
                .AddTo(ref disposableBuilder);
            _model.TugOfWarPercent
                .Where(x => x >= _model.FishingRodInstance.CurrentStats.CurrentTugOfWarDecayThreshold)
                .Subscribe(_ => _thresholdReached = true)
                .AddTo(ref disposableBuilder);
            _model.TugOfWarPercent
                .Where(x => x == Percentage.Full)
                .Subscribe(_ => OnWinTugOfWar())
                .AddTo(ref disposableBuilder);
            _model.TugOfWarPercent
                .Where(x => x == Percentage.Zero && _thresholdReached)
                .Subscribe(_ => OnLoseTugOfWar().Forget())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnTugButtonDown()
        {
            _model.CurrentTugOfWarProgress.Value += _model.FishingRodInstance.CurrentStats.CurrentTugOfWarGainRate;
        }
        
        
        private async UniTaskVoid OnLoseTugOfWar()
        {
            _inputActive = false;
            _decayingTimer?.Dispose();
            _thresholdReached = false;
            var regression = (float)_model.FishInstance.CurrentStats.CurrentTugOfWarRegression;
            var reelingProgress = (float)_reelingModel.CurrentReelingProgress.Value;
            var newProgress = reelingProgress - regression;
            _reelingModel.CurrentReelingProgress.Value = newProgress;
            if (newProgress < 0) _reelingModel.Inventory.ChangeCurrentBaitAmount(-1);
            var newPercent = _reelingModel.ReelingPercent.CurrentValue;
            await _hookFactory.Current.MoveX(newPercent.AsInversePercentage);
            OnTugOfWarResult?.Invoke(newProgress < 0 ? Sign.Negative : Sign.Zero);
        }

        private void OnWinTugOfWar()
        {
            _inputActive = false;
            _decayingTimer?.Dispose();
            _thresholdReached = false;
            OnTugOfWarResult?.Invoke(Sign.Positive);
        }

        private void StartDecaying()
        {
            _playerAnimator.Set(PlayerAnimationKey.Pulling, 0, true);
            _fishSpriteFactory.Current.Animator.Set(FishSpriteAnimationKey.Pulling, 0, true);
            _decayingTimer = Observable.EveryUpdate()
                .Where(_ => _thresholdReached)
                .Subscribe(_ =>
                {
                    _model.CurrentTugOfWarProgress.Value -= _model.FishInstance.CurrentStats.CurrentTugOfWarDecayRate * Time.deltaTime;
                });
        }
        
        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                _model.SetFishInstance(_sharedVariable.CurrentFishable as FishItemInstance);
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _inputActive = true;
                Bind();
                StartDecaying();
            }
            else
            {
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
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
        
        public void Reset()
        {
            _thresholdReached = false;
            _inputActive = false;
            _model.Reset();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
        }
    }
}