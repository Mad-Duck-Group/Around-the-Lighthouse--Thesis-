using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Scripts.Fishing.UI.Reeling;
using Madduck.Scripts.Fishing.UI.ThrowHook;
using Madduck.Scripts.Input;
using Madduck.Scripts.Utils.Others;
using R3;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Scripts.Fishing.Controller.Reeling
{
    public class ReelingController : IDisposable
    {
        public event Action<Sign> OnReelingResult;
        private readonly ThrowHookProjectileFactory _factory;
        private readonly PlayerInputHandler _inputHandler;
        private readonly ReelingCommander _commander;
        private readonly ReelingModel _model;
        private IDisposable _bindings;
        private CancellationTokenSource _fatigueTimerCts = new();
        
        [Inject]
        public ReelingController(
            ThrowHookProjectileFactory factory,
            PlayerInputHandler inputHandler, 
            ReelingCommander commander,
            ReelingModel model)
        {
            _factory = factory;
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
            var fatigueDuration = _model.FishInstance.FishBehaviorData.FatigueDuration;
            _fatigueTimerCts = new CancellationTokenSource();
            await UniTask.WaitForSeconds(fatigueDuration, cancellationToken: _fatigueTimerCts.Token);
            _model.FishInstance.CurrentFatigueCount++;
            var maxFatigueAttempt = _model.FishInstance.FishBehaviorData.MaxFatigueAttempts;
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
            await _factory.CurrentHook.Return();
            _factory.DestroyHook();
            OnReelingResult?.Invoke(Sign.Positive);
        }
        
        private async UniTaskVoid OnLoseReeling()
        {
            SetActive(false);
            await _factory.CurrentHook.Return();
            _factory.DestroyHook();
            OnReelingResult?.Invoke(Sign.Negative);
        }
        
        private void OnFishRegainConsciousness()
        {
            OnReelingResult?.Invoke(Sign.Zero);
        }
    }
}