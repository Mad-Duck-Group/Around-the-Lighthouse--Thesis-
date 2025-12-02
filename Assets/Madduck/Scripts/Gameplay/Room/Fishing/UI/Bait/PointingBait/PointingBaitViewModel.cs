using System;
using Madduck.Input;
using Madduck.Shared;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class PointingBaitViewModel : IDisposable
    {
        private readonly ReactiveProperty<bool> _leftPressed = new(false);
        public ReadOnlyReactiveProperty<bool> LeftPressed { get; private set; }
    
        private readonly ReactiveProperty<bool> _rightPressed = new(false);
        public ReadOnlyReactiveProperty<bool> RightPressed { get; private set; }
        private IDisposable _disposables;


        [Inject]
        public PointingBaitViewModel()
        {
            Bind();
        }
        
        private void Bind()
        {
            var builder = Disposable.CreateBuilder();
            LeftPressed = _leftPressed.ToReadOnlyReactiveProperty();
            RightPressed = _rightPressed.ToReadOnlyReactiveProperty();
            _disposables = builder.Build();
        }
        public void UpdateInput(float input)
        {
            _rightPressed.Value = input > 0;
            _leftPressed.Value  = input < 0;
        }
        
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
