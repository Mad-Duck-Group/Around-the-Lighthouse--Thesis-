using System;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class ThrowHookViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<Percentage> ThrowHookPercentRelative { get; private set; }
        public ReadOnlyReactiveProperty<Percentage> LockedRangePercent { get; private set; }
        public ReadOnlyReactiveProperty<bool> ShowSlider { get; private set; }
        private readonly ThrowHookModel _model;
        private IDisposable _bindings;
        
        [Inject]
        public ThrowHookViewModel(ThrowHookModel model)
        {
            _model = model;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            ThrowHookPercentRelative = _model.ThrowHookCurrentValue
                .CombineLatest(_model.ThrowHookCurrentMaxValue, (current, max) => max <= 0 
                    ? Percentage.FromFraction(0f) 
                    : Percentage.FromFraction(Mathf.Clamp01(current / max)))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            LockedRangePercent = _model.ThrowHookCurrentMaxValue
                .CombineLatest(new ReactiveProperty<UFloat>(Percentage.Full.AsPercentage), (current, max) => max <= 0 
                    ? Percentage.FromFraction(0f) 
                    : Percentage.FromFraction(1 - Mathf.Clamp01(current / max)))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            ShowSlider = _model.HookThrownFirstHeld
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}