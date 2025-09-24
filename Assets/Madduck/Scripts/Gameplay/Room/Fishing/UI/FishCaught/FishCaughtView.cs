using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Madduck.Room
{
    public class FishCaughtView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private TextMeshProUGUI currentFishCaughtText;
        [Required,
         SerializeField] private TextMeshProUGUI maxFishCaughtText;

        public void SetFishCaught(uint current, uint max)
        {
            currentFishCaughtText.text = current.ToString();
            maxFishCaughtText.text = max.ToString();
        }
    }
}
