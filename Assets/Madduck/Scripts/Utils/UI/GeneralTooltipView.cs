using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Utils
{
    public class GeneralTooltipView : MonoBehaviour, ITooltipView<GeneralTooltipObject>, ITransitionable
    {
        [Title("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> alphaTweenSettings;
        
        private Sequence _transitionSequence;
        
        public async UniTask ShowTooltip(GeneralTooltipObject tooltip, CancellationToken cancellationToken = default)
        {
            titleText.text = tooltip.Title;
            descriptionText.text = tooltip.Description;
            await TransitionIn(cancellationToken);
        }

        public async UniTask HideTooltip(CancellationToken cancellationToken = default)
        {
            await TransitionOut(cancellationToken);
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(StopTransition);
            canvasGroup.alpha = 0f;
            _transitionSequence = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaTweenSettings.WithDirection(true)));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(StopTransition);
            canvasGroup.alpha = 1f;
            _transitionSequence = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaTweenSettings.WithDirection(false)));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        private void StopTransition()
        {
            _transitionSequence.Complete();
        }
    }
}
