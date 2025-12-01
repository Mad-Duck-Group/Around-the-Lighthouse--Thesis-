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
    
    public enum RoomHistoryState 
    {
        Past,
        Future     
    }

    public enum SelectionIcon
    {
        Unselected,
        Selected,
    }
    public enum PointingDirection
    {
        None,
        Left,
        Right
    }
    public enum InputIconType
    {
        X,
        A,
        B,
        Y,
        Lb,
        Rb,
        Lt,
        Rt,
        AnalogLeft,
        AnalogRight,
        Dpad,
        Reeling,
        TugOfWar
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
        PrepareBait = 1,
        ThrowHook = 2,
        ThrowingHook = 3,
        Nibble = 4,
        FishingBoard = 5,
        Reeling = 6,
        TugOfWar = 7,
        CatchFish = 8,
    }

    public enum BubbleType
    {
        None,
        Standard,
        Special
    }
}