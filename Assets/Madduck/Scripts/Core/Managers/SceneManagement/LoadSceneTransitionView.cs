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
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> alphaTweenSettings;
        
        private Sequence _transitionSequence;
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            canvasGroup.blocksRaycasts = true;
            cancellationToken.Register(CancelTransition);
            _transitionSequence = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaTweenSettings.WithDirection(true)));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
            canvasGroup.blocksRaycasts = false;
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            canvasGroup.blocksRaycasts = true;
            cancellationToken.Register(CancelTransition);
            _transitionSequence = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaTweenSettings.WithDirection(false)));
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