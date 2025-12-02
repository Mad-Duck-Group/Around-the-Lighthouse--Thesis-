namespace Madduck.GameData
{
    public readonly struct WeatherChangedEvent
    {
        public WeatherItemInstance Weather { get; }
        public WeatherChangedEvent(WeatherItemInstance weather) => Weather = weather;
    }
}