using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Shared
{
    public class QteSequenceView : MonoBehaviour, IQteElement
    {
        [Title("References")]
        [Required,
         SerializeField] private CanvasGroup canvasGroup;
        [Required,
         SerializeField] private LayoutGroup layoutGroup;
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> alphaTweenSettings;
        
        private Sequence _transitionSequence;
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            canvasGroup.alpha = alphaTweenSettings.startValue;
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            await Transition(false);
        }
        
        private async UniTask Transition(bool forward)
        {
             _transitionSequence = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaTweenSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }

        public async UniTask OnSuccess(CancellationToken cancellationToken = default)
        {
            await UniTask.CompletedTask;
        }

        public async UniTask OnFail(CancellationToken cancellationToken = default)
        {
            await UniTask.CompletedTask;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void SetAsChild(IQteElement child)
        {
            child.SetParent(layoutGroup);
        }
    }
}