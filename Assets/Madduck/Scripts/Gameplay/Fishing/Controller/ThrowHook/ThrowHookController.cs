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
        #region Events

        public event Action OnHookThrown;

        #endregion

        #region Fields

        private readonly ThrowHookCommander _commander;
        private readonly ThrowHookModel _model;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly ITransitionable _viewTransition;
        
        private IDisposable _bindings;
        private CancellationTokenSource _transitionCts = new();

        #endregion

        #region Injection

        [Inject]
        public ThrowHookController(
            ThrowHookCommander commander,
            ThrowHookModel model,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            [Key(FishingStateType.ThrowHook)] ITransitionable viewTransition)
        {
            _inputHandler = inputHandler;
            _commander = commander;
            _model = model;
            _hookFactory = hookFactory;
            _viewTransition = viewTransition;
        }

        #endregion

        #region Bindings

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.Action0Button.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => OnHookFirstHeld())
                .AddTo(ref disposableBuilder);
            _inputHandler.Action0Button.IsHeld
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .EveryUpdateWhen(x => x && !_model.HookThrown.Value)
                .Subscribe(_ => OnHookHeld())
                .AddTo(ref disposableBuilder);
            _inputHandler.Action0Button.IsUpAfterHeld
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => OnHookRelease())
                .AddTo(ref disposableBuilder);
            _model.HookThrown
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => OnThrownHook())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
        }

        #endregion

        #region Event Handlers

        private void OnHookFirstHeld()
        {
            _commander.ThrowHookFirstHeldCommand.Execute(InputType.NonUI);
        }

        private void OnHookHeld()
        {
            _commander.ThrowHookHeldCommand.Execute(InputType.NonUI);
        }
        
        private void OnHookRelease()
        {
            _commander.ThrowHookReleaseCommand.Execute(InputType.NonUI);
        }

        private void OnThrownHook()
        {
            OnHookThrown?.Invoke();
        }

        #endregion

        #region Utils

        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                Bind();
            }
            else
            {
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }
        
        public async UniTask ThrowHook()
        {
            var projectile = _hookFactory.Create();
            var throwPercent = _model.ThrowHookPercent.CurrentValue;
            await projectile.Throw(throwPercent);
        }
        
        public void Reset()
        {
            _model.Reset();
            _commander.Reset();
        }

        #endregion
    }
}