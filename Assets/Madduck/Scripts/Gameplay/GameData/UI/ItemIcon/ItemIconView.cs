using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.GameData
{
    public class ItemIconView : MonoBehaviour
    {
        [Title("References")]
        [Required,
         SerializeField] private RectTransform elementParent;
        [Required, 
         SerializeField] private Image iconImage;
        [Required,
        SerializeField] private TMP_Text itemNameText;
        
        public void SetItem(IItemIconData itemIconData)
        {
            iconImage.sprite = itemIconData.Icon;
            itemNameText.text = itemIconData.Name;
        }
        
        public void SetOffset(Vector2 offset)
        {
            elementParent.anchoredPosition += offset;
        }
    }
}