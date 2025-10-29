using System;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class TugOfWarViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<Percentage> TugOfWarPercent { get; private set; }
        
        private readonly TugOfWarModel _model;
        private IDisposable _bindings;

        [Inject]
        public TugOfWarViewModel(
            TugOfWarModel model)
        {
            _model = model;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            TugOfWarPercent = _model.TugOfWarPercent
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