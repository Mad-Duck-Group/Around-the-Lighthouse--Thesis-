namespace Madduck.GameData
{
    public class FishItemDisplay : ItemDisplay
    {
        public override void Initialize(ItemData itemData)
        {
            if (itemData is not FishItemData fishData) return;
            base.Initialize(itemData);
            spriteRenderer.sprite = fishData.FishSprite;
        }
    }
}
