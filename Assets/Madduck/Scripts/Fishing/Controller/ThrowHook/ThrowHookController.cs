using System;
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
            Observable.EveryUpdate()
                .Where(_ => _inputHandler.ThrowHookButton.Value.isHeld)
                .Subscribe(_ => OnHookHeld())
                .AddTo(ref disposableBuilder);
            _inputHandler.ThrowHookButton
                .Subscribe(OnHookRelease)
                .AddTo(ref disposableBuilder);
            _model.HookThrown.Subscribe(OnHookThrown)
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
            _commander.ThrowHookHeldCommand.Execute(Unit.Default);
        }
        
        private void OnHookRelease(PlayerInputHandler.InputButton button)
        {
            if (!button.isUpAfterHeld) return;
            _commander.ThrowHookReleaseCommand.Execute(Unit.Default);
        }

        private void OnHookThrown(bool thrown)
        {
            if (!thrown) return;
            var projectile = _factory.Create();
            var throwPercent = _model.ThrowHookPercent.CurrentValue;
            var distance = Mathf.Lerp(
                _config.ThrowRange.x,
                _config.ThrowRange.y, 
                throwPercent);
            projectile.Throw(distance)
                .ContinueWith(() => OnHookHitWater?.Invoke());
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}