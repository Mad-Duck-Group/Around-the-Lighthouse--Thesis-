using System;
using Madduck.Fishing.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;

namespace Madduck.Fishing.UI
{
    [Serializable]
    public class ThrowHookModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<UFloat> ThrowHookMaxValue { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<UFloat> ThrowHookCurrentValue { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<bool> HookThrown { get; private set; }
        public ReadOnlyReactiveProperty<Percentage> ThrowHookPercent { get; private set; }
        
        private readonly ThrowHookConfig _config;
        private IDisposable _bindings;
        
        public ThrowHookModel(ThrowHookConfig config)
        {
            _config = config;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            ThrowHookMaxValue = new SerializableReactiveProperty<UFloat>(_config.ThrowHookMaxValue)
                .AddTo(ref disposableBuilder);
            ThrowHookCurrentValue = new SerializableReactiveProperty<UFloat>(0f)
                .AddTo(ref disposableBuilder);
            HookThrown = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            ThrowHookPercent = ThrowHookCurrentValue
                .CombineLatest(ThrowHookMaxValue, (current, max) => max <= 0 
                    ? Percentage.FromFraction(0f) 
                    : Percentage.FromFraction(Mathf.Clamp01(current / max)))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Reset()
        {
            ThrowHookCurrentValue.Value = 0f;
            ThrowHookMaxValue.Value = _config.ThrowHookMaxValue;
            HookThrown.Value = false;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}