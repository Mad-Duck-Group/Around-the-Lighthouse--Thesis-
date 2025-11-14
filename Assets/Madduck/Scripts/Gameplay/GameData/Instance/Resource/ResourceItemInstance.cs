namespace Madduck.GameData
{
    public class ResourceItemInstance : ItemInstance<ResourceItemData>
    {
        public ResourceItemInstance(ResourceItemData itemData, uint count = 1) : base(itemData, count)
        {
        }
    }
}