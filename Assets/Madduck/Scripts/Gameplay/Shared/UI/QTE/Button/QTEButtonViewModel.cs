using System;
using Madduck.Utils;
using R3;
using UnityEngine.InputSystem;
using VContainer;

namespace Madduck.Shared
{
    public class QTEButtonViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<string> ButtonName { get; private set; }
        public ReadOnlyReactiveProperty<Percentage> Remaining { get; private set; }
        
        private readonly QTEButtonController _controller;
        private IDisposable _bindings;
        
        [Inject]
        public QTEButtonViewModel(QTEButtonController controller)
        {
            _controller = controller;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            ButtonName = _controller.CurrentBinding
                .Select(x => x.ToDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions))
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            Remaining = _controller.RemainingPercentage
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