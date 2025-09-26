namespace Madduck.Shared.Events
{
    public readonly struct WeatherChangedEvent
    {
        public WeatherType Weather { get; }
        public WeatherChangedEvent(WeatherType weather) => Weather = weather;
    }
}