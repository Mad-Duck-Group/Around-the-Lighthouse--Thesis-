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

        private readonly NibbleModel _model;
        private readonly NibbleCommander _commander;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly IGenericFactory<FishItemInstance> _fishFactory;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly ITransitionable _viewTransition;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        
        private IDisposable _bindings;
        private CancellationTokenSource _waitingCts = new();
        private CancellationTokenSource _transitionCts = new();

        #endregion

        #region Injection

        [Inject]
        public NibbleController(
            NibbleModel model, 
            NibbleCommander commander,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            IGenericFactory<FishItemInstance> fishFactory,
            IFishSpriteFactory fishSpriteFactory,
            ITransitionable viewTransition,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _inputHandler = inputHandler;
            _model = model;
            _commander = commander;
            _hookFactory = hookFactory;
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
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => OnPullHook())
                .AddTo(ref disposableBuilder);
            _model.PullHookResult
                .Where(x => x is not Sign.Zero)
                .SubscribeAwait((result, _) => OnPullHookResultChanged(result), AwaitOperation.Drop)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _waitingCts.Cancel();
            _waitingCts.Dispose();
            _bindings?.Dispose();
        }

        #endregion

        #region Event Handlers

        private void OnPullHook()
        {
            _commander.PullHookCommand.Execute(Unit.Default);
        }
        
        private async UniTask OnPullHookResultChanged(Sign result)
        {
            _waitingCts.Cancel();
            _hookFactory.Current.StopNibble();
            if (result is Sign.Positive)
            {
                _fishSpriteFactory.Create();
                _fishSpriteFactory.Current.SetUp(_hookFactory.CurrentGameObject.transform, _fishFactory.Current);
                await UniTask.WhenAll(
                    _fishSpriteFactory.Current.TransitionIn(),
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
            _playerAnimator.Set(PlayerAnimationKey.PullHookUp, 0, false);
            await _hookFactory.Current.Return();
            _hookFactory.DestroyHook();
        }
        
        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _waitingCts.Cancel();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _model.SetFishInstance(_fishFactory.Current);
                Bind();
                StartWaiting().Forget();
            }
            else
            {
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }

        private async UniTaskVoid StartWaiting()
        {
            var maxAttempt = _model.FishItemInstance.ItemData.MaxNibbleAttempts;
            for (var i = 0; i < maxAttempt; i++)
            {
                _waitingCts = new CancellationTokenSource();
                await StartNibbleTimer(_waitingCts.Token);
            }
            DebugUtils.Log("Fish got away because no nibble detected in time");
            OnPullHookResultChanged(Sign.Negative).Forget();
        }

        private async UniTask StartNibbleTimer(CancellationToken cancellationToken)
        {
            var waitRange = _model.FishItemInstance.ItemData.NibbleIntervalRange;
            var waitTime = Random.Range(waitRange.x, waitRange.y);
            await UniTask.WaitForSeconds(waitTime, cancellationToken: cancellationToken);
            _model.IsNibbling.Value = true;
            _hookFactory.Current.Nibble(-1).Forget();
            var nibbleTimeframeRange = _model.FishItemInstance.ItemData.NibbleTimeFrameRange;
            var nibbleTimeframe = Random.Range(nibbleTimeframeRange.x, nibbleTimeframeRange.y);
            await UniTask.WaitForSeconds(nibbleTimeframe, cancellationToken: cancellationToken);
            _model.IsNibbling.Value = false;
            _hookFactory.Current.StopNibble();
        }

        #endregion
    }
}