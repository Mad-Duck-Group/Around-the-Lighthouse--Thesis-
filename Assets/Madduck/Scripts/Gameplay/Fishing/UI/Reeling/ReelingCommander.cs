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
        public ReactiveCommand<InputType> OnReelingFirstHold { get; private set; } = new();
        public ReactiveCommand<InputType> OnReelingHold { get; private set; } = new();
        public ReactiveCommand<InputType> OnReelingRelease { get; private set; } = new();
        
        private readonly ReelingModel _reelingModel;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        private InputType _activeInputType;
        private IDisposable _bindings;
        
        [Inject]
        public ReelingCommander(
            ReelingModel reelingModel,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _reelingModel = reelingModel;
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
                .Where(x => x == _activeInputType)
                .Subscribe(_ => OnReelingFirstHeld())
                .AddTo(ref disposableBuilder);
            OnReelingHold
                .Where(x => x == _activeInputType)
                .Subscribe(_ => OnReelingHeld())
                .AddTo(ref disposableBuilder);
            OnReelingRelease
                .Where(x => x == _activeInputType)
                .Subscribe(_ => OnReelingReleased())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
        
        private void OnReelingFirstHeld()
        {
            _playerAnimator.Set(PlayerAnimationKey.Reeling, 0, true);
        }

        private void OnReelingHeld()
        {
            var reelingSpeed = (float)_reelingModel.FishingRodInstance.CurrentStats.CurrentReelingSpeed;
            _reelingModel.CurrentReelingProgress.Value += reelingSpeed * Time.deltaTime;
        }

        private void OnReelingReleased()
        {
            _playerAnimator.Set(PlayerAnimationKey.IdleRod, 0, true);
        }
    }
}