using System;

namespace Madduck.Shared
{
    [Flags]
    public enum DayPhaseType
    {
        Day = 1 << 0,
        Night = 1 << 1,
        Both = Day | Night
    }
    
    [Flags]
    public enum WeatherType
    {
        Clear = 1 << 0,
        Rain = 1 << 1,
        Fog = 1 << 2,
        All = Clear | Rain | Fog
    }

    public enum RoomType
    {
        Fishing,
        Shop,
        Restaurant,
        Event,
    }
    
    public enum FishingStateType
    {
        None = 0,
        ThrowHook = 1,
        ThrowingHook = 2,
        Nibble = 3,
        FishingBoard = 4,
        Reeling = 5,
    }
}