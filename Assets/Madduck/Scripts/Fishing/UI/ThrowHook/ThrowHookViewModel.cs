using System;
using Madduck.Scripts.Fishing.Config.ThrowHook;
using R3;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.ThrowHook
{
    public class ThrowHookViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsActive { get; private set; }
        public ReadOnlyReactiveProperty<float> ThrowHookPercent { get; private set; }
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
            IsActive = _model.IsActive
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            ThrowHookPercent = _model.ThrowHookPercent
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