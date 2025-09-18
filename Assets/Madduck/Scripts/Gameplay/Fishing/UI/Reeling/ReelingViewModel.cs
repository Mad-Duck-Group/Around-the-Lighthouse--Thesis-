using System;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class ReelingViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<Percentage> ReelingProgressPercent { get; private set; }

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
            ReelingProgressPercent = _model.CurrentReelingProgress
                .CombineLatest(_model.MaxReelingProgress, (current, max) => max == 0f 
                    ? Percentage.FromFraction(0f) 
                    : Percentage.FromFraction(Mathf.Clamp01(current / max)))
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