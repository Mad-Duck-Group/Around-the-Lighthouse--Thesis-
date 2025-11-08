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
        
        [Title("Settings")]
        [SerializeField] private Vector2 lengthRange;
        
        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        
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
        }
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            bubbleSpriteRenderer.transform.localScale = scaleTweenSettings.startValue;
            cancellationToken.Register(CancelTransition);
            await Transition(forward: true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(forward: false);
            Destroy(gameObject);
        }

        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(bubbleSpriteRenderer.transform, scaleTweenSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
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