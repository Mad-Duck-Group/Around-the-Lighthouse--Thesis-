using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
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
        private readonly ThrowHookConfig _config;
        private readonly ThrowHookModel _model;
        private readonly IAudioManager _audioManager;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        private readonly IIdleAnimator _playerIdleAnimator;
        
        private bool _hookThrown;
        private bool _isHolding;
        private CancellationTokenSource _chargeCts = new();
        private InputType? _activeInputType;
        private Sign _throwHookSliderDirection = Sign.Positive;
        private IDisposable _bindings;
        private const string ThrowEventName = "After_Throw";
        
        [Inject]
        public ThrowHookCommander(
            ThrowHookConfig config,
            ThrowHookModel model, 
            IAudioManager audioManager,
            ISpineAnimator<PlayerAnimationKey> playerAnimator,
            IIdleAnimator playerIdleAnimator)
        {
            _config = config;
            _model = model;
            _audioManager = audioManager;
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
                .Where(_ => _activeInputType is null && !_hookThrown && !_isHolding)
                .Subscribe(x =>
                {
                    _activeInputType = x;
                    _isHolding = true;
                    OnThrowHookFirstHeld(_chargeCts.Token).Forget();
                })
                .AddTo(ref disposableBuilder);
            ThrowHookHeldCommand
                .Where(x=> x == _activeInputType && !_hookThrown && _isHolding)
                .Subscribe(_ => OnThrowHookHeld())
                .AddTo(ref disposableBuilder);
            ThrowHookReleaseCommand
                .Where(x => x == _activeInputType && !_hookThrown && _isHolding)
                .Subscribe(_ =>
                {
                    _isHolding = false;
                    OnThrowHookReleased().Forget();
                    _activeInputType = null;
                })
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private async UniTaskVoid OnThrowHookFirstHeld(CancellationToken token)
        {
            _model.HookThrownFirstHeld.Value = true;
            _playerIdleAnimator.StopIdle();
            await _playerAnimator.Set(PlayerAnimationKey.PrepareThrow, 0, false).WaitUntilComplete(cancellationToken: token);
            _playerAnimator.Set(PlayerAnimationKey.ChargingThrow, 0, true);
        }
        
        private void OnThrowHookHeld()
        {
            var currentValue = (float)_model.ThrowHookCurrentValue.Value;   
            var currentMaxValue = (float)_model.ThrowHookCurrentMaxValue.Value;
            if (currentValue >= currentMaxValue && _throwHookSliderDirection is Sign.Positive)
            {
                _throwHookSliderDirection = Sign.Negative;
            }
            else if (currentValue <= 0 && _throwHookSliderDirection is Sign.Negative)
            {
                _throwHookSliderDirection = Sign.Positive;
            }
            _model.ThrowHookCurrentValue.Value = currentValue + (int)_throwHookSliderDirection 
                * ((float)_model.FishingRod.CurrentStats.CurrentThrowSliderSpeed * Time.deltaTime);
        }
        
        private async UniTaskVoid OnThrowHookReleased()
        {
            _chargeCts.Cancel();
            _hookThrown = true;
            var track = _playerAnimator.Set(PlayerAnimationKey.ReleaseThrow, 0, false);
            await track.WaitUntilEvent(ThrowEventName);
            _audioManager.PlayAudioOneShot(_config.ThrowHookSfx, Vector3.zero);
            _model.HookThrown.Value = true;
            await track.WaitUntilComplete();
            _playerAnimator.Set(PlayerAnimationKey.IdleRod, 0, true);
        }

        public void Reset()
        {
            _chargeCts = new();
            _hookThrown = false;
            _isHolding = false;
            _activeInputType = null;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}