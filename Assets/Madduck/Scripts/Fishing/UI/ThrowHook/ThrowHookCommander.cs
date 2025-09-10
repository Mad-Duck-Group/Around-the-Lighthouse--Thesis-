using System;
using Madduck.Scripts.Fishing.Config.ThrowHook;
using Madduck.Scripts.Utils.Others;
using R3;
using UnityEngine;

namespace Madduck.Scripts.Fishing.UI.ThrowHook
{
    public class ThrowHookCommander : IDisposable
    {
        public ReactiveCommand ThrowHookHeldCommand { get; private set; }
        public ReactiveCommand ThrowHookReleaseCommand { get; private set; }
        private readonly ThrowHookModel _model;
        private readonly ThrowHookConfig _config;
        private Sign _throwHookSliderDirection = Sign.Positive;
        private IDisposable _bindings;
        
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
            ThrowHookHeldCommand = new ReactiveCommand(_ => OnThrowHookHeld())
                .AddTo(ref disposableBuilder);
            ThrowHookReleaseCommand = new ReactiveCommand(_ => OnThrowHookReleased())
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