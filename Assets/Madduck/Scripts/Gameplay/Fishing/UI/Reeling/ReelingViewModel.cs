using System;
using Madduck.Input;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class ReelingViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<Percentage> ReelingProgressPercent { get; private set; }
        public ReadOnlyReactiveProperty<string> CurrentScheme { get; private set; } 
        private readonly ReelingModel _model;
        private readonly IPlayerInputHandler _inputHandler;
        private IDisposable _bindings;
        
        [Inject]
        public ReelingViewModel(ReelingModel model,
            IPlayerInputHandler inputHandler)
        {
            _model = model;
            _inputHandler = inputHandler;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            ReelingProgressPercent = _model.ReelingPercent
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            CurrentScheme = Observable.FromEvent<string>(
                        h => _inputHandler.OnControlSchemeChanged += h,
                        h => _inputHandler.OnControlSchemeChanged -= h)
                    .ToReadOnlyReactiveProperty(_inputHandler.CurrentControlScheme)
                    .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}