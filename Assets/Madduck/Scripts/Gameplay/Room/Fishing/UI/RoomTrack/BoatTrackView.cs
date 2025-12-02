using Cysharp.Threading.Tasks;
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
        
        public async UniTask AnimateBoatTrack(Vector3 targetWorldPos)
        {
            DebugUtils.Log($"Animate Boat Track to {targetWorldPos}");
            var boatUI = (RectTransform)transform;
            var roomPosInParent = (Vector2)boatUI.parent.InverseTransformPoint(targetWorldPos);
            _boatTrackSequence.Stop();
            _boatTrackSequence = Sequence.Create()
                .Group(Tween.UIAnchoredPosition(boatUI, roomPosInParent, moveBoatTweenSettings))
                .Group(Tween.LocalRotation(boatUI, Quaternion.Euler(rotateAngle), rotateBoatTweenSettings))
                .Chain(Tween.LocalRotation(boatUI, Quaternion.Euler(-rotateAngle), rotateBoatTweenSettings))
                .Chain(Tween.LocalRotation(boatUI, Quaternion.identity, rotateBoatTweenSettings));
            await _boatTrackSequence.ToYieldInstruction().ToUniTask();
        }
        #endregion
        
    }
}
