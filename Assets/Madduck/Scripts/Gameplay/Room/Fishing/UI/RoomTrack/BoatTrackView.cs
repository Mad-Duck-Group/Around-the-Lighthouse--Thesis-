using Madduck.Utils;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class BoatTrackView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TweenSettings moveBoatTweenSettings;
        [SerializeField] private TweenSettings rotateBoatTweenSettings;
        [SerializeField] private Vector3 rotateAngle = new(0f, 0f, 10f);
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
            var moveTween = Tween.UIAnchoredPosition(boatUI, roomPosInParent, moveBoatTweenSettings);

            var rotateTween = Tween.LocalRotation(boatUI, Quaternion.Euler(rotateAngle), rotateBoatTweenSettings);
            rotateTween.OnComplete(() =>
            {
                Tween.LocalRotation(boatUI, Quaternion.Euler(-rotateAngle), rotateBoatTweenSettings)
                    .OnComplete(() => Tween.LocalRotation(boatUI, Quaternion.identity, rotateBoatTweenSettings));
                    
            });
            _boatTrackSequence = Sequence.Create()
                .Group(moveTween)
                .Group(rotateTween);
        }
        #endregion
    }
}
