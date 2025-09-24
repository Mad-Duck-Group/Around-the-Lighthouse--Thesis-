using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class WeatherHUDView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private Image icon;
        
        public void SetWeatherIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }
    }
}
