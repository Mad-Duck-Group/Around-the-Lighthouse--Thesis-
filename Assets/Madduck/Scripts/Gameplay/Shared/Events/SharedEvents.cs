namespace Madduck.Shared.Events
{
    public struct BaitSelectionActivationEvent
    {
        public bool isActive;
        
        public BaitSelectionActivationEvent(bool isActive)
        {
            this.isActive = isActive;
        }
    }
}