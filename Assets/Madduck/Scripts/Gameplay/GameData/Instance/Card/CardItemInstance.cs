using System;

namespace Madduck.GameData
{
    [Serializable]
    public class CardItemInstance : ItemInstance<CardItemData>
    {
        public CardItemInstance(CardItemData itemData) : base(itemData) { }
    }
}