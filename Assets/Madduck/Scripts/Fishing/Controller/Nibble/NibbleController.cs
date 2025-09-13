using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Scripts.Fishing.UI.Nibble;
using Madduck.Scripts.Fishing.UI.ThrowHook;
using Madduck.Scripts.Input;
using Madduck.Scripts.Utils.Others;
using R3;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Scripts.Fishing.Controller.Nibble
{
    public class NibbleController : IDisposable
    {
        public event Action<Sign> OnPullHookResult;
        private readonly PlayerInputHandler _inputHandler;
        private readonly NibbleModel _model;
        private readonly NibbleCommander _commander;
        private readonly ThrowHookProjectileFactory _factory;
        private IDisposable _bindings;
        private CancellationTokenSource _waitingCts = new();
        
        [Inject]
        public NibbleController(
            PlayerInputHandler inputHandler,
            NibbleModel model, 
            NibbleCommander commander,
            ThrowHookProjectileFactory factory)
        {
            _inputHandler = inputHandler;
            _model = model;
            _commander = commander;
            _factory = factory;
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
        
        public void SetActive(bool active)
        {
            _bindings?.Dispose();
            _waitingCts.Cancel();
            if (active)
            {
                Bind();
                StartWaiting().Forget();
            }
            _model.IsActive.Value = active;
        }

        private async UniTaskVoid StartWaiting()
        {
            var maxAttempt = _model.FishItemInstance.FishBehaviorData.MaxNibbleAttempts;
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
            var waitRange = _model.FishItemInstance.FishBehaviorData.NibbleIntervalRange;
            var waitTime = UnityEngine.Random.Range(waitRange.x, waitRange.y);
            await UniTask.WaitForSeconds(waitTime, cancellationToken: cancellationToken);
            _model.IsNibbling.Value = true;
            _factory.CurrentHook.Nibble(-1).Forget();
            var nibbleTimeframeRange = _model.FishItemInstance.FishBehaviorData.NibbleTimeFrameRange;
            var nibbleTimeframe = UnityEngine.Random.Range(nibbleTimeframeRange.x, nibbleTimeframeRange.y);
            await UniTask.WaitForSeconds(nibbleTimeframe, cancellationToken: cancellationToken);
            _model.IsNibbling.Value = false;
            _factory.CurrentHook.StopNibble();
        }
        
        private void OnPullHook()
        {
            _commander.PullHookCommand.Execute(Unit.Default);
        }
        
        private async UniTask OnPullHookResultChanged(Sign result)
        {
            _waitingCts.Cancel();
            _factory.CurrentHook.StopNibble();
            if (result is Sign.Negative)
            {
                SetActive(false);
                await _factory.CurrentHook.Return();
                _factory.DestroyHook();
            }
            OnPullHookResult?.Invoke(result);
        }
    }
}