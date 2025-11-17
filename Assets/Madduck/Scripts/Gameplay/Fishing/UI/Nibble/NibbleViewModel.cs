using System;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class NibbleViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsNibbling { get; private set; }
        public ReadOnlyReactiveProperty<Percentage> CatchChange { get; private set; }
        
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
            IsNibbling = _model.IsNibbling
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            CatchChange = _model.CatchChance
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