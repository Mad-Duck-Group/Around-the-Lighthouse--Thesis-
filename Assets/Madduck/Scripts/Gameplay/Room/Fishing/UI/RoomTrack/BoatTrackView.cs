using Madduck.Utils;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class BoatTrackView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        
        private Sequence _boatTrackSequence;
        
        #region SetUp
        public void SetUp(Sprite sprite)
        {
            icon.sprite = sprite;
        }
        
        public void AnimateBoatTrack(Vector3 targetWorldPos)
        {
            var boatUI = (RectTransform)transform;
            var roomPosInParent = (Vector2)boatUI.parent.InverseTransformPoint(targetWorldPos);
            _boatTrackSequence.Stop();
            _boatTrackSequence = Sequence.Create()
                .Group(Tween.UIAnchoredPosition(boatUI, roomPosInParent, 2f, Ease.Linear))
                .Group(Tween.LocalRotation(boatUI, Quaternion.Euler(0f, 0f, 10f), 2f, Ease.InOutSine));
            _boatTrackSequence.OnComplete(() =>
            {
                boatUI.rotation = Quaternion.identity;
            });
        }
        #endregion
    }
}
