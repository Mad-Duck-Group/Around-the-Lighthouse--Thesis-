using System;
using Madduck.GameData;
using MessagePipe;
using R3;
using VContainer;

namespace Madduck.Room
{
    public class WeatherHUDViewModel : IDisposable
    {
        public ReactiveProperty<WeatherItemInstance> CurrentWeather { get; } = new();
        
        private readonly ISubscriber<WeatherChangedEvent> _weatherChangedSubscriber;
        private IDisposable _bindings;
        
        [Inject]
        public WeatherHUDViewModel( 
            ISubscriber<WeatherChangedEvent> weatherChangedSubscriber)
        {
            _weatherChangedSubscriber = weatherChangedSubscriber;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _weatherChangedSubscriber
                .Subscribe(e => OnWeatherChanged(e.Weather))
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnWeatherChanged(WeatherItemInstance newWeather)
        {
            CurrentWeather.Value = newWeather;
        }
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}
