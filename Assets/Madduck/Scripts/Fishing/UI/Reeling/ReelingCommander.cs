using System;
using Madduck.Scripts.Input;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.Reeling
{
    public class ReelingCommander : IDisposable
    {
        public ReactiveCommand<InputType> OnReelingHold { get; private set; }
        
        private readonly ReelingModel _reelingModel;
        private InputType _activeInputType;
        private IDisposable _bindings;
        
        [Inject]
        public ReelingCommander(ReelingModel reelingModel)
        {
            _reelingModel = reelingModel;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            OnReelingHold = new ReactiveCommand<InputType>();
            OnReelingHold
                .ResolveInputType()
                .Subscribe(x => _activeInputType = x)
                .AddTo(ref disposableBuilder);
            OnReelingHold
                .Where(x => x == _activeInputType)
                .Subscribe(_ => OnReelingHeld())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }

        private void OnReelingHeld()
        {
            var reelingSpeed = _reelingModel.FishingRodInstance.CurrentReelingSpeed;
            _reelingModel.CurrentReelingProgress.Value += reelingSpeed * Time.deltaTime;
        }
    }
}