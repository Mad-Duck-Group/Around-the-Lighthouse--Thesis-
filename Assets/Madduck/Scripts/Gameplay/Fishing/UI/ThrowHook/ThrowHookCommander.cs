using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class ThrowHookCommander : IDisposable
    {
        public ReactiveCommand<InputType> ThrowHookFirstHeldCommand { get; } = new();
        public ReactiveCommand<InputType> ThrowHookHeldCommand { get; } = new();
        public ReactiveCommand<InputType> ThrowHookReleaseCommand { get; } = new();
        private readonly ThrowHookModel _model;
        private readonly ThrowHookConfig _config;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        private readonly IIdleAnimator _playerIdleAnimator;
        
        private bool _hookThrown;
        private CancellationTokenSource _chargeCts = new();
        private InputType? _activeInputType;
        private Sign _throwHookSliderDirection = Sign.Positive;
        private IDisposable _bindings;
        
        [Inject]
        public ThrowHookCommander(
            ThrowHookModel model, 
            ThrowHookConfig config, 
            ISpineAnimator<PlayerAnimationKey> playerAnimator,
            IIdleAnimator playerIdleAnimator)
        {
            _model = model;
            _config = config;
            _playerAnimator = playerAnimator;
            _playerIdleAnimator = playerIdleAnimator;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            ThrowHookHeldCommand
                .ResolveInputType()
                .Subscribe(x => _activeInputType = x)
                .AddTo(ref disposableBuilder);
            ThrowHookFirstHeldCommand
                .Where(_ => _activeInputType is null && !_hookThrown)
                .Subscribe(x =>
                {
                    _activeInputType = x;
                    OnThrowHookFirstHeld(_chargeCts.Token).Forget();
                })
                .AddTo(ref disposableBuilder);
            ThrowHookHeldCommand
                .Where(x=> x == _activeInputType && !_hookThrown)
                .Subscribe(_ => OnThrowHookHeld())
                .AddTo(ref disposableBuilder);
            ThrowHookReleaseCommand
                .Where(x => x == _activeInputType && !_hookThrown)
                .Subscribe(_ =>
                {
                    OnThrowHookReleased().Forget();
                    _activeInputType = null;
                })
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private async UniTaskVoid OnThrowHookFirstHeld(CancellationToken token)
        {
            _playerIdleAnimator.StopIdle();
            await _playerAnimator.Set(PlayerAnimationKey.PrepareThrow, 0, false).WaitUntilComplete(cancellationToken: token);
            _playerAnimator.Set(PlayerAnimationKey.ChargingThrow, 0, true);
        }
        
        private void OnThrowHookHeld()
        {
            var currentValue = (float)_model.ThrowHookCurrentValue.Value;   
            var maxValue = (float)_model.ThrowHookMaxValue.Value;
            if (currentValue >= maxValue && _throwHookSliderDirection is Sign.Positive)
            {
                _throwHookSliderDirection = Sign.Negative;
            }
            else if (currentValue <= 0 && _throwHookSliderDirection is Sign.Negative)
            {
                _throwHookSliderDirection = Sign.Positive;
            }
            _model.ThrowHookCurrentValue.Value = currentValue + (int)_throwHookSliderDirection 
                * ((float)_config.ThrowHookSliderSpeed * Time.deltaTime);
        }
        
        private async UniTaskVoid OnThrowHookReleased()
        {
            _chargeCts.Cancel();
            _hookThrown = true;
            await _playerAnimator.Set(PlayerAnimationKey.ReleaseThrow, 0, false).WaitUntilComplete();
            _playerAnimator.Set(PlayerAnimationKey.IdleRod, 0, true);
            _model.HookThrown.Value = true;
        }

        public void Reset()
        {
            _chargeCts = new();
            _hookThrown = false;
            _activeInputType = null;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}