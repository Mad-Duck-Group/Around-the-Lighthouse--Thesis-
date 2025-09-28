using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class CurrentRoomView : MonoBehaviour
    {
        
        [Title("References")]
        [Required, 
         SerializeField] private Image icon;
        
        #region SetUp
        public void SetUp()
        {
            icon.sprite = null;
        }

        #endregion
    }
}
