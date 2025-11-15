using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.GameData
{
    public class ItemIconView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private Image iconImage;
        
        public void SetItem(IItemIconData itemIconData)
        {
            iconImage.sprite = itemIconData.Icon;
        }
    }
}