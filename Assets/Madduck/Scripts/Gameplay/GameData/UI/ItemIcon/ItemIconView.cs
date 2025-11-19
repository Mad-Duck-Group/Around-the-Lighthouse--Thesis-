using System;
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
         SerializeField] private Image qualityStarImage;
        [Required, 
         SerializeField] private Animator qualityStarAnimator;
        [Required, 
         SerializeField] private TMP_Text itemNameText;
        
        public void SetItem(IFishableItemInstance itemInstance)
        {
            switch (itemInstance)
            {
                case FishItemInstance fishItemInstance:
                    iconImage.sprite = fishItemInstance.ItemData.Icon;
                    itemNameText.text = fishItemInstance.ItemData.FishName;
                    qualityStarImage.enabled = true;
                    AnimateQualityStar(fishItemInstance.CurrentFishQuality);
                    break;
                case ResourceItemInstance resourceItemInstance:
                    iconImage.sprite = resourceItemInstance.ItemData.Icon;
                    itemNameText.text = resourceItemInstance.ItemData.ResourceName;
                    qualityStarImage.enabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemInstance), itemInstance, null);
            }
        }
        
        public void SetOffset(Vector2 offset)
        {
            elementParent.anchoredPosition += offset;
        }

        private void AnimateQualityStar(FishQuality fishQuality)
        {
            switch (fishQuality)
            {
                case FishQuality.Common:
                    qualityStarAnimator.Play("Copper");
                    break;
                case FishQuality.Good:
                    qualityStarAnimator.Play("Silver");
                    break;
                case FishQuality.Premium:
                    qualityStarAnimator.Play("Gold");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fishQuality), fishQuality, null);
            }
        }
    }
}