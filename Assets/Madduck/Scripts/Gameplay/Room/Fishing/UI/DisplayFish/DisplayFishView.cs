using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room.DisplayFish
{
    public class DisplayFishView : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private TMP_Text fishNameText;
        [SerializeField] private TMP_Text fishDescriptionText;
        [SerializeField] private TMP_Text fishWeightText;
        [SerializeField] private TMP_Text fishRarityText;
        [SerializeField] private Image fishIcon;
    }
}