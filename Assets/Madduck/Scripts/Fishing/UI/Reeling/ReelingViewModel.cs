using System;
using R3;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.Reeling
{
    public class ReelingViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsActive { get; private set; }
        public ReadOnlyReactiveProperty<float> ReelingProgressPercent { get; private set; }

        private readonly ReelingModel _model;
        private IDisposable _bindings;
        
        [Inject]
        public ReelingViewModel(ReelingModel model)
        {
            _model = model;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            IsActive = _model.IsActive.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            ReelingProgressPercent = _model.CurrentReelingProgress
                .CombineLatest(_model.MaxReelingProgress, (current, max) => max == 0f ? 0f : current / max)
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