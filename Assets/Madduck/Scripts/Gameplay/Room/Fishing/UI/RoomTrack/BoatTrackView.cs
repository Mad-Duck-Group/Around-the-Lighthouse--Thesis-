using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class BoatTrackView : MonoBehaviour
    {
        [SerializeField]private Image icon;
        [SerializeField] private RectTransform _boatRectTransform;
        #region SetUp
        public void SetUp(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        #endregion
    }
}
