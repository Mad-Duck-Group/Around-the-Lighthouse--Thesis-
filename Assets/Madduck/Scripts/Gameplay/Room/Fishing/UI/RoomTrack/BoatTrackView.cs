using Madduck.Core;
using Madduck.Utils;
using MessagePipe;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room
{
    public class BoatTrackView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TweenSettings moveBoatTweenSettings;
        [SerializeField] private TweenSettings rotateBoatTweenSettings;
        [SerializeField] private Vector3 rotateAngle = new(0f, 0f, 10f);
        private Sequence _boatTrackSequence;
        private IPublisher<LoadingSceneAnimationFinishedEvent> _loadingSceneAnimationFinishedPublisher;
        
        [Inject]
        public void Initialize(IPublisher<LoadingSceneAnimationFinishedEvent> loadingSceneAnimationFinishedPublisher)
        {
            _loadingSceneAnimationFinishedPublisher = loadingSceneAnimationFinishedPublisher;
        }
        
        
        #region SetUp
        public void SetUp(Sprite sprite)
        {
            icon.sprite = sprite;
        }
        
        public void AnimateBoatTrack(Vector3 targetWorldPos, bool shouldNotify = false)
        {
            DebugUtils.Log($"Animate Boat Track to {targetWorldPos}");
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
                .Group(rotateTween).OnComplete(() => 
                {
                    if (shouldNotify)
                    {
                        GlobalMessagePipe.GetPublisher<LoadingSceneAnimationFinishedEvent>()
                            .Publish(new LoadingSceneAnimationFinishedEvent());
                    }
                });
            
            
            
        }
        #endregion
    }
}
