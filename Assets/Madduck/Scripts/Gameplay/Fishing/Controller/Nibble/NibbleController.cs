using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.Controller
{
    public class NibbleController : IDisposable
    {
        public event Action<Sign> OnPullHookResult;
        private readonly NibbleModel _model;
        private readonly NibbleCommander _commander;
        private readonly HookProjectileFactory _hookFactory;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IFishFactory _fishFactory;
        private readonly ITransitionable _viewTransition;
        
        private IDisposable _bindings;
        private CancellationTokenSource _waitingCts = new();
        private CancellationTokenSource _transitionCts = new();
        
        [Inject]
        public NibbleController(
            NibbleModel model, 
            NibbleCommander commander,
            HookProjectileFactory hookFactory,
            IPlayerInputHandler inputHandler,
            IFishFactory fishFactory,
            ITransitionable viewTransition)
        {
            _inputHandler = inputHandler;
            _model = model;
            _commander = commander;
            _hookFactory = hookFactory;
            _fishFactory = fishFactory;
            _viewTransition = viewTransition;
        }

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
        
        public void Reset()
        {
            _model.Reset();
        }
        
        public void Dispose()
        {
            _waitingCts.Cancel();
            _waitingCts.Dispose();
            _bindings?.Dispose();
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
                _model.SetFishInstance(_fishFactory.CurrentFish);
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
            var waitTime = UnityEngine.Random.Range(waitRange.x, waitRange.y);
            await UniTask.WaitForSeconds(waitTime, cancellationToken: cancellationToken);
            _model.IsNibbling.Value = true;
            _hookFactory.CurrentHook.Nibble(-1).Forget();
            var nibbleTimeframeRange = _model.FishItemInstance.ItemData.NibbleTimeFrameRange;
            var nibbleTimeframe = UnityEngine.Random.Range(nibbleTimeframeRange.x, nibbleTimeframeRange.y);
            await UniTask.WaitForSeconds(nibbleTimeframe, cancellationToken: cancellationToken);
            _model.IsNibbling.Value = false;
            _hookFactory.CurrentHook.StopNibble();
        }
        
        private void OnPullHook()
        {
            _commander.PullHookCommand.Execute(Unit.Default);
        }
        
        private async UniTask OnPullHookResultChanged(Sign result)
        {
            _waitingCts.Cancel();
            _hookFactory.CurrentHook.StopNibble();
            if (result is Sign.Negative)
            {
                SetActive(false);
                await _hookFactory.CurrentHook.Return();
                _hookFactory.DestroyHook();
            }
            OnPullHookResult?.Invoke(result);
        }
    }
}