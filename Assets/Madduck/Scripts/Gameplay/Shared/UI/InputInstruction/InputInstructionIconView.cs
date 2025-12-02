using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Shared
{
    public class InputInstructionIconView : MonoBehaviour
    {
        [Title("References")]
        [Required,
         SerializeField] private Image iconImage;
        [Required, 
         SerializeField] private TMP_Text descriptionText;
        
        public void SetUp(Sprite iconSprite, string description)
        {
            iconImage.sprite = iconSprite;
            descriptionText.text = description;
        }
    }
}