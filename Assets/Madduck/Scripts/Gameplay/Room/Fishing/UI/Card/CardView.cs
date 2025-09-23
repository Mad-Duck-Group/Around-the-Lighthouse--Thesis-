using Madduck.GameData;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class CardView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private Image icon;
        [Required, 
         SerializeField] private TMP_Text nameText;
        [Required, 
         SerializeField] private TMP_Text descriptionText;
        
        public CardItemInstance Card { get; private set; }

        public void SetCard(CardItemInstance card)
        {
            Card = card;
            icon.sprite = card.ItemData.CardIcon;
            nameText.text = card.ItemData.CardName;
            descriptionText.text = card.ItemData.CardDescription;
        }
    }
}