using System;
using Madduck.Input;
using R3;
using VContainer;

namespace Madduck.Shared
{
    public class InputInstructionViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<InputInstruction[]> CurrentInstructions { get; private set; }
        public ReadOnlyReactiveProperty<string> CurrentScheme { get; private set; } 
        
        private readonly InputInstructionManager _instructionManager;
        private readonly IPlayerInputHandler _inputHandler;
        private IDisposable _bindings;

        [Inject]
        public InputInstructionViewModel(
            IPlayerInputHandler inputHandler,
            InputInstructionManager instructionManager)
        {
            _inputHandler = inputHandler;
            _instructionManager = instructionManager; 
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            CurrentInstructions = _instructionManager.CurrentInstructions
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