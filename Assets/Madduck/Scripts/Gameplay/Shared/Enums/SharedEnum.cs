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