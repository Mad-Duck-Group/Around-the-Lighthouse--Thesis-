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
    
    public enum RoomHistoryState {
        Past,
        Future     
    }

    public enum SelectionIcon
    {
        Unselected,
        Selected,
    }
    public enum IconAnimationEvent
    {
        TugOfWar ,
        Reeling
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
    
    public enum InputControllerDevice
    {
        MouseKeyboard,
        Gamepad,
        Touch
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