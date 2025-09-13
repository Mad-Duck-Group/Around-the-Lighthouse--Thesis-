using System;
using R3;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.Nibble
{
    public class NibbleViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsActive { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsNibbling { get; private set; }
        
        private readonly NibbleModel _model;
        private IDisposable _bindings;
        
        [Inject]
        public NibbleViewModel(NibbleModel model)
        {
            _model = model;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            IsActive = _model.IsActive.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            IsNibbling = _model.IsNibbling.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}