using System;
using Madduck.Scripts.Fishing.Config.ThrowHook;
using R3;
using UnityEngine;

namespace Madduck.Scripts.Fishing.UI.ThrowHook
{
    [Serializable]
    public class ThrowHookModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<bool> IsActive { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<float> ThrowHookMaxValue { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<float> ThrowHookCurrentValue { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<bool> HookThrown { get; private set; }
        public ReadOnlyReactiveProperty<float> ThrowHookPercent { get; private set; }
        
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
            IsActive = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            ThrowHookMaxValue = new SerializableReactiveProperty<float>(_config.ThrowHookMaxValue)
                .AddTo(ref disposableBuilder);
            ThrowHookCurrentValue = new SerializableReactiveProperty<float>(0f)
                .AddTo(ref disposableBuilder);
            HookThrown = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            ThrowHookPercent = ThrowHookCurrentValue
                .CombineLatest(ThrowHookMaxValue, (current, max) => max <= 0 ? 0f : Mathf.Clamp01(current / max))
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