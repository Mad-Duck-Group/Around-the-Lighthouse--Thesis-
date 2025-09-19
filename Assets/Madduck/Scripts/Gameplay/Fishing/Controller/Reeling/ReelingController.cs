using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Utils;
using MessagePipe;
using R3;
using VContainer;

namespace Madduck.Fishing.Controller
{
    public class ReelingController : IDisposable
    {
        public event Action<Sign> OnReelingResult;
        
        private readonly HookProjectileFactory _hookFactory;
        private readonly ReelingCommander _commander;
        private readonly ReelingModel _model;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IFishFactory _fishFactory;
        private readonly ITransitionable _viewTransition;
        
        private IDisposable _bindings;
        private CancellationTokenSource _fatigueTimerCts = new();
        private CancellationTokenSource _transitionCts = new();
        
        [Inject]
        public ReelingController(
            HookProjectileFactory hookFactory,
            ReelingCommander commander,
            ReelingModel model,
            IPlayerInputHandler inputHandler,
            IFishFactory fishFactory,
            ITransitionable viewTransition)
        {
            _hookFactory = hookFactory;
            _inputHandler = inputHandler;
            _commander = commander;
            _model = model;
            _fishFactory = fishFactory;
            _viewTransition = viewTransition;
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.ThrowHookButton.IsHeld
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .EveryUpdateWhen(x => x)
                .Subscribe(_ => OnReelingHold())
                .AddTo(ref disposableBuilder);
            _model.CurrentReelingProgress
                .Where(progress => progress >= _model.MaxReelingProgress.Value)
                .Subscribe(_ => OnWinReeling())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
        }
        
        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _model.SetFishInstance(_fishFactory.CurrentFish);
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
        
        public async UniTask ReturnHook()
        {
            await _hookFactory.CurrentHook.Return();
            _hookFactory.DestroyHook();
        }
        
        private void OnReelingHold()
        {
            _commander.OnReelingHold.Execute(InputType.NonUI);
        }

        private void OnWinReeling()
        {
            OnReelingResult?.Invoke(Sign.Positive);
        }
        
        private void OnLoseReeling()
        {
            OnReelingResult?.Invoke(Sign.Negative);
        }
        
        private void OnFishRegainConsciousness()
        {
            OnReelingResult?.Invoke(Sign.Zero);
        }
    }
}