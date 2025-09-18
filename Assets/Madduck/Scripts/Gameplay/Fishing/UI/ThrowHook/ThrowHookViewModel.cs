using System;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class ThrowHookViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<Percentage> ThrowHookPercent { get; private set; }
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