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