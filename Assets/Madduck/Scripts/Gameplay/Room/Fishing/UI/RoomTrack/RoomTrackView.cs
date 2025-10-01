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
        
        #region SetUp
        public void SetUp(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        #endregion
    }
}
