using System;
using System.Linq;
using Madduck.Scripts.Fishing.Config.ThrowHook;
using Madduck.Scripts.Input;
using Madduck.Scripts.Utils.Others;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.ThrowHook
{
    public class ThrowHookCommander : IDisposable
    {
        public ReactiveCommand<InputType> ThrowHookHeldCommand { get; private set; }
        public ReactiveCommand<InputType> ThrowHookReleaseCommand { get; private set; }
        private readonly ThrowHookModel _model;
        private readonly ThrowHookConfig _config;
        private InputType _activeInputType;
        private Sign _throwHookSliderDirection = Sign.Positive;
        private IDisposable _bindings;
        
        [Inject]
        public ThrowHookCommander(
            ThrowHookModel model, 
            ThrowHookConfig config)
        {
            _model = model;
            _config = config;
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
            ThrowHookHeldCommand
                .Where(x=> x == _activeInputType && !_model.HookThrown.Value)
                .Subscribe(_ => OnThrowHookHeld())
                .AddTo(ref disposableBuilder);
            ThrowHookReleaseCommand = new ReactiveCommand<InputType>();
            ThrowHookReleaseCommand
                .Where(x => x == _activeInputType && !_model.HookThrown.Value)
                .Subscribe(_ => OnThrowHookReleased())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnThrowHookHeld()
        {
            var currentValue = _model.ThrowHookCurrentValue.Value;
            var maxValue = _model.ThrowHookMaxValue.Value;
            if (currentValue >= maxValue && _throwHookSliderDirection is Sign.Positive)
            {
                _throwHookSliderDirection = Sign.Negative;
            }
            else if (currentValue <= 0 && _throwHookSliderDirection is Sign.Negative)
            {
                _throwHookSliderDirection = Sign.Positive;
            }
            _model.ThrowHookCurrentValue.Value += (int)_throwHookSliderDirection 
                                                  * (_config.ThrowHookSliderSpeed * Time.deltaTime);
        }
        
        private void OnThrowHookReleased()
        {
            _throwHookSliderDirection = Sign.Positive;
            _model.HookThrown.Value = true;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}