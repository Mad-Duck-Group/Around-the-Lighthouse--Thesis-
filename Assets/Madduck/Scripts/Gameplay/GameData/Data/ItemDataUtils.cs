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
}