using System;

namespace Madduck.GameData.Bait
{
    [Serializable]
    public class BaitItemInstance : ItemInstance<BaitItemData>
    {
        public BaitItemInstance(BaitItemData itemData, uint count) : base(itemData, count)
        {
        }
    }
}