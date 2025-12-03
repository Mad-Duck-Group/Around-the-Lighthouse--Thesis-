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
        private bool _isDown;
        private bool _isHolding;
        private CancellationTokenSource _chargeCts = new();
        private InputType? _activeInputType;
        private Sign _throwHookSliderDirection = Sign.Positive;
        private IDisposable _bindings;
        private AudioReference _throwChargingAudioRef;
        private const string ThrowHookProgressParameter = "ThrowHookProgress";
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
                .Where(_ => !_hookThrown && !_isHolding)
                .Subscribe(x =>
                {
                    _activeInputType = x;
                    _isDown = true;
                    OnThrowHookDown(_chargeCts.Token).Forget();
                })
                .AddTo(ref disposableBuilder);
            ThrowHookHeldCommand
                .Where(x=> x == _activeInputType && !_hookThrown && _isDown)
                .Subscribe(_ =>
                {
                    if (!_isHolding)
                    {
                        OnThrowHookFirstHeld();
                    }
                    _isHolding = true;
                    OnThrowHookHeld();
                })
                .AddTo(ref disposableBuilder);
            ThrowHookReleaseCommand
                .Where(x => x == _activeInputType && !_hookThrown && _isDown)
                .Subscribe(_ =>
                {
                    _isDown = false;
                    if (!_isHolding)
                    {
                        CancelCharge(); // premature release
                        return;
                    }
                    _isHolding = false;
                    OnThrowHookReleased().Forget();
                    _activeInputType = null;
                })
                .AddTo(ref disposableBuilder);
            _model.ThrowHookPercent
                .Subscribe(x =>
                {
                    if (_throwChargingAudioRef == null) return;
                    _throwChargingAudioRef.eventInstance.setParameterByName(ThrowHookProgressParameter, x.AsFraction);
                })
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private async UniTaskVoid OnThrowHookDown(CancellationToken token)
        {
            _playerIdleAnimator.StopIdle();
            await _playerAnimator.Set(PlayerAnimationKey.PrepareThrow, 0, false).WaitUntilComplete(cancellationToken: token);
            _playerAnimator.Set(PlayerAnimationKey.ChargingThrow, 0, true);
        }

        private void OnThrowHookFirstHeld()
        {
            _model.HookThrownFirstHeld.Value = true;
            _throwChargingAudioRef = _audioManager.PlayAudio(_config.ChargingSfx, Vector3.zero);
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
            var finalValue = currentValue + (int)_throwHookSliderDirection 
                * ((float)_model.FishingRod.CurrentStats.CurrentThrowSliderSpeed * Time.deltaTime);
            _model.ThrowHookCurrentValue.Value = finalValue;
        }
        
        private async UniTaskVoid OnThrowHookReleased()
        {
            _audioManager.StopAudio(_throwChargingAudioRef);
            _chargeCts.Cancel();
            _hookThrown = true;
            var track = _playerAnimator.Set(PlayerAnimationKey.ReleaseThrow, 0, false);
            await track.WaitUntilEvent(ThrowEventName);
            _audioManager.PlayAudioOneShot(_config.ThrowHookSfx, Vector3.zero);
            _model.HookThrown.Value = true;
            await track.WaitUntilComplete();
            _playerAnimator.Set(PlayerAnimationKey.IdleRod, 0, true);
        }

        private void CancelCharge()
        {
            _audioManager.StopAudio(_throwChargingAudioRef);
            _model.HookThrownFirstHeld.OnNext(false);
            _isHolding = false;
            _chargeCts.Cancel();
            _playerIdleAnimator.StartIdle();
        }

        public void Reset()
        {
            _audioManager.StopAudio(_throwChargingAudioRef);
            _chargeCts = new();
            _hookThrown = false;
            _isDown = false;
            _isHolding = false;
            _activeInputType = null;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}