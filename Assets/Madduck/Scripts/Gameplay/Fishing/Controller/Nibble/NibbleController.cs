using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using VContainer;
using Random = UnityEngine.Random;

namespace Madduck.Fishing.Controller
{
    public class NibbleController : IDisposable
    {
        #region Events

        public event Action<Sign> OnPullHookResult;

        #endregion

        #region Fields

        private readonly NibbleConfig _config;
        private readonly NibbleModel _model;
        private readonly NibbleCommander _commander;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly IGenericFactory<FishItemInstance> _fishFactory;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly IQTEButtonFactory _qteButtonFactory;
        private readonly ITransitionable _viewTransition;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        
        private IDisposable _bindings;
        private IDisposable _qteIntervalTimer;
        private CancellationTokenSource _transitionCts = new();
        private const string ThrowEventName = "After_Throw";
        private int _currentStageIndex;
        private bool _qteActive;

        private readonly Dictionary<int, Percentage> _currentStageChance = new()
        {
            { 0, Percentage.Zero },
            { 1, Percentage.Zero }
        };

        #endregion

        #region Injection

        [Inject]
        public NibbleController(
            NibbleConfig config,
            NibbleModel model, 
            NibbleCommander commander,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            IGenericFactory<FishItemInstance> fishFactory,
            IFishSpriteFactory fishSpriteFactory,
            IQTEButtonFactory qteButtonFactory,
            ITransitionable viewTransition,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _config = config;
            _inputHandler = inputHandler;
            _model = model;
            _commander = commander;
            _hookFactory = hookFactory;
            _fishFactory = fishFactory;
            _qteButtonFactory = qteButtonFactory;
            _fishSpriteFactory = fishSpriteFactory;
            _viewTransition = viewTransition;
            _playerAnimator = playerAnimator;
        }

        #endregion

        #region Bindings

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            // _inputHandler.ThrowHookButton.IsDown
            //     .IgnoreFirstValueWhenSubscribe()
            //     .DistinctUntilChanged()
            //     .Where(x => x)
            //     .Subscribe(_ => OnPullHook())
            //     .AddTo(ref disposableBuilder);
            _inputHandler.Action1Button.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x && !_qteActive)
                .Subscribe(_ => OnCancel())
                .AddTo(ref disposableBuilder);
            _model.PullHookResult
                .Where(x => x is not Sign.Zero)
                .SubscribeAwait((result, _) => OnPullHookResultChanged(result), AwaitOperation.Drop)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _qteIntervalTimer?.Dispose();
            _bindings?.Dispose();
        }

        #endregion

        #region Event Handlers

        private void OnPullHook()
        {
            _commander.PullHookCommand.Execute(Unit.Default);
        }

        private void OnCancel()
        {
            OnPullHookResultChanged(Sign.Negative).Forget();
        }
        
        private async UniTask OnPullHookResultChanged(Sign result)
        {
            _qteIntervalTimer?.Dispose();
            _hookFactory.Current.StopNibble();
            if (result is Sign.Positive)
            {
                var fishSprite = _fishSpriteFactory.Create();
                fishSprite.SetUp(_hookFactory.CurrentGameObject.transform, _fishFactory.Current);
                fishSprite.Animator.Set(FishSpriteAnimationKey.Idle, 0, true);
                await UniTask.WhenAll(
                    fishSprite.TransitionIn(),
                    _hookFactory.Current.MoveY(Percentage.Full));
                await _hookFactory.Current.MoveX(Percentage.Full);
            }
            OnPullHookResult?.Invoke(result);
        }

        #endregion

        #region Utils

        public void Reset()
        {
            _model.Reset();
        }

        public async UniTask ReturnHook()
        {
            _playerAnimator.Set(PlayerAnimationKey.Reeling, 0, true);
            await _hookFactory.Current.ReelBack();
            var track = _playerAnimator.Set(PlayerAnimationKey.PullHookUp, 0, false);
            await track.WaitUntilEvent(ThrowEventName);
            await UniTask.WhenAny(_hookFactory.Current.Return(), 
                track.WaitUntilComplete());
            _hookFactory.DestroyHook();
        }
        
        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _qteIntervalTimer?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _currentStageChance[0] = _model.FishingRod.CurrentStats.CurrentNibbleBaseSuccessChances[0];
                _currentStageChance[1] = _model.FishingRod.CurrentStats.CurrentNibbleBaseSuccessChances[1];
                _currentStageIndex = 0;
                Bind();
                StartQteTimer();
            }
            else
            {
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }

        private void StartQteTimer()
        {
            _qteIntervalTimer = Observable.Timer(TimeSpan.FromSeconds(_config.QteIntervalRange.RandomBetweenRange()))
                .Subscribe(_ => NewQte());
        }

        private void NewQte()
        {
            var qte = _qteButtonFactory.Create();
            qte.OnSuccess += OnQteSuccess;
            qte.OnFail += OnQteFail;
            qte.StartQuickTimeEvent();
            _qteActive = true;
        }

        private void OnQteSuccess()
        {
            _qteActive = false;
            _qteButtonFactory.Current.OnSuccess -= OnQteSuccess;
            _hookFactory.Current.Nibble(1);
            _currentStageChance[_currentStageIndex] += _model.FishingRod.CurrentStats.CurrentBubbleNibbleBonuses[BubbleType.None]; //TODO: Do bubble later
            var result = Percentage.TryRoll(_currentStageChance[_currentStageIndex]);
            switch (_currentStageIndex)
            {
                case 0 when result:
                    _currentStageChance[_currentStageIndex] = _model.FishingRod.CurrentStats.CurrentNibbleBaseSuccessChances[0];
                    _currentStageIndex++;
                    var fish = _fishFactory.Create();
                    _model.SetFishInstance(fish);
                    break;
                case 1 when result:
                    OnPullHookResultChanged(Sign.Positive).Forget();
                    return;
            }
            StartQteTimer();
        }

        private void OnQteFail()
        {
            _qteActive = false;
            _qteButtonFactory.Current.OnFail -= OnQteFail;
            _currentStageChance[_currentStageIndex] -= _model.FishingRod.CurrentStats.CurrentBubbleNibblePenalties[BubbleType.None]; //TODO: Do bubble later
            switch (_currentStageIndex)
            {
                case 0:
                    break;
                case 1:
                    _currentStageChance[_currentStageIndex] = _model.FishingRod.CurrentStats.CurrentNibbleBaseSuccessChances[1];
                    _currentStageIndex--;
                    break;
            }
            StartQteTimer();
        }

        #endregion
    }
}