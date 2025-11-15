using System;
using System.Collections.Generic;
using UnityEngine;

namespace Madduck.GameData
{
    public interface IItemIconData
    {
        string Name { get; }
        string Description { get; }
        Sprite Icon { get; }
    }
    
    public interface IFishableItemData { }
    
    public interface IFishableItemInstance { }

    public static class ItemDataUtils
    {
        public static Dictionary<Guid, IItemInstance> CombineCount(List<IItemInstance> items)
        {
            var combinedItems = new Dictionary<Guid, IItemInstance>();

            foreach (var item in items)
            {
                if (item.ItemData is not ItemData itemData) continue;
                if (!combinedItems.TryAdd(itemData.Guid, item))
                {
                    combinedItems[itemData.Guid].ChangeCurrentCount((int)item.CurrentCount);
                }
            }

            return combinedItems;
        }
    }
}