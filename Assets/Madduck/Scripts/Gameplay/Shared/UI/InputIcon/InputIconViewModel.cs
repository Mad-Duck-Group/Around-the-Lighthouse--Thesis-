using System;
using Madduck.Input;
using R3;
using VContainer;

namespace Madduck.Shared
{
    public class InputIconViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<string> CurrentScheme { get; private set; } 
        private readonly IPlayerInputHandler _input;
        private IDisposable _disposables;
        
        [Inject]
        public InputIconViewModel(IPlayerInputHandler input)
        {
            _input = input;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            CurrentScheme = Observable.FromEvent<string>(
                    h => _input.OnControlSchemeChanged += h,
                    h => _input.OnControlSchemeChanged -= h)
                .ToReadOnlyReactiveProperty(_input.CurrentControlScheme)
                .AddTo(ref disposableBuilder);
            _disposables = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
    
}
