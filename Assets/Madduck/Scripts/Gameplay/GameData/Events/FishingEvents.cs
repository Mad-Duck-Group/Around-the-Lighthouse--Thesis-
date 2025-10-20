using Madduck.Shared;

namespace Madduck.GameData
{
    public struct FishingRoomStartedEvent{ }
    public struct FishCaughtEvent
    {
        public FishItemInstance FishItemInstance { get; private set; }

        public FishCaughtEvent(FishItemInstance fishItemInstance)
        {
            FishItemInstance = fishItemInstance;
        }
    }
    public struct FishEscapedEvent { }

    public struct FishingStateEvent
    {
        public FishingStateType StateType { get; private set; }
        public FishingStateEvent(FishingStateType stateType)
        {
            StateType = stateType;
        }
    }
    public struct FishingRoomEndedEvent { }
    
}