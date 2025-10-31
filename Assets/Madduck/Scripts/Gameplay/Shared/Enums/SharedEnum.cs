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
        Storm = 1 << 2,
        Cloudy = 1 << 3,
        StrongWinds = 1 << 4,
        All = Clear | Rain | Storm | Cloudy |StrongWinds
    }

    public enum WindDirection
    {
        Left,
        Middle,
        Right
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
        TugOfWar = 6,
        CatchFish = 7,
    }

    public enum BubbleType
    {
        None,
        Standard,
        Special
    }
}