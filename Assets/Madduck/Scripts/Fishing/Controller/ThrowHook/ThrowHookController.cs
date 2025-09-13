using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Madduck.Scripts.Fishing.Config.ThrowHook;
using Madduck.Scripts.Fishing.UI.ThrowHook;
using Madduck.Scripts.Input;
using Madduck.Scripts.Utils.Others;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.Fishing.Controller.ThrowHook
{
    public class ThrowHookController : IDisposable
    {
        public event Action OnHookHitWater;
        private readonly PlayerInputHandler _inputHandler;
        private readonly ThrowHookCommander _commander;
        private readonly ThrowHookModel _model;
        private readonly ThrowHookConfig _config;
        private readonly ThrowHookProjectileFactory _factory;
        private IDisposable _bindings;
        
        [Inject]
        public ThrowHookController(
            PlayerInputHandler inputHandler,
            ThrowHookCommander commander,
            ThrowHookModel model,
            ThrowHookConfig config,
            ThrowHookProjectileFactory factory)
        {
            _inputHandler = inputHandler;
            _commander = commander;
            _model = model;
            _config = config;
            _factory = factory;
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
        
        public void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
            }
            _model.IsActive.Value = active;
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
            var projectile = _factory.Create();
            var throwPercent = _model.ThrowHookPercent.CurrentValue;
            var distance = Mathf.Lerp(
                _config.ThrowRange.x,
                _config.ThrowRange.y, 
                throwPercent);
            await projectile.Throw(distance);
            OnHookHitWater?.Invoke();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
        }
    }
}