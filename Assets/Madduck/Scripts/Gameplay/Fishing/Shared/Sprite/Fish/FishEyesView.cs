using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Madduck.Fishing.Shared
{
    public interface IFishEyesView : ITransitionable
    {
        void SetUp(Transform hook);
        UniTask Bite(CancellationToken cancellationToken = default);
    }
    public class FishEyesView : MonoBehaviour, IFishEyesView
    {
        [Title("References")]
        [Required,
         SerializeField] private SpriteRenderer spriteRenderer;
        [Required,
         SerializeField] private SpriteRenderer questionMarkSpriteRenderer;
        
        [Title("Settings")] 
        [SerializeField] private Vector2 spawnOffset;
        [SerializeField] private Vector2 biteOffset;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector2> positionTweenSettings;
        [SerializeField] private TweenSettings<Vector3> questionMarkScaleTweenSettings;
        [SerializeField] private TweenSettings<Vector2> biteTransitionOutSettings;

        private Transform _hook;
        private TweenSettings<Vector3> _relativeSettings; 
        private Sequence _transitionSequence;
        private Sequence _questionMarkSequence;
        private Sequence _biteSequence;
        
        public void SetUp(Transform hook)
        {
            _hook = hook;
            transform.position = hook.position + (Vector3)spawnOffset;
            _relativeSettings = positionTweenSettings.ToVector3().ToRelative(transform.position);
            questionMarkSpriteRenderer.transform.localScale = questionMarkScaleTweenSettings.startValue;
        }
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(true);
            QuestionMark(true, cancellationToken).Forget();
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            QuestionMark(false, cancellationToken).Forget();
            await Transition(false);
        }

        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Position(transform, _relativeSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }

        private async UniTask QuestionMark(bool active, CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(() => _questionMarkSequence.Complete());
            _questionMarkSequence = Sequence.Create()
                .Group(Tween.Scale(questionMarkSpriteRenderer.transform, questionMarkScaleTweenSettings.WithDirection(active)));
            await _questionMarkSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask Bite(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(() => _biteSequence.Complete());
            QuestionMark(false, cancellationToken).Forget();
            var bitePosition = _hook.position + (Vector3)biteTransitionOutSettings.endValue;
            _biteSequence = Sequence.Create()
                .Chain(Tween.Position(transform, bitePosition, biteTransitionOutSettings.settings));
            await _biteSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }
    }
}