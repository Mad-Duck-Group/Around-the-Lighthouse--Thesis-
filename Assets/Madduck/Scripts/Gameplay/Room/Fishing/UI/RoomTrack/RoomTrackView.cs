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
         SerializeField] private RectTransform _RoomRectTransform;
        
        #region SetUp
        public void SetUp(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        #endregion
    }
}
