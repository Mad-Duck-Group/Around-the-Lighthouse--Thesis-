using System;
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
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
    public class ThrowHookController : IDisposable
    {
        public event Action OnHookHitWater;
        
        private readonly ThrowHookCommander _commander;
        private readonly ThrowHookModel _model;
        private readonly ThrowHookConfig _config;
        private readonly HookProjectileFactory _hookFactory;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IGenericFactory<FishItemInstance> _fishFactory;
        private readonly ITransitionable _viewTransition;
        
        private IDisposable _bindings;
        private CancellationTokenSource _transitionCts = new();
        
        [Inject]
        public ThrowHookController(
            ThrowHookCommander commander,
            ThrowHookModel model,
            ThrowHookConfig config,
            HookProjectileFactory hookFactory,
            IPlayerInputHandler inputHandler,
            IGenericFactory<FishItemInstance> fishFactory,
            ITransitionable viewTransition)
        {
            _inputHandler = inputHandler;
            _commander = commander;
            _model = model;
            _config = config;
            _hookFactory = hookFactory;
            _fishFactory = fishFactory;
            _viewTransition = viewTransition;
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.ThrowHookButton.IsHeld
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .EveryUpdateWhen(x => x && !_model.HookThrown.Value)
                .Subscribe(_ => OnHookHeld())
                .AddTo(ref disposableBuilder);
            _inputHandler.ThrowHookButton.IsUpAfterHeld
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => OnHookRelease())
                .AddTo(ref disposableBuilder);
            _model.HookThrown
                .DistinctUntilChanged()
                .Where(x => x)
                .SubscribeAwait((_,_) => OnHookThrown(), AwaitOperation.Drop)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _fishFactory.Create();
                Bind();
            }
            else
            {
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }
        
        public void Reset()
        {
            _model.Reset();
        }

        private void OnHookHeld()
        {
            _commander.ThrowHookHeldCommand.Execute(InputType.NonUI);
        }
        
        private void OnHookRelease()
        {
            _commander.ThrowHookReleaseCommand.Execute(InputType.NonUI);
        }

        private async UniTask OnHookThrown()
        {
            var projectile = _hookFactory.Create();
            var throwPercent = _model.ThrowHookPercent.CurrentValue;
            var distance = Mathf.Lerp(
                _config.ThrowRange.x,
                _config.ThrowRange.y, 
                throwPercent.AsFraction);
            await projectile.Throw(distance);
            OnHookHitWater?.Invoke();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
        }
    }
}