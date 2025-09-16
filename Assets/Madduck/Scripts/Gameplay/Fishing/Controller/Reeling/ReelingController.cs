using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.Controller
{
    public class ReelingController : IDisposable
    {
        public event Action<Sign> OnReelingResult;
        private readonly HookProjectileFactory _hookFactory;
        private readonly IFishFactory _fishFactory;
        private readonly PlayerInputHandler _inputHandler;
        private readonly ReelingCommander _commander;
        private readonly ReelingModel _model;
        private IDisposable _bindings;
        private CancellationTokenSource _fatigueTimerCts = new();
        
        [Inject]
        public ReelingController(
            HookProjectileFactory hookFactory,
            IFishFactory fishFactory,
            PlayerInputHandler inputHandler, 
            ReelingCommander commander,
            ReelingModel model)
        {
            _hookFactory = hookFactory;
            _fishFactory = fishFactory;
            _inputHandler = inputHandler;
            _commander = commander;
            _model = model;
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
                .SubscribeAwait((_, _) =>OnWinReeling(), AwaitOperation.Drop)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
        }
        
        public void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                _model.SetFishInstance(_fishFactory.CurrentFish);
                Bind();
                StartFatigueTimer().Forget();
            }
            else
            {
                _fatigueTimerCts.Cancel();
            }
            _model.IsActive.Value = active;
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
                OnLoseReeling().Forget();
                return;
            }
            OnFishRegainConsciousness();
        }
        
        private void OnReelingHold()
        {
            _commander.OnReelingHold.Execute(InputType.NonUI);
        }

        private async UniTask OnWinReeling()
        {
            SetActive(false);
            await _hookFactory.CurrentHook.Return();
            _hookFactory.DestroyHook();
            OnReelingResult?.Invoke(Sign.Positive);
        }
        
        private async UniTaskVoid OnLoseReeling()
        {
            SetActive(false);
            await _hookFactory.CurrentHook.Return();
            _hookFactory.DestroyHook();
            OnReelingResult?.Invoke(Sign.Negative);
        }
        
        private void OnFishRegainConsciousness()
        {
            OnReelingResult?.Invoke(Sign.Zero);
        }
    }
}