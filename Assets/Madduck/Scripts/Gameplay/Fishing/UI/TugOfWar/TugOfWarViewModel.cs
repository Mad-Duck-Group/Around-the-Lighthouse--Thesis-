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
        private ReactiveProperty<bool> _isTugButtonDown = new(false);
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
            CurrentScheme = _inputHandler.CurrentControlScheme
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _model.IsTugButtonDown
                .Subscribe(isDown =>
                {
                    OnTugButtonDown(isDown) ;
                })
                .AddTo(ref disposableBuilder);
            IsTugButtonDown = _isTugButtonDown
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
            
        }

        private void OnTugButtonDown(bool isDown)
        {
            _isTugButtonDown.Value = isDown;
            if (isDown)
            {
                DebugUtils.Log("Tug Button Down");
            }
            else
            {
                DebugUtils.Log("Tug Button Up");
            }
        }
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}