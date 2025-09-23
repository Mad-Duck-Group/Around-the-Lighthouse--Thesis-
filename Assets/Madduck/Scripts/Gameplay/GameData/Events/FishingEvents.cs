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
    public struct OutOfFishEvent { }
    
}