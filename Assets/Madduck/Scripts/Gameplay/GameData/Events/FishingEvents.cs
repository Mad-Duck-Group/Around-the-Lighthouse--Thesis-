using System.Collections.Generic;
using Madduck.Shared;

namespace Madduck.GameData
{
    public struct FishingRoomStartedEvent{ }
    public struct FishableCaughtEvent
    {
        public List<IFishableItemInstance> FishableItemInstances { get; private set; }

        public FishableCaughtEvent(params IFishableItemInstance[] fishableItemInstances)
        {
            FishableItemInstances = new List<IFishableItemInstance>(fishableItemInstances);
        }
    }
    public struct FishEmergedEvent
    {
        public FishItemInstance FishItemInstance { get; private set; }

        public FishEmergedEvent(FishItemInstance fishableItemInstance)
        {
            FishItemInstance = fishableItemInstance;
        }
    }

    public struct FishEscapedEvent
    {
        public FishItemInstance FishItemInstance { get; private set; }

        public FishEscapedEvent(FishItemInstance fishableItemInstance)
        {
            FishItemInstance = fishableItemInstance;
        }
    }

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