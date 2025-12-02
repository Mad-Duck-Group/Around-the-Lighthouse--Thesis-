using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class RoomTrackView : MonoBehaviour
    {
        
        [Title("References")]
        [Required, 
         SerializeField] private Image icon;
        [Required,
         SerializeField] private Image aura;
        
        #region SetUp
        public void SetUp(Sprite iconSprite,Sprite spriteAura)
        {
            icon.sprite = iconSprite;
            aura.sprite = spriteAura;
        }

        #endregion
    }
}
