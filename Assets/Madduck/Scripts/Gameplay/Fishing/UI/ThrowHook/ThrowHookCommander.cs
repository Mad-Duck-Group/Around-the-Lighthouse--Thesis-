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
        public ReactiveCommand<InputType> ThrowHookHeldCommand { get; private set; }
        public ReactiveCommand<InputType> ThrowHookReleaseCommand { get; private set; }
        private readonly ThrowHookModel _model;
        private readonly ThrowHookConfig _config;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        private readonly ReactiveProperty<bool> _isHolding = new();
        
        private bool _hookThrown;
        private CancellationTokenSource _chargeCts = new();
        private InputType _activeInputType;
        private Sign _throwHookSliderDirection = Sign.Positive;
        private IDisposable _bindings;
        
        [Inject]
        public ThrowHookCommander(
            ThrowHookModel model, 
            ThrowHookConfig config, 
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _model = model;
            _config = config;
            _playerAnimator = playerAnimator;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            ThrowHookHeldCommand = new ReactiveCommand<InputType>();
            ThrowHookHeldCommand
                .ResolveInputType()
                .Subscribe(x => _activeInputType = x)
                .AddTo(ref disposableBuilder);
            _isHolding
                .DistinctUntilChanged()
                .Where(x => x && !_hookThrown)
                .Subscribe(_ => OnThrowHookFirstHeld(_chargeCts.Token).Forget())
                .AddTo(ref disposableBuilder);
            ThrowHookHeldCommand
                .Where(x=> x == _activeInputType && !_hookThrown)
                .Subscribe(_ => OnThrowHookHeld())
                .AddTo(ref disposableBuilder);
            ThrowHookReleaseCommand = new ReactiveCommand<InputType>();
            ThrowHookReleaseCommand
                .Where(x => x == _activeInputType && !_hookThrown)
                .Subscribe(_ => OnThrowHookReleased().Forget())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private async UniTaskVoid OnThrowHookFirstHeld(CancellationToken token)
        {
            await _playerAnimator.Set(PlayerAnimationKey.PrepareThrow, 0, false).WaitUntilComplete(cancellationToken: token);
            _playerAnimator.Set(PlayerAnimationKey.ChargingThrow, 0, true);
        }
        
        private void OnThrowHookHeld()
        {
            _isHolding.Value = true;
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
            _isHolding.Value = false;
            await _playerAnimator.Set(PlayerAnimationKey.ReleaseThrow, 0, false).WaitUntilComplete();
            _playerAnimator.Set(PlayerAnimationKey.Idle1, 0, true);
            _model.HookThrown.Value = true;
        }

        public void Reset()
        {
            _chargeCts = new();
            _hookThrown = false;
            _isHolding.Value = false;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}