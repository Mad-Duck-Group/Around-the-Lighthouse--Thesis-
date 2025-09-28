using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Core
{
    public class LoadSceneTransitionView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform circleTransform;
        [Title("Tween")]
        [SerializeField] private TweenSettings tweenSettings;
        [SerializeField] private Vector2 startSize ; 
        [SerializeField] private Vector2 endSize ; 
        
        private Sequence _transitionSequence;
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            canvasGroup.blocksRaycasts = true;
            cancellationToken.Register(CancelTransition);
            circleTransform.sizeDelta = startSize;
            _transitionSequence = Sequence.Create()
                .Group(
                    Tween.UISizeDelta(circleTransform, endSize, tweenSettings));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
            canvasGroup.blocksRaycasts = false;
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            canvasGroup.blocksRaycasts = true;
            cancellationToken.Register(CancelTransition);
            _transitionSequence = Sequence.Create()
                .Group(
                    Tween.UISizeDelta(circleTransform, startSize, tweenSettings));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
            canvasGroup.blocksRaycasts = false;
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
            canvasGroup.blocksRaycasts = false;
        }
    }
}