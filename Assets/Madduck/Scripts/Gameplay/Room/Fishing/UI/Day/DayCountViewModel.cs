using System;
using Madduck.Day;
using R3;
using VContainer;

namespace Madduck.Room
{
    public class DayCountViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<uint> CurrentDayIndex { get; private set; }
        
        private readonly DayManager _dayManager;
        private IDisposable _bindings;
        
        [Inject]
        public DayCountViewModel(DayManager dayManager)
        {
            _dayManager = dayManager;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            CurrentDayIndex = _dayManager.CurrentDayIndex
                .ToReadOnlyReactiveProperty();
            _bindings = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}