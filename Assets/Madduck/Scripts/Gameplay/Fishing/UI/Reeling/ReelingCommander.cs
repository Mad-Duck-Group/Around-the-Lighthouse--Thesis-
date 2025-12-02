using System;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class ReelingCommander : IDisposable
    {
        public ReactiveCommand<InputType> OnReelingFirstHold { get; } = new();
        public ReactiveCommand<InputType> OnReelingHold { get; } = new();
        public ReactiveCommand<InputType> OnReelingRelease { get; } = new();
        
        private readonly ReelingModel _model;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        private bool _isHolding;
        private InputType? _activeInputType;
        private IDisposable _bindings;
        
        [Inject]
        public ReelingCommander(
            ReelingModel model,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _model = model;
            _playerAnimator = playerAnimator;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            OnReelingHold
                .ResolveInputType()
                .Subscribe(x => _activeInputType = x)
                .AddTo(ref disposableBuilder);
            OnReelingFirstHold
                .Where(_ => !_isHolding)
                .Subscribe(x =>
                {
                    _activeInputType = x;
                    OnReelingFirstHeld();
                })
                .AddTo(ref disposableBuilder);
            OnReelingHold
                .Where(x => x == _activeInputType && _isHolding)
                .Subscribe(_ => OnReelingHeld())
                .AddTo(ref disposableBuilder);
            OnReelingRelease
                .Where(x => x == _activeInputType && _isHolding)
                .Subscribe(_ =>
                {
                    OnReelingReleased();
                    _activeInputType = null;
                })
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
        
        private void OnReelingFirstHeld()
        {
            _isHolding = true;
            _playerAnimator.Set(PlayerAnimationKey.Reeling, 0, true);
        }

        private void OnReelingHeld()
        {
            var reelingSpeed = (float)_model.FishingRodInstance.CurrentStats.CurrentReelingSpeed;
            var fishWeight = _model.FishInstance is null ? 0 : (float)_model.FishInstance.CurrentStats.CurrentFishWeight;
            var final = Mathf.Max(0, reelingSpeed - fishWeight);
            _model.CurrentReelingProgress.Value += final * Time.deltaTime;
        }

        private void OnReelingReleased()
        {
            _isHolding = false;
            _playerAnimator.Set(PlayerAnimationKey.IdleRod, 0, true);
        }

        public void Reset()
        {
            _isHolding = false;
            _activeInputType = null;
        }
    }
}