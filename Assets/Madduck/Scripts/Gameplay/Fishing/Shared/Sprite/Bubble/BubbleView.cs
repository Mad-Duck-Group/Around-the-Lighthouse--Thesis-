using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Shared
{
    public interface IBubbleView : ITransitionable
    {
        BubbleType BubbleType { get; }
        float BubbleLength { get; }
        
        void SetUp(Vector2 position, BubbleType bubbleType);
    }
    public class BubbleView : MonoBehaviour, IBubbleView
    {
        [Title("References")]
        [Required,
         SerializeField] private SpriteRenderer bubbleSpriteRenderer;
        [Required,
         SerializeField] private SpriteRenderer fishShadowSpriteRenderer;
        
        [Title("Settings")]
        [SerializeField] private Vector2 lengthRange;
        
        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector2> relativePositionTweenSettings;
        [SerializeField] private TweenSettings<float> fadeOutTweenSettings;

        private TweenSettings<Vector2> _bubblePositionTweenSettings;
        private TweenSettings<Vector2> _fishShadowPositionTweenSettings;
        
        [Title("Debug")]
        [Button("Preview Transition")]
        private void PreviewTransitionIn(bool active)
        {
            if (active)
            {
                TransitionIn().Forget();
            }
            else
            {
                TransitionOut().Forget();
            }
        }
        
        public BubbleType BubbleType { get; private set; }

        public float BubbleLength
        {
            get
            {
                var spriteTransform = bubbleSpriteRenderer.transform;
                var left = (Vector2)spriteTransform.position - lengthRange.x * Vector2.left;
                var right = (Vector2)spriteTransform.position + lengthRange.y * Vector2.right;
                return Vector3.Distance(left, right);
            }
        }
        
        private Sequence _transitionSequence;
        
        
        public void SetUp(Vector2 position, BubbleType bubbleType)
        {
            transform.position = position;
            BubbleType = bubbleType;
            _bubblePositionTweenSettings = relativePositionTweenSettings.ToRelative(bubbleSpriteRenderer.transform.localPosition);
            _fishShadowPositionTweenSettings = relativePositionTweenSettings.ToRelative(fishShadowSpriteRenderer.transform.localPosition);
        }
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            bubbleSpriteRenderer.transform.localPosition = _bubblePositionTweenSettings.startValue;
            fishShadowSpriteRenderer.transform.localPosition = _fishShadowPositionTweenSettings.startValue;
            cancellationToken.Register(CancelTransition);
            _transitionSequence = Sequence.Create()
                .Group(Tween.LocalPosition(bubbleSpriteRenderer.transform, _bubblePositionTweenSettings.ToVector3()))
                .Group(Tween.LocalPosition(fishShadowSpriteRenderer.transform, _fishShadowPositionTweenSettings.ToVector3()));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            _transitionSequence = Sequence.Create()
                .Group(Tween.Alpha(bubbleSpriteRenderer, fadeOutTweenSettings))
                .Group(Tween.Alpha(fishShadowSpriteRenderer, fadeOutTweenSettings));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
            Destroy(gameObject);
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }

        private void OnDrawGizmosSelected()
        {
            var spriteTransform = bubbleSpriteRenderer.transform;
            if (!spriteTransform) return;
            var left = (Vector2)spriteTransform.position - lengthRange.x * Vector2.left;
            var right = (Vector2)spriteTransform.position + lengthRange.y * Vector2.right;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(left, right);
        }
    }
}