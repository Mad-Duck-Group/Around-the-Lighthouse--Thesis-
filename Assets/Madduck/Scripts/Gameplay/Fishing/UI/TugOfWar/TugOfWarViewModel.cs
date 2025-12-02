using System;
using Madduck.Input;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class TugOfWarViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<Percentage> TugOfWarPercent { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsTugButtonDown { get; private set; }
        private readonly ReactiveProperty<bool> _isTugButtonDown = new(false);
        public ReadOnlyReactiveProperty<string> CurrentScheme { get; private set; } 

        private readonly TugOfWarModel _model;
        private readonly IPlayerInputHandler _inputHandler;
        private IDisposable _bindings;

        [Inject]
        public TugOfWarViewModel(
            TugOfWarModel model,
            IPlayerInputHandler inputHandler)
        {
            _model = model;
            _inputHandler = inputHandler;
            Bind();
        }

        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            TugOfWarPercent = _model.TugOfWarPercent
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            CurrentScheme = Observable.FromEvent<string>(
                        h => _inputHandler.OnControlSchemeChanged += h,
                        h => _inputHandler.OnControlSchemeChanged -= h)
                    .ToReadOnlyReactiveProperty(_inputHandler.CurrentControlScheme)
                    .AddTo(ref disposableBuilder);
            _model.IsTugButtonDown
                .Subscribe(OnTugButtonDown)
                .AddTo(ref disposableBuilder);
            IsTugButtonDown = _isTugButtonDown
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
            
        }

        private void OnTugButtonDown(bool isDown)
        {
            _isTugButtonDown.Value = isDown;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}