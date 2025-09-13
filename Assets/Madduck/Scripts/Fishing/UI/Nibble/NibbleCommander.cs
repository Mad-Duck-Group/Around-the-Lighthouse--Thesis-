using System;
using Madduck.Scripts.Utils.Others;
using R3;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.Nibble
{
    public class NibbleCommander : IDisposable
    {
        public ReactiveCommand<Unit> PullHookCommand { get; private set; }
        
        private readonly NibbleModel _model;
        private IDisposable _bindings;
        
        [Inject]
        public NibbleCommander(NibbleModel model)
        {
            _model = model;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            PullHookCommand = new ReactiveCommand<Unit>(_ => OnPullHook())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnPullHook()
        {
            _model.PullHookResult.Value = 
                _model.IsNibbling.Value ? Sign.Positive : Sign.Negative;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}