namespace Madduck.GameData
{
    public class ResourceItemInstance : ItemInstance<ResourceItemData>, IFishableItemInstance
    {
        public ResourceItemInstance(ResourceItemData itemData, uint count = 1) : base(itemData, count)
        {
        }
    }
}